using System;

namespace Ristretto
{
    /// <summary>
    /// InvalidEncodingException
    /// </summary>
    public class InvalidEncodingException : Exception
    {
        public InvalidEncodingException(string message) : base(message)
        {
        }
    }
}
