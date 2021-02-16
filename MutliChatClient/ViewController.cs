using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using MultiChatCore;
using MultiChatCore.model;

namespace MutliChatClient
{
    public partial class ViewController : NSViewController
    {
        private ChatListDataSource _chatListDataSource;
        private NetworkStream _ns;

        private void AddMessage(Message message)
        {
            if (_chatListDataSource.Messages.Count > 0 &&
                _chatListDataSource.Messages[0].content.Contains("No messages yet"))
            {
                _chatListDataSource.Messages.RemoveAt(0);
            }

            _chatListDataSource.Messages.Add(message);
            ChatList.ReloadData();
        }

        private async Task ReceiveData(TcpClient client)
        {
            while (true)
            {
                var readBytes = new byte[1];
                var messageContent = "";

                while (messageContent.IndexOf("@") < 0)
                {
                    var receivedBytes = await _ns.ReadAsync(readBytes, 0, readBytes.Length);
                    messageContent += Encoding.ASCII.GetString(readBytes, 0, receivedBytes);
                }

                var finalMessageContent = messageContent.Substring(0, messageContent.IndexOf("@"));

                var message = Serialization.Deserialize(finalMessageContent);

                AddMessage(message);
            }
        }

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            _chatListDataSource = new ChatListDataSource();
            ChatList.DataSource = _chatListDataSource;
            ChatList.Delegate = new ChatListDelegate(_chatListDataSource);

            var noMessages = new Message(
                MessageType.Info,
                "system",
                "No messages yet",
                DateTime.Now);

            AddMessage(noMessages);
        }

        async partial void Connect(NSObject sender)
        {
            var enteredPort = -1;
            var serverIPString = EnteredIPAddress.StringValue.Trim();
            IPAddress serverIP;

            // IP validation
            if (serverIPString == "")
            {
                UI.ShowAlert("Provide server IP",
                    "Please provide a server IP in order to connect to the server.");
                return;
            }

            if (IPAddress.TryParse(serverIPString, out serverIP) == false)
            {
                UI.ShowAlert("Invalid server ip",
                    "The IP Address you entered was invalid.");
                return;
            }

            // Port validation
            try
            {
                enteredPort = Int32.Parse(EnteredPort.StringValue.Trim());
            }
            catch (FormatException)
            {
                UI.ShowAlert("Invalid server port",
                    "The port you entered was invalid. Please make sure the port is a numeric value.");
                return;
            }

            if (enteredPort == -1)
            {
                UI.ShowAlert("Provide server port",
                    "Please provide a server port in order to connect to the server.");
                return;
            }

            try
            {
                using (var tcpClient = new TcpClient(serverIP.ToString(), enteredPort)
                )
                {
                    var connectedMessage = new Message(
                        MessageType.Info,
                        "system",
                        "Connected to the server",
                        DateTime.Now);
                    AddMessage(connectedMessage);

                    _ns = tcpClient.GetStream();
                    await ReceiveData(tcpClient);
                }
            }
            catch (SocketException socketException)
            {
                UI.ShowExceptionAlert("Server error", socketException);
            }
        }


        async partial void SendMessage(NSObject sender)
        {
            var messageContent = EnteredMessage.StringValue;

            if (!messageContent.Trim().Equals(""))
            {
                EnteredMessage.StringValue = "";
                var message = new Message(
                    MessageType.Info,
                    "Client",
                    messageContent,
                    DateTime.Now);
 
                await Messaging.SendMessage(message, _ns);
            }
        }
    }
}