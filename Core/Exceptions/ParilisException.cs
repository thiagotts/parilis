using System;

namespace Core.Exceptions {
    public class ParilisException : Exception {
        protected ParilisException() {}
        protected ParilisException(string message) : base(message) {}
    }
}