﻿using System;
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
        private string _serverName;
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
            ChatList.ScrollRowToVisible(_chatListDataSource.Messages.Count - 1);
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
                var readBytes = new byte[1024];
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
                        var leftMessage = new Message(MessageType.Info, $"{_serverName} (server)",
                            $"{message.sender} just left the server.", DateTime.Now);

                        AddMessage(leftMessage);
                        await BroadcastMessage(leftMessage);
                        break;
                    case MessageType.Handshake:
                        if (_clientListDataSource.Clients.Contains(message.sender))
                        {
                            var duplicateNameMessage = new Message(MessageType.Error, $"{_serverName} (server)",
                                "The username you are trying to connect with is already in use", DateTime.Now);
                            var serverInfoMessage = new Message(MessageType.Info, $"{_serverName} (server)",
                                $"A user with duplicate name {message.sender} was denied to connect.", DateTime.Now);
                            await Messaging.SendMessage(duplicateNameMessage, stream);
                            AddMessage(serverInfoMessage);
                        }
                        else
                        {
                            _connectedStreams.Add(stream);
                            AddClient(message.sender);

                            var joinedMessage = new Message(MessageType.Info, $"{_serverName} (server)",
                                $"{message.sender} just joined the server!", DateTime.Now);
                            AddMessage(joinedMessage);
                            await BroadcastMessage(joinedMessage);
                        }

                        break;
                    default:
                        AddMessage(message);
                        await BroadcastMessage(message);
                        break;
                }
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
                "System",
                "No messages yet",
                DateTime.Now);

            var noClients = "No clients yet";

            AddMessage(noMessages);
            AddClient(noClients);
        }

        async partial void StartServer(NSObject sender)
        {
            var enteredServerName = EnteredServerName.StringValue.Trim();
            var enteredPort = -1;

            if (enteredServerName == "")
            {
                UI.ShowAlert("Provide server name", "Please provide a server name in order to start the server.");
                return;
            }

            _serverName = enteredServerName;

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
                MessageType.ServerStopped, $"{_serverName} (server)", "The server was stopped.", DateTime.Now);
            await BroadcastMessage(globalStoppedMessage);

            _connectedStreams.Clear();

            _tcpListener.Stop();
            SetServerStopped();

            var endedMessage = new Message(
                MessageType.Info,
                "System",
                "Server was stopped",
                DateTime.Now);
            AddMessage(endedMessage);
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
            
            EnteredServerName.Editable = true;
            EnteredServerPort.Editable = true;
            EnteredBufferSize.Editable = true;
            StartStopButton.Title = "Start Server";
        }
    }
}