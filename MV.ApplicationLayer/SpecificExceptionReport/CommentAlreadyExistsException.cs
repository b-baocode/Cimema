namespace MV.ApplicationLayer.SpecificExceptionReport
{
    public class CommentAlreadyExistsException : Exception
    {
        public CommentAlreadyExistsException(string message) : base(message) { }
        public CommentAlreadyExistsException(string message, Exception innerException) : base(message, innerException) { }
    }
}