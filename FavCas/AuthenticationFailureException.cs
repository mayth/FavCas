using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FavCas
{
    [Serializable]
    public class AuthenticationFailureException : Exception
    {
        public AuthenticationFailureException() { }
        public AuthenticationFailureException(string message) : base(message) { }
        public AuthenticationFailureException(string message, Exception inner) : base(message, inner) { }
        protected AuthenticationFailureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
