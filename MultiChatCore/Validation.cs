using System;
using System.Net;

namespace MultiChatCore
{
    // Validation handles several validation rules, mostly shared between client and server.
    public static class Validation
    {
        public enum NameTypes
        {
            Server,
            Client
        }

        // ValidateName validates the entered name. The only check on this validation is that it cannot be empty.
        // The type parameter is passed to generate an applicable error message for the implementing party.
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

        // ValidateIp validates the IP Address passed as a parameter. It validates if it is not empty and then it parses
        // it to a valid IP address.
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

        // ValidatePort checks if the passed port isn't empty. After that, it tries to parse the string to an integer.
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

        // ValidateBufferSize checks if the buffersize is a valid integer and it checks if the buffersize is greater
        // than 0.
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
}