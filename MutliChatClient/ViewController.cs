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

        private string _username;
        private int _bufferSize;
        
        private bool _isConnected;

        private void AddMessage(Message message)
        {
            if (_chatListDataSource.Messages.Count > 0 &&
                _chatListDataSource.Messages[0].content.Contains("No messages yet"))
            {
                _chatListDataSource.Messages.RemoveAt(0);
            }

            _chatListDataSource.Messages.Add(message);
            ChatList.ReloadData();
            ChatList.ScrollRowToVisible(_chatListDataSource.Messages.Count - 1);
        }

        private async Task ReceiveData(TcpClient client)
        {
            while (_isConnected)
            {
                var readBytes = new byte[_bufferSize];
                var messageContent = "";

                while (messageContent.IndexOf("@") < 0)
                {
                    var receivedBytes = await _ns.ReadAsync(readBytes, 0, readBytes.Length);
                    messageContent += Encoding.ASCII.GetString(readBytes, 0, receivedBytes);
                }

                var finalMessageContent = messageContent.Substring(0, messageContent.IndexOf("@"));

                var message = Serialization.Deserialize(finalMessageContent);

                AddMessage(message);

                if (message.type == MessageType.ServerStopped)
                {
                    _ns = null;

                    SetDisconnected();
                }
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

            SendButton.Enabled = false;
            EnteredBufferSize.StringValue = "1024";
        }

        async partial void Connect(NSObject sender)
        {
            var enteredUsername = EnteredName.StringValue.Trim();
            var serverIpString = EnteredIPAddress.StringValue.Trim();
            var enteredPort = -1;
            var enteredBufferSize = EnteredBufferSize.StringValue.Trim();

            if (enteredUsername == "")
            {
                UI.ShowAlert("Provide username",
                    "Please provide a username in order to connect to the server.");
                return;
            }

            _username = enteredUsername;

            // IP validation
            if (serverIpString == "")
            {
                UI.ShowAlert("Provide server IP",
                    "Please provide a server IP in order to connect to the server.");
                return;
            }

            if (IPAddress.TryParse(serverIpString, out var serverIp) == false)
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
            
            // Buffersize validation
            try
            {
                _bufferSize = Int32.Parse(enteredBufferSize);
            }
            catch (FormatException)
            {
                UI.ShowAlert("Invalid server buffer size",
                    "The buffer size you entered is likely not a number. Please enter a valid number.");
                return;
            }

            if (enteredBufferSize == "" || _bufferSize <= 0)
            {
                UI.ShowAlert("Provide buffer size",
                    "Please provide a positive buffer size in order to connect to the server.");
                return;
            }

            if (!_isConnected)
            {
                await ConnectToServer(serverIp, enteredPort, enteredBufferSize);
            }
            else
            {
                await DisconnectFromServer();
            }
        }

        private async Task DisconnectFromServer()
        {
            var message = new Message(
                MessageType.Disconnect,
                _username,
                "",
                DateTime.Now);

            await Messaging.SendMessage(message, _ns);

            var endedMessage = new Message(
                MessageType.Info,
                "system",
                "Disconnected from the server",
                DateTime.Now);
            AddMessage(endedMessage);

            _ns = null;
            SetDisconnected();
        }

        private async Task ConnectToServer(IPAddress serverIp, int enteredPort, string bufferSize)
        {
            try
            {
                using (var tcpClient = new TcpClient(serverIp.ToString(), enteredPort)
                )
                {
                    SetConnected();
                    var connectedMessage = new Message(
                        MessageType.Info,
                        "system",
                        "Connected to the server",
                        DateTime.Now);
                    AddMessage(connectedMessage);
                    while (_isConnected)
                    {
                        try
                        {
                            _ns = tcpClient.GetStream();

                            var handshakeMessage = new Message(MessageType.Handshake, _username, $"{bufferSize}",
                                DateTime.Now);

                            await Messaging.SendMessage(handshakeMessage, _ns);
                            await ReceiveData(tcpClient);
                        }
                        catch (Exception e)
                        {
                            if (e is ObjectDisposedException || e is NullReferenceException)
                            {
                                return;
                            }

                            UI.ShowExceptionAlert("An unexpected error occured", e);
                            SetDisconnected();
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                UI.ShowExceptionAlert("Server error", socketException);
                SetDisconnected();
            }
        }

        private void SetDisconnected()
        {
            _isConnected = false;
            _username = null;
            _bufferSize = -1;

            EnteredName.Editable = true;
            EnteredIPAddress.Editable = true;
            EnteredPort.Editable = true;
            EnteredBufferSize.Editable = true;

            ConnectButton.Title = "Connect";
            SendButton.Enabled = false;
        }

        private void SetConnected()
        {
            _isConnected = true;
            EnteredName.Editable = false;
            EnteredIPAddress.Editable = false;
            EnteredPort.Editable = false;
            EnteredBufferSize.Editable = false;

            ConnectButton.Title = "Disconnect";
            SendButton.Enabled = true;
        }


        async partial void SendMessage(NSObject sender)
        {
            var messageContent = EnteredMessage.StringValue;

            if (!messageContent.Trim().Equals("") && _isConnected)
            {
                EnteredMessage.StringValue = "";
                var message = new Message(
                    MessageType.Info,
                    _username,
                    messageContent,
                    DateTime.Now);

                await Messaging.SendMessage(message, _ns);
            }
        }
    }
}