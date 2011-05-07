using System;

namespace Propaganda.Core.Util
{
    internal class ServiceLoadingException : InitialisationException
    {
        public ServiceLoadingException()
        {
        }

        public ServiceLoadingException(string message) : base(message)
        {
        }

        public ServiceLoadingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}