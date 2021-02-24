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
                    case MessageType.Disconnect:
                        _connectedStreams.Remove(stream);
                        RemoveClient(message.sender);
                        var leftMessage = new Message(MessageType.Info, $"{_serverName}",
                            $"{message.sender} just left the server.", DateTime.Now);

                        await BroadcastMessage(leftMessage);
                        break;
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

                        var clientBufferSize = -1;

                        try
                        {
                            clientBufferSize = Int32.Parse(message.content);
                        }
                        catch (FormatException)
                        {
                        }

                        if (clientBufferSize != _bufferSize)
                        {
                            var bufferMismatchMessage = new Message(MessageType.Error, $"{_serverName}",
                                "The buffer size you entered is either invalid or doesn't match the server buffer size.",
                                DateTime.Now);
                            var serverInfoMessage = new Message(MessageType.Info, $"{_serverName}",
                                $"A user ({message.sender}) with mismatching buffer size of {_bufferSize} was denied to connect.",
                                DateTime.Now);
                            await Messaging.SendMessage(bufferMismatchMessage, stream);
                            UI.AddMessage(serverInfoMessage, _chatListDataSource, ChatList);
                            break;
                        }

                        _connectedStreams.Add(stream);
                        AddClient(message.sender);

                        var joinedMessage = new Message(MessageType.Info, $"{_serverName}",
                            $"{message.sender} just joined the server!", DateTime.Now);
                        await BroadcastMessage(joinedMessage);
                        break;
                    default:
                        await BroadcastMessage(message);
                        break;
                }
            }
        }

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

        async partial void StartServer(NSObject sender)
        {
            int enteredPort;

            try
            {
                _serverName =
                    Validation.ValidateName(EnteredServerName.StringValue.Trim(), Validation.NameTypes.Server);
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
                await StartServer(enteredPort);
            }
        }

        private async Task StopServer()
        {
            var globalStoppedMessage = new Message(
                MessageType.ServerStopped, $"{_serverName}", "The server was stopped.", DateTime.Now);
            await BroadcastMessage(globalStoppedMessage);

            _connectedStreams.Clear();

            _tcpListener.Stop();
            SetServerStopped();
        }

        private async Task StartServer(int enteredPort)
        {
            SetServerStarted();
            _tcpListener = new TcpListener(IPAddress.Any, enteredPort);
            try
            {
                _tcpListener.Start();
            }
            catch (SocketException)
            {
                UI.ShowAlert("Couldn't start server", "The entered IP is already in use.");
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

        private void SetServerStarted()
        {
            _serverStarted = true;
            EnteredServerName.Editable = false;
            EnteredServerPort.Editable = false;
            EnteredBufferSize.Editable = false;
            StartStopButton.Title = "Stop Server";
        }

        private void SetServerStopped()
        {
            _serverStarted = false;
            _serverName = null;
            _bufferSize = -1;

            EnteredServerName.Editable = true;
            EnteredServerPort.Editable = true;
            EnteredBufferSize.Editable = true;
            StartStopButton.Title = "Start Server";

            _clientListDataSource.Clients.Clear();
        }

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