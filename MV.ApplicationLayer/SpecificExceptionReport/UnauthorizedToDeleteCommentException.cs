public class UnauthorizedToDeleteCommentException : Exception
{
    public UnauthorizedToDeleteCommentException(string message) : base(message) { }
    public UnauthorizedToDeleteCommentException(string message, Exception innerException) : base(message, innerException) { }
}