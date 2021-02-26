using System;
using System.Text;
using AppKit;
using MultiChatCore.model;

namespace MultiChatCore
{
    // UI Handles various shared UI events.
    public static class UI
    {
        // ShowAlert shows a simple UI alert, mostly being used for error handling.
        public static void ShowAlert(string title, string message)
        {
            var alert = new NSAlert()
            {
                AlertStyle = NSAlertStyle.Critical,
                MessageText = title,
                InformativeText = message
            };
            alert.RunModal();
        }

        // ShowExceptionAlert is similar to ShowAlert, but in stead of a hardcoded message it receives an exception
        // from which it generates an alert.
        public static void ShowExceptionAlert(string title, Exception exception)
        {
            var messageSb = new StringBuilder("The following error occured: ");
            messageSb.Append(exception.Message);
            var alert = new NSAlert()
            {
                AlertStyle = NSAlertStyle.Critical,
                MessageText = title,
                InformativeText = messageSb.ToString()
            };
            alert.RunModal();
        }

        // AddMessage adds a new message to the desired datasource. It removes the first placeholder message if present,
        // and it adds the message to the list in the datasource. It then reloads the tableview and it scrolls it to
        // the last message.
        public static void AddMessage(Message message, ChatListDataSource dataSource, NSTableView chatList)
        {
            if (dataSource.Messages.Count > 0 &&
                dataSource.Messages[0].content.Contains("No messages yet"))
            {
                dataSource.Messages.RemoveAt(0);
            }

            dataSource.Messages.Add(message);
            chatList.ReloadData();
            chatList.ScrollRowToVisible(dataSource.Messages.Count - 1);
        }
    }
}