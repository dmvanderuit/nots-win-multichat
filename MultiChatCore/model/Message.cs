using System;
using System.Text;
using Newtonsoft.Json;

namespace MultiChatCore.model
{
    public enum MessageType
    {
        Handshake,
        Error,
        Message,
        Info
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

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            if (type == MessageType.Message)
            {
                stringBuilder.Append($"{sender} ({time:hh:mm:ss})\t|");
            }
            else
            {
                stringBuilder.Append($"{time:hh:mm:ss}\t|");
            }

            stringBuilder.Append(content);

            return stringBuilder.ToString();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}