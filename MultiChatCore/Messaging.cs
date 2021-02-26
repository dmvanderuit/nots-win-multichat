using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
    }
}