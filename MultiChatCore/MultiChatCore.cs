using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AppKit;
using MultiChatCore.model;
using Newtonsoft.Json;

namespace MultiChatCore
{
    public class Serialization
    {
        public static Message Deserialize(string serializedMessage)
        {
            return JsonConvert.DeserializeObject<Message>(serializedMessage);
        }
    }

    public class Messaging
    {
        public static async Task SendMessage(Message message, NetworkStream ns)
        {
            var buffer = Encoding.ASCII.GetBytes(message.ToJson() + "@");
            await ns.WriteAsync(buffer, 0, buffer.Length);
        }
    }

    public class UI
    {
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
    }
}