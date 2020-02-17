using System.Windows;
using OmegleAPI;
using System.Collections.Generic;
using System.Windows.Media;

namespace OmegleSpyWPF
{
    /// <summary>
    /// Interaction logic for WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        private enum MessageIssuer{Stranger1, Stranger2}

        private readonly System.Boolean[] Connected = new System.Boolean[] { false, false };
        
        /// <summary>
        /// The two Clients this Software connects to spy on.
        /// </summary>
        private readonly Client[] OmegleStrangers = new Client[] { new Client(), new Client() };

        /// <summary>
        /// Keeps any messages that were sent before the recepient connected
        /// </summary>
        private readonly List<System.String>[] Buffer = new List<System.String>[] { new List<System.String>(), new List<System.String>() };
        
        public WindowMain()
        {
            //Hooking up the Event Handlers
            foreach (Client c in OmegleStrangers)
            {
                c.Error += new System.EventHandler(c_Error);
                c.StrangerConnected += new System.EventHandler(c_StrangerConnected);
                c.StrangerDisconnected += new System.EventHandler(c_StrangerDisconnected);
                c.StrangerSentMessage += new Client.StrangerSentMessageHandler(c_StrangerSentMessage);
                c.StrangerTypingStarted += new System.EventHandler(c_StrangerTypingStarted);
                c.StrangerTypingStopped += new System.EventHandler(c_StrangerTypingStopped);
            }
            
            InitializeComponent();
        }

        void c_StrangerTypingStopped(object sender, System.EventArgs e)
        {
            if (sender == OmegleStrangers[0])
            {
                this.Dispatcher.Invoke(new System.Action(() => { ChkbxS1Writing.IsChecked = false; }));
                OmegleStrangers[1].StopTyping();
            }
            if (sender == OmegleStrangers[1])
            {
                this.Dispatcher.Invoke(new System.Action(() => { ChkbxS2Writing.IsChecked = false; }));
                OmegleStrangers[0].StopTyping();
            }
        }

        void c_StrangerTypingStarted(object sender, System.EventArgs e)
        {
            if (sender == OmegleStrangers[0])
            {
                this.Dispatcher.Invoke(new System.Action(() => { ChkbxS1Writing.IsChecked = true; }));
                OmegleStrangers[1].StartTyping();
            }
            if (sender == OmegleStrangers[1])
            {
                this.Dispatcher.Invoke(new System.Action(() => { ChkbxS2Writing.IsChecked = true; }));
                OmegleStrangers[0].StartTyping();
            }
        }

        void c_StrangerSentMessage(object sender, string Message)
        {
            if (sender == OmegleStrangers[0])
            {
                if (Connected[1]) SendMessage(Message, MessageIssuer.Stranger1, false);
                else Buffer[1].Add(Message);
            }
            if (sender == OmegleStrangers[1])
            {
                if (Connected[0]) SendMessage(Message, MessageIssuer.Stranger2, false);
                else Buffer[0].Add(Message);
            }
        }

        void c_StrangerDisconnected(object sender, System.EventArgs e)
        {
            this.Dispatcher.Invoke(new System.Action(() => { Disconnect(); }));
        }

        void c_StrangerConnected(object sender, System.EventArgs e)
        {
            if (sender == OmegleStrangers[0])
            {
                this.Dispatcher.Invoke(new System.Action(() => { ChkbxS1Connected.IsChecked = true; }));
                Connected[0] = true;
                foreach (System.String Message in Buffer[0]) SendMessage(Message, MessageIssuer.Stranger2, false);
                Buffer[0].Clear();
            }
            if (sender == OmegleStrangers[1])
            {
                this.Dispatcher.Invoke(new System.Action(() => { ChkbxS2Connected.IsChecked = true; }));
                Connected[1] = true;
                foreach (System.String Message in Buffer[1]) SendMessage(Message, MessageIssuer.Stranger1, false);
                Buffer[1].Clear();
            }
        }

        void c_Error(object sender, System.EventArgs e)
        {
            MessageBox.Show("An Error occured. Try reconnecting and check if you are banned (by visting Omegles Website). If you are banned, try rebooting your Router or use a VPN.", "Error", MessageBoxButton.OK);
        }

        private void SendMessage(System.String Message, MessageIssuer Issuer, System.Boolean IsIntervention)
        {
            //Very unlikely that all of this has to run on the UI Thread, but it works for now
            this.Dispatcher.Invoke(new System.Action(() => {
                System.Boolean Success = false;
                System.Windows.Documents.Run ToScreen = new System.Windows.Documents.Run();
                if (IsIntervention) ToScreen.Text += "YOU as ";
                if (Issuer == MessageIssuer.Stranger1)
                {
                    Success = OmegleStrangers[1].SendMessage(Message);
                    ToScreen.Text += "Stranger 1: ";
                    if (IsIntervention) ToScreen.Foreground = new SolidColorBrush(Colors.Orange);
                    else ToScreen.Foreground = new SolidColorBrush(Colors.Red);
                }
                else if (Issuer == MessageIssuer.Stranger2)
                {
                    Success = OmegleStrangers[0].SendMessage(Message);
                    ToScreen.Text += "Stranger 2: ";
                    if (IsIntervention) ToScreen.Foreground = new SolidColorBrush(Colors.Purple);
                    else ToScreen.Foreground = new SolidColorBrush(Colors.Blue);
                }

                if (Success)
                {
                    ToScreen.Text += Message;
                    System.Windows.Documents.Paragraph p = new System.Windows.Documents.Paragraph(ToScreen);
                    TxtbxChat.Document.Blocks.Add(p);
                }
            }));
        }

        /// <summary>
        /// Ensures only Numbers get entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtbxAutosaveMinLength_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Disconnect()
        {
            foreach (Client c in OmegleStrangers) c.Disconnect();

            Connected[0] = false;
            Connected[1] = false;
            ChkbxS1Connected.IsChecked = false;
            ChkbxS1Writing.IsChecked = false;
            ChkbxS2Connected.IsChecked = false;
            ChkbxS2Writing.IsChecked = false;

            if (ChkbxActivateAutosave.IsChecked == true)
            {
                if (TxtbxAutosaveMinLength.Text.Length == 0) TxtbxAutosaveMinLength.Text = "0";
                if (TxtbxChat.Document.Blocks.Count >= System.Convert.ToInt32(TxtbxAutosaveMinLength.Text))
                {
                    SaveLog();
                }
            }
            if (ChkbxActivateAutoReconnect.IsChecked == true)
            {
                Connect();
            }
        }

        private void Connect()
        {           
            foreach (Client c in OmegleStrangers) c.ConnectChat(null, false);
            TxtbxChat.Document.Blocks.Clear(); ;
        }

        private void BtnReconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
            if(ChkbxActivateAutoReconnect.IsChecked == false) Connect();
        }

        private void BtnSaveLog_Click(object sender, RoutedEventArgs e)
        {
            SaveLog();
        }

        private void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            InterveneText();
        }

        private void TxtbxInterveneText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                InterveneText();
            }
        }
        
        /// <summary>
        /// Sends Text as Intervention
        /// </summary>
        private void InterveneText()
        {
            if (TxtbxInterveneText.Text.Trim().Length > 0)
            {
                if (CmbbxInterveneAs.SelectedIndex == 0) SendMessage(TxtbxInterveneText.Text.Trim(), MessageIssuer.Stranger1, true);
                else if (CmbbxInterveneAs.SelectedIndex == 1) SendMessage(TxtbxInterveneText.Text.Trim(), MessageIssuer.Stranger2, true);
                TxtbxInterveneText.Text = "";
            }
        }

        /// <summary>
        /// Saves the Chat Window Log
        /// </summary>
        private void SaveLog()
        {
            TxtbxChat.SelectAll();
            TxtbxChat.Selection.Save(new System.IO.FileStream(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.fff") + ".rtf"), System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), DataFormats.Rtf);
        }

        private void TxtbxChat_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TxtbxChat.ScrollToEnd();
        }
    }
}
