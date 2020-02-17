# OmegleSpyWPF
![A little man-in-the-middle Spying tool for Omegle](https://github.com/jeanluc162/OmegleSpyWPF/raw/master/OmegleSpyWPF.png)
# What can you do with it?
- Watch two strangers chatting without their knowledge
- Send a message as one of them that will only be seen by the other
- Save logs/configure an autosave for all chats that were longer than x messages
# Is this hacking?
No. This program works by connecting two clients to omegle at the same time using [Omegle-.NET](https://github.com/jeanluc162/Omegle-.NET) and forwarding all messages the first client receives to the second client and the other way around.
# Is this illegal?
Frankly, I don't know. Please refrain from doing evil with it. An important note: From omegles perspective, all messages going through this program are being sent by you at one point. Therefore, **you can get your IP banned because of a message written by one of the strangers**.
# Where does this Software run?
Windows XP with .NET 3.5 and up. You might get a warning on Windows 10 because I haven't signed the executeable. If you want to compile it yourself: I used Visual Studio 2008, but newer versions should work just as fine.
