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


        // While the client is connected to the server, this method reads the data from the networkstream. The size of
        // the buffer is determined by the user's input. 
        private async Task ReceiveData(TcpClient client)
        {
            while (_isConnected)
            {
                var readBytes = new byte[_bufferSize];
                var messageContent = "";

                // While the content does not contain the end marker, it keeps reading and adding to the messageContent
                // string. 
                while (messageContent.IndexOf("@") < 0)
                {
                    var receivedBytes = await _ns.ReadAsync(readBytes, 0, readBytes.Length);
                    messageContent += Encoding.ASCII.GetString(readBytes, 0, receivedBytes);
                }

                // When the end marker is received, the message is complete. End marker is removed.
                var finalMessageContent = messageContent.Substring(0, messageContent.IndexOf("@"));

                // After removing the end marker, we serialize the received string - which is in JSON - to a Message.
                var message = Serialization.Deserialize(finalMessageContent);

                // We add the message to the list of messages.
                UI.AddMessage(message, _chatListDataSource, ChatList);

                // If the message has the type "ServerStopped", we set the networkstream to null and we disconnect from
                // the server.
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

        // ViewWillAppear handles the initial window title value.
        public override void ViewWillAppear()
        {
            base.ViewWillAppear();
            View.Window.Title = "MultiChat Client";
        }

        // AwakeFromNib handles the initial setup of the ChatList. It also adds the initial placeholder message.
        // Finally, it sets the send button to be disabled, because the server isn't connected yet, and it enters the 
        // placeholder buffer size in the input field. 
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

            UI.AddMessage(noMessages, _chatListDataSource, ChatList);

            SendButton.Enabled = false;
            EnteredBufferSize.StringValue = "1024";
        }

        // Connect is the button handler for the connect button. It validates all input fields and shows a UI alert when
        // validation fails. If the client isn't connected yet, we try to connect. If it's already connected, we 
        // disconnect from the server.
        async partial void Connect(NSObject sender)
        {
            IPAddress serverIp;
            int enteredPort;
            int enteredBufferSize;

            try
            {
                _username = Validation.ValidateName(EnteredName.StringValue.Trim(), Validation.NameTypes.Client);
                serverIp = Validation.ValidateIp(EnteredIPAddress.StringValue.Trim(), Validation.NameTypes.Client);
                enteredPort = Validation.ValidatePort(EnteredPort.StringValue.Trim());
                enteredBufferSize = Validation.ValidateBufferSize(EnteredBufferSize.StringValue.Trim());
                _bufferSize = enteredBufferSize;
            }
            catch (InvalidInputException e)
            {
                UI.ShowAlert(e.Title,
                    e.Message);
                return;
            }

            if (!_isConnected)
            {
                try
                {
                    await ConnectToServer(serverIp, enteredPort, enteredBufferSize.ToString());
                }
                catch (SocketException e)
                {
                    UI.ShowExceptionAlert("An error occured connecting to the server.", e);
                    await DisconnectFromServer();
                }
            }
            else
            {
                await DisconnectFromServer();
            }
        }

        // In DisconnectFromServer we send a message to the server saying we want to quit the server. The server then
        // handles some things on their side, and we show a message to the client that we're successfully disconnected.
        // After that, we set the networkstream to null and we handle the rest of the UI in a separate method.
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
            UI.AddMessage(endedMessage, _chatListDataSource, ChatList);

            _ns = null;
            SetDisconnected();
        }

        // ConnectToServer is the heart of the client application. It starts a new TCPClient and it connects to it. 
        // It adds the notification that the server has been started. After that, while it is connected, it gets the
        // network stream and it send the connecting message, telling the server that it wants to connect.
        // It receives data and handles that in a separate method.
        // When anything goes wrong, we stop the server gracefully. 
        private async Task ConnectToServer(IPAddress serverIp, int enteredPort, string bufferSize)
        {
            try
            {
                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(serverIp.ToString(), enteredPort);
                using (tcpClient)
                {
                    SetConnected();
                    var connectedMessage = new Message(
                        MessageType.Info,
                        "system",
                        "Connected to the server",
                        DateTime.Now);
                    UI.AddMessage(connectedMessage, _chatListDataSource, ChatList);
                    while (_isConnected)
                    {
                        try
                        {
                            _ns = tcpClient.GetStream();

                            var handshakeMessage = new Message(MessageType.Handshake, _username, "",
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

        // SetDisconnected makes sure everything is set back to the original value to make it possible to connect
        // to the server again. It also manipulates the UI so that it can be connected to the server again.
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
            View.Window.Title = "MultiChat Client";
        }

        // SetConnected manipulates the UI so that it is not possible to change the server details when you're already
        // connected. It also sets the _isConnected to true, which is an important variable as it determines whether the
        // client should receive/send data or not.
        private void SetConnected()
        {
            _isConnected = true;
            EnteredName.Editable = false;
            EnteredIPAddress.Editable = false;
            EnteredPort.Editable = false;
            EnteredBufferSize.Editable = false;

            ConnectButton.Title = "Disconnect";
            SendButton.Enabled = true;
            View.Window.Title = "MultiChat Client - Connected";
        }
        
        // SendMessage is the button handler for the send button, and it gets called when the user presses "enter" in 
        // the message field. 
        // When the entered message is not empty, and the server is connected, it empties the field and it sends the
        // message to the server.
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