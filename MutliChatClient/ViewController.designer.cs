// WARNING
//
// This file has been generated automatically by Rider IDE
//   to store outlets and actions made in Xcode.
// If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace MutliChatClient
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSTableView ChatList { get; set; }

		[Outlet]
		AppKit.NSTableColumn ChatListMessageColumn { get; set; }

		[Outlet]
		AppKit.NSTableColumn ChatListSenderColumn { get; set; }

		[Outlet]
		AppKit.NSTextField EnteredBufferSize { get; set; }

		[Outlet]
		AppKit.NSTextField EnteredIPAddress { get; set; }

		[Outlet]
		AppKit.NSTextField EnteredMessage { get; set; }

		[Outlet]
		AppKit.NSTextField EnteredName { get; set; }

		[Outlet]
		AppKit.NSTextField EnteredPort { get; set; }

		[Action ("Connect:")]
		partial void Connect (Foundation.NSObject sender);

		[Action ("SendMessage:")]
		partial void SendMessage (Foundation.NSObject sender);

		void ReleaseDesignerOutlets ()
		{
			if (ChatList != null) {
				ChatList.Dispose ();
				ChatList = null;
			}

			if (ChatListSenderColumn != null) {
				ChatListSenderColumn.Dispose ();
				ChatListSenderColumn = null;
			}

			if (ChatListMessageColumn != null) {
				ChatListMessageColumn.Dispose ();
				ChatListMessageColumn = null;
			}

			if (EnteredMessage != null) {
				EnteredMessage.Dispose ();
				EnteredMessage = null;
			}

			if (EnteredName != null) {
				EnteredName.Dispose ();
				EnteredName = null;
			}

			if (EnteredIPAddress != null) {
				EnteredIPAddress.Dispose ();
				EnteredIPAddress = null;
			}

			if (EnteredPort != null) {
				EnteredPort.Dispose ();
				EnteredPort = null;
			}

			if (EnteredBufferSize != null) {
				EnteredBufferSize.Dispose ();
				EnteredBufferSize = null;
			}

		}
	}
}
