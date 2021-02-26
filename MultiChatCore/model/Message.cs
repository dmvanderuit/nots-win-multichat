using System;
using System.Text;
using Newtonsoft.Json;

namespace MultiChatCore.model
{
    // MessageType contains all the message types the application uses. 
    public enum MessageType
    {
        Handshake, // Handshake is the type used by clients registering with a new server
        Disconnect, // Disconnect is the type used by clients to tell the server they are leaving 
        Error, // Error is used when something goes wrong in the server, e.g. the username already exists
        ServerStopped, // ServerStopped is the type the server sends to all clients when it is stopping
        Message, // Message is the regular message type a client or the server sends
        Info // Info is a generic message type, typically not seen by anyone but the client or server itself
    }

    public class Message
    {
        public MessageType type { get; }
        public string sender { get; }
        public string content { get; }
        public DateTime time { get; }

        public Message(MessageType type, string sender, string content, DateTime time)
        {
            this.type = type;
            this.sender = sender;
            this.content = content;
            this.time = time;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}