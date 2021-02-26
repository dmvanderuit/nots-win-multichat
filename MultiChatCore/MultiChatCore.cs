using System;

namespace MultiChatCore
{
    // InvalidInputException contains a "title" field to facilitate the string that is shown in the title field of the
    // alert window this exception is often shown in.
    public class InvalidInputException : Exception
    {
        public string Title { get; private set; }

        public InvalidInputException(string title, string message)
            : base(message)
        {
            Title = title;
        }
    }
}