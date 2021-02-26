using System;
using System.Collections.Generic;
using AppKit;

namespace MultiChatServer
{
    // This class handles the data in the client list.
    public class ClientListDataSource : NSTableViewDataSource
    {
        public List<string> Clients = new List<string>();

        public override nint GetRowCount(NSTableView tableView)
        {
            return Clients.Count;
        }
    }
}