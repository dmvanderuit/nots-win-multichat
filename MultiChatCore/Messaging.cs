using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AppKit;
using MultiChatCore.model;

namespace MultiChatCore
{
    // Messaging handles the shared message functionality.
    public static class Messaging
    {
        // SendMessage converts the message to JSON and adds an "@" end marker. It then writes the message to the
        // network stream.
        public static async Task SendMessage(Message message, NetworkStream ns)
        {
            var buffer = Encoding.ASCII.GetBytes(message.ToJson() + "@");
            await ns.WriteAsync(buffer, 0, buffer.Length);
        }

        // UpdateBufferSize updates the buffer size and adds an info message to the chatlist. 
        public static int UpdateBufferSize(string enteredBufferSizeString, string name,
            ChatListDataSource chatListDataSource, NSTableView chatList)
        {
            try
            {
                var enteredBufferSize = Validation.ValidateBufferSize(enteredBufferSizeString);
                var message = new Message(MessageType.Info, name,
                    $"The buffer size was changed to {enteredBufferSize}.", DateTime.Now);
                UI.AddMessage(message, chatListDataSource, chatList);
                return enteredBufferSize;
            }
            catch (InvalidInputException e)
            {
                UI.ShowAlert(e.Title,
                    e.Message);
                return 0;
            }
        }
    }
}