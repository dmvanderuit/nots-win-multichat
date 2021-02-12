using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
}