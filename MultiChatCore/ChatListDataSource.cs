using System;
using System.Collections.Generic;
using AppKit;
using MultiChatCore.model;

namespace MultiChatCore
{
    // ChatListDataSource is the datasource for the ChatList table.
    public class ChatListDataSource : NSTableViewDataSource
    {
        public List<Message> Messages = new List<Message>();

        public override nint GetRowCount(NSTableView tableView)
        {
            return Messages.Count;
        }
    }
}