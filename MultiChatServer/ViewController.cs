using System;
using AppKit;
using Foundation;
using MultiChatCore;
using MultiChatCore.model;

namespace MultiChatServer
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            var chatListDataSource = new ChatListDataSource();
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST", "CONTENT", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST2", "CONTENT2", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));
            chatListDataSource.Messages.Add(new Message(MessageType.Info, "TEST3", "CONTENT3", DateTime.Now));

            ChatList.DataSource = chatListDataSource;
            ChatList.Delegate = new ChatListDelegate(chatListDataSource);

            ClientListDataSource clientListDataSource = new ClientListDataSource();
            clientListDataSource.Clients.Add("Client 1");
            clientListDataSource.Clients.Add("Client 2");
            clientListDataSource.Clients.Add("Client 3");
            clientListDataSource.Clients.Add("Client 4");

            ClientList.DataSource = clientListDataSource;
            ClientList.Delegate = new ClientListDelegate(clientListDataSource);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
        }

        public override NSObject RepresentedObject
        {
            get { return base.RepresentedObject; }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}