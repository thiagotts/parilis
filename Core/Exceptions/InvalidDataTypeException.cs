namespace Core.Exceptions {
    public class InvalidDataTypeException : ParilisException {
        public InvalidDataTypeException() {}
        public InvalidDataTypeException(string message) : base(message) {}
    }
}