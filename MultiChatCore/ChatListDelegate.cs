using System;
using AppKit;

namespace MultiChatCore
{
    public class ChatListDelegate : NSTableViewDelegate
    {
        private const string CellIdentifier = "ChatCell";
        private ChatListDataSource _dataSource;

        public ChatListDelegate(ChatListDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
        {
            NSTextField view = (NSTextField) tableView.MakeView(CellIdentifier, this);

            if (view == null)
            {
                view = new NSTextField();
                view.Identifier = CellIdentifier;
                view.BackgroundColor = NSColor.Clear;
                view.Bordered = false;
                view.Selectable = false;
                view.Editable = false;
            }

            switch (tableColumn.Title)
            {
                case "Sender":
                    view.StringValue = _dataSource.Messages[(int) row].sender;
                    break;
                case "Message":
                    view.StringValue = _dataSource.Messages[(int) row].content;
                    break;
            }

            return view;
        }
    }
}