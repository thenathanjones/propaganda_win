using System;

namespace Propaganda.Core.Util
{
    public class InitialisationException : Exception
    {
        public InitialisationException()
        {
        }

        public InitialisationException(string message) : base(message)
        {
        }

        public InitialisationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}