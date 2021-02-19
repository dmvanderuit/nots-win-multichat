// WARNING
//
// This file has been generated automatically by Rider IDE
//   to store outlets and actions made in Xcode.
// If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace MultiChatServer
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
		AppKit.NSTableView ClientList { get; set; }

		[Outlet]
		AppKit.NSTableColumn ClientListClientColumn { get; set; }

		[Outlet]
		AppKit.NSTextField EnteredBufferSize { get; set; }

		[Outlet]
		AppKit.NSTextField EnteredMessage { get; set; }

		[Outlet]
		AppKit.NSTextField EnteredServerName { get; set; }

		[Outlet]
		AppKit.NSTextField EnteredServerPort { get; set; }
		
		[Outlet] 
		AppKit.NSButton StartStopButton { get; set; }		
		
		[Outlet] 
		AppKit.NSButton SendMessageButton { get; set; }

		[Action ("SendMessage:")]
		partial void SendMessage (Foundation.NSObject sender);

		[Action ("StartServer:")]
		partial void StartServer (Foundation.NSObject sender);

		void ReleaseDesignerOutlets ()
		{
			if (ChatList != null) {
				ChatList.Dispose ();
				ChatList = null;
			}

			if (ChatListMessageColumn != null) {
				ChatListMessageColumn.Dispose ();
				ChatListMessageColumn = null;
			}

			if (ChatListSenderColumn != null) {
				ChatListSenderColumn.Dispose ();
				ChatListSenderColumn = null;
			}

			if (ClientList != null) {
				ClientList.Dispose ();
				ClientList = null;
			}

			if (ClientListClientColumn != null) {
				ClientListClientColumn.Dispose ();
				ClientListClientColumn = null;
			}

			if (EnteredMessage != null) {
				EnteredMessage.Dispose ();
				EnteredMessage = null;
			}

			if (EnteredServerName != null) {
				EnteredServerName.Dispose ();
				EnteredServerName = null;
			}

			if (EnteredServerPort != null) {
				EnteredServerPort.Dispose ();
				EnteredServerPort = null;
			}

			if (EnteredBufferSize != null) {
				EnteredBufferSize.Dispose ();
				EnteredBufferSize = null;
			}

		}
	}
}
