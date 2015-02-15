using System;

namespace Core.Exceptions {
    public class InvalidDataTypeException : Exception {
        public InvalidDataTypeException() {}
        public InvalidDataTypeException(string message) : base(message) { }
    }
}