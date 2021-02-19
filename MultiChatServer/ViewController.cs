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
        private bool _serverStarted = false;
        private TcpListener _tcpListener;

        private void AddMessage(Message message)
        {
            if (_chatListDataSource.Messages.Count > 0 &&
                _chatListDataSource.Messages[0].content.Contains("No messages yet"))
            {
                _chatListDataSource.Messages.RemoveAt(0);
                ChatList.StringValue = "";
            }

            _chatListDataSource.Messages.Add(message);
            ChatList.ReloadData();
            ChatList.ScrollRowToVisible(_chatListDataSource.Messages.Count -1);
        }

        private void AddClient(String username)
        {
            if (_clientListDataSource.Clients.Count > 0 &&
                _clientListDataSource.Clients[0].Contains("No clients yet"))
            {
                _clientListDataSource.Clients.RemoveAt(0);
            }

            _clientListDataSource.Clients.Add(username);
            ClientList.ReloadData();
        }

        private async Task ReceiveData(TcpClient client)
        {
            var stream = client.GetStream();
            _connectedStreams.Add(stream);

            while (_serverStarted)
            {
                var readBytes = new byte[1024];
                var messageContent = "";

                while (messageContent.IndexOf("@") < 0)
                {
                    var receivedBytes = await stream.ReadAsync(readBytes, 0, readBytes.Length);
                    messageContent += Encoding.ASCII.GetString(readBytes, 0, receivedBytes);
                }

                var finalMessageContent = messageContent.Substring(0, messageContent.IndexOf("@"));

                var message = Serialization.Deserialize(finalMessageContent);

                if (message.type == MessageType.Disconnect)
                {
                    _connectedStreams.Remove(stream);

                    var leftMessage = new Message(MessageType.Info, "System",
                        $"{message.sender} just left the server.", DateTime.Now);

                    AddMessage(leftMessage);
                    await BroadcastMessage(leftMessage);
                    return;
                }
                
                AddMessage(message);

                await BroadcastMessage(message);
            }
        }

        private async Task BroadcastMessage(Message message)
        {
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
                "system",
                "No messages yet",
                DateTime.Now);

            var noClients = "No clients yet";

            AddMessage(noMessages);
            AddClient(noClients);
        }

        async partial void StartServer(NSObject sender)
        {
            var enteredPort = -1;

            // Port validation
            try
            {
                enteredPort = Int32.Parse(EnteredServerPort.StringValue.Trim());
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
                MessageType.ServerStopped, "System", "The server was stopped.", DateTime.Now);
            await BroadcastMessage(globalStoppedMessage);

            _connectedStreams.Clear();

            _tcpListener.Stop();
            _serverStarted = false;

            var endedMessage = new Message(
                MessageType.Info,
                "system",
                "Server was stopped",
                DateTime.Now);
            AddMessage(endedMessage);
        }

        private async Task StartServer(int enteredPort)
        {
            _tcpListener = new TcpListener(IPAddress.Any, enteredPort);
            _tcpListener.Start();
            _serverStarted = true;
            var listeningMessage = new Message(
                MessageType.Info,
                "system",
                "Server started and listening for clients",
                DateTime.Now);
            AddMessage(listeningMessage);

            while (_serverStarted)
            {
                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync();
                    await ReceiveData(client);
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }
    }
}