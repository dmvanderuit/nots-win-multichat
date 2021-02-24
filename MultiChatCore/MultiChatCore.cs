using System;
using System.Net;
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

    public class InvalidInputException : Exception
    {
        public string Title { get; private set; }

        public InvalidInputException(string title, string message)
            : base(message)
        {
            Title = title;
        }
    }

    public class Validation
    {
        public enum NameTypes
        {
            Server,
            Client
        }

        public static string ValidateName(string enteredName, NameTypes type)
        {
            if (enteredName == "")
            {
                throw new InvalidInputException($"Provide {(type == NameTypes.Client ? "username" : "server name")}",
                    $"Please provide a {(type == NameTypes.Client ? "username" : "server name")} in order to " +
                    $"{(type == NameTypes.Client ? "connect to" : "start")} the server.");
            }

            return enteredName;
        }

        public static IPAddress ValidateIp(string enteredIp)
        {
            IPAddress validatedIp;

            if (enteredIp == "")
            {
                throw new InvalidInputException("Provide server IP",
                    "Please provide a server IP in order to connect to the server.");
            }

            if (IPAddress.TryParse(enteredIp, out validatedIp) == false)
            {
                throw new InvalidInputException("Invalid server ip",
                    "The IP Address you entered was invalid.");
            }

            return validatedIp;
        }

        public static int ValidatePort(string enteredPort)
        {
            int validatedPort;

            if (enteredPort == "")
            {
                throw new InvalidInputException("Provide server port",
                    "Please provide a server port in order to connect to the server.");
            }

            try
            {
                validatedPort = Int32.Parse(enteredPort);
            }
            catch (FormatException)
            {
                throw new InvalidInputException("Invalid server port",
                    "The port you entered was invalid. Please make sure the port is a numeric value.");
            }

            return validatedPort;
        }

        public static int ValidateBufferSize(string enteredBufferSize)
        {
            int validatedBufferSize;

            try
            {
                validatedBufferSize = Int32.Parse(enteredBufferSize);
            }
            catch (FormatException)
            {
                throw new InvalidInputException("Invalid server buffer size",
                    "The buffer size you entered is likely not a number. Please enter a valid number.");
            }

            if (enteredBufferSize == "" || validatedBufferSize <= 0)
            {
                throw new InvalidInputException("Provide buffer size",
                    "Please provide a positive buffer size in order to connect to the server.");
            }

            return validatedBufferSize;
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