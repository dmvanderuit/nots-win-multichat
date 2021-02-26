using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using MultiChatCore;
using MultiChatCore.model;

namespace MultiChatServer
{
    public partial class ViewController : NSViewController
    {
        private ChatListDataSource _chatListDataSource;
        private ClientListDataSource _clientListDataSource;
        private readonly List<NetworkStream> _connectedStreams = new List<NetworkStream>();
        private bool _serverStarted;
        private string _serverName;
        private TcpListener _tcpListener;
        private int _bufferSize;

        // AddClient adds a new client to the list. When the only entry in the list is "No clients yet",
        // the app removes that entry. When the client is added to the datasource, the ClientList gets reloaded
        // and the ClientList scrolls to the last visible row.
        private void AddClient(String username)
        {
            if (_clientListDataSource.Clients.Count > 0 &&
                _clientListDataSource.Clients[0].Contains("No clients yet"))
            {
                _clientListDataSource.Clients.RemoveAt(0);
            }

            _clientListDataSource.Clients.Add(username);
            ClientList.ReloadData();
            ClientList.ScrollRowToVisible(_clientListDataSource.Clients.Count - 1);
        }

        // RemoveClient removes a client from the list of connected clients. When the list is empty after
        // removing the client, "No clients yet" will be added to the list as an info message. After this,
        // the function also reloads the data and scrolls to the bottom, like in the method above.
        private void RemoveClient(String username)
        {
            _clientListDataSource.Clients.Remove(username);

            if (_clientListDataSource.Clients.Count == 0)
            {
                var noClients = "No clients yet";

                AddClient(noClients);
            }

            ClientList.ReloadData();
            ClientList.ScrollRowToVisible(_clientListDataSource.Clients.Count - 1);
        }

        // ReceiveData receives the data from a client. It fetches the stream from the TCPClient, then, while the
        // server is started, it read the messages that the stream sends. When the messageContent equals the "@"
        // character, we know the message is done. 
        // The function now parses the message (without the @ sign) to the Message class (the message is in JSON).
        // The message type is determined and depending on the type the message gets processed.
        private async Task ReceiveData(TcpClient client)
        {
            var stream = client.GetStream();

            while (_serverStarted)
            {
                var readBytes = new byte[_bufferSize];
                var messageContent = "";

                while (messageContent.IndexOf("@") < 0)
                {
                    var receivedBytes = await stream.ReadAsync(readBytes, 0, readBytes.Length);
                    messageContent += Encoding.ASCII.GetString(readBytes, 0, receivedBytes);
                }

                var finalMessageContent = messageContent.Substring(0, messageContent.IndexOf("@"));

                var message = Serialization.Deserialize(finalMessageContent);

                switch (message.type)
                {
                    // In case the message-type is disconnect, the client sending the message wants to leave
                    // the server. In this case, we remove the client's stream from the list of streams. We remove the
                    // client from the list of connected clients and we broadcast a message to all other users saying
                    // that the user has left the server.
                    case MessageType.Disconnect:
                        _connectedStreams.Remove(stream);
                        RemoveClient(message.sender);
                        var leftMessage = new Message(MessageType.Info, $"{_serverName}",
                            $"{message.sender} just left the server.", DateTime.Now);

                        await BroadcastMessage(leftMessage);
                        break;
                    // In case the message-type is handshake, the client sending the message wants to connect to the
                    // server. When this is the case, we check if the name of the client isn't already in use. 
                    // If the name isn't in use, we add the stream to the list of connected streams and we broadcast
                    // a message saying the user has joined.
                    case MessageType.Handshake:
                        if (_clientListDataSource.Clients.Contains(message.sender))
                        {
                            var duplicateNameMessage = new Message(MessageType.Error, $"{_serverName}",
                                "The username you are trying to connect with is already in use.", DateTime.Now);
                            var serverInfoMessage = new Message(MessageType.Info, $"{_serverName}",
                                $"A user with duplicate name {message.sender} was denied to connect.", DateTime.Now);
                            await Messaging.SendMessage(duplicateNameMessage, stream);
                            UI.AddMessage(serverInfoMessage, _chatListDataSource, ChatList);
                            break;
                        }

                        _connectedStreams.Add(stream);
                        AddClient(message.sender);

                        var joinedMessage = new Message(MessageType.Info, $"{_serverName}",
                            $"{message.sender} just joined the server!", DateTime.Now);
                        await BroadcastMessage(joinedMessage);
                        break;
                    // In any other case, we simply broadcast the message to all users.
                    default:
                        await BroadcastMessage(message);
                        break;
                }
            }
        }

        // BroadcastMessage first adds the given message to the server UI. After that, it loops through all connected
        // network streams and send the message to each individual client.
        private async Task BroadcastMessage(Message message)
        {
            UI.AddMessage(message, _chatListDataSource, ChatList);
            foreach (var connectedStream in _connectedStreams)
            {
                await Messaging.SendMessage(message, connectedStream);
            }
        }

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        // ViewWillAppear is the initializing method that sets the window's title.
        public override void ViewWillAppear()
        {
            base.ViewWillAppear();
            View.Window.Title = "MultiChat Server";
        }

        // In AwakeFromNib a lot of setup is done for the server. The first sections cover the setup for the list
        // of chats and the list of clients. It creates a new DataSource that holds all necessary information
        // for the list (holding a list of items and providing the amount of rows). After that, a delegate is 
        // instantiated, containing some logic behind the lists.
        // After this, a message is added that no messages are present yet. The same is done for the clients. 
        // The default buffer size value is also inserted into the input field.
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            _chatListDataSource = new ChatListDataSource();
            ChatList.DataSource = _chatListDataSource;
            ChatList.Delegate = new ChatListDelegate(_chatListDataSource);

            _clientListDataSource = new ClientListDataSource();
            ClientList.DataSource = _clientListDataSource;
            ClientList.Delegate = new ClientListDelegate(_clientListDataSource);

            var noMessages = new Message(
                MessageType.Info,
                "System",
                "No messages yet",
                DateTime.Now);

            var noClients = "No clients yet";

            UI.AddMessage(noMessages, _chatListDataSource, ChatList);
            AddClient(noClients);

            EnteredBufferSize.StringValue = "1024";
        }

        // StartServer is the button handler that is called when the connect-disconnect button is clicked.
        // In this method we validate all fields need validation. When validation fails, a custom exception
        // (InvalidInputException) is thrown and an alert is shown based on the exception. 
        // If all is well, we start the server. If it is already started, we stop it.
        async partial void StartServer(NSObject sender)
        {
            IPAddress serverIp;
            int enteredPort;

            try
            {
                _serverName =
                    Validation.ValidateName(EnteredServerName.StringValue.Trim(), Validation.NameTypes.Server);
                serverIp = Validation.ValidateIp(EnteredServerIP.StringValue.Trim(), Validation.NameTypes.Server);
                enteredPort = Validation.ValidatePort(EnteredServerPort.StringValue.Trim());
                _bufferSize = Validation.ValidateBufferSize(EnteredBufferSize.StringValue.Trim());
            }
            catch (InvalidInputException e)
            {
                UI.ShowAlert(e.Title,
                    e.Message);
                return;
            }

            if (_serverStarted)
            {
                await StopServer();
            }
            else
            {
                await StartServer(serverIp, enteredPort);
            }
        }

        // StopServer stops the server. It broadcasts a message with the special "ServerStopped" type, telling all clients
        // to disconnect. It empties the list of connected streams, it stops the TCP listener and it sets the server
        // as stopped.
        private async Task StopServer()
        {
            var globalStoppedMessage = new Message(
                MessageType.ServerStopped, $"{_serverName}", "The server was stopped.", DateTime.Now);
            await BroadcastMessage(globalStoppedMessage);

            _connectedStreams.Clear();

            _tcpListener.Stop();
            SetServerStopped();
        }

        // StartServer handles everything that needs handling when the server is starting. This might be the most
        // important method of the server application. 
        // Firstly, it calls the method that handles the UI. After that, it starts the tcpListener on the given port.
        // If that throws a socketException, we know the IP is in use and we show the user that. We stop the server
        // when this exception is thrown.
        // After this, we show the message that the server is listening for new clients. While the server is started,
        // we accept clients (asynchronously) and we receive the data.
        private async Task StartServer(IPAddress ipAddress, int enteredPort)
        {
            SetServerStarted();
            _tcpListener = new TcpListener(ipAddress, enteredPort);
            try
            {
                _tcpListener.Start();
            }
            catch (SocketException)
            {
                UI.ShowAlert("Couldn't start server", "The entered IP is already in use or can't be used.");
                SetServerStopped();
            }

            var listeningMessage = new Message(
                MessageType.Info,
                "System",
                "Server started and listening for clients",
                DateTime.Now);
            UI.AddMessage(listeningMessage, _chatListDataSource, ChatList);

            while (_serverStarted)
            {
                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync();
                    var data = ReceiveData(client);
                    if (data.IsFaulted)
                        await data;
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        // SetServerStarted handles all UI when the server is started.
        private void SetServerStarted()
        {
            _serverStarted = true;
            EnteredServerName.Editable = false;
            EnteredServerIP.Editable = false;
            EnteredServerPort.Editable = false;
            EnteredBufferSize.Editable = false;
            StartStopButton.Title = "Stop Server";
            View.Window.Title = "MultiChat Server - Server Started";
        }

        // SetServerStopped returns the application to "cold and dark" state meaning that when this method is called,
        // everything will be as it was when it first started. After this method, we'll be able to start the server
        // again.
        private void SetServerStopped()
        {
            _serverStarted = false;
            _serverName = null;
            _bufferSize = -1;

            EnteredServerName.Editable = true;
            EnteredServerIP.Editable = true;
            EnteredServerPort.Editable = true;
            EnteredBufferSize.Editable = true;
            StartStopButton.Title = "Start Server";

            _clientListDataSource.Clients.Clear();
            View.Window.Title = "MultiChat Server";
        }

        // SendMessage gets the entered message in the message field. If this contains anything and the server is
        // started, we clear the field and broadcast the message.
        // This method is the button handler for the send button, and it gets called when the user presses "enter"
        // in the input field.
        async partial void SendMessage(NSObject sender)
        {
            var messageContent = EnteredMessage.StringValue;

            if (!messageContent.Trim().Equals("") && _serverStarted)
            {
                EnteredMessage.StringValue = "";
                var message = new Message(
                    MessageType.Info,
                    $"{_serverName}",
                    messageContent,
                    DateTime.Now);

                await BroadcastMessage(message);
            }
        }
    }
}