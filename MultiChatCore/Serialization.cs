using MultiChatCore.model;
using Newtonsoft.Json;

namespace MultiChatCore
{
    public static class Serialization
    {
        // Serialization deserializes a string in JSON format to a Message.
        public static Message Deserialize(string serializedMessage)
        {
            return JsonConvert.DeserializeObject<Message>(serializedMessage);
        }
    }
}