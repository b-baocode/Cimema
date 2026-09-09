namespace MV.ApplicationLayer.GenericExceptionReport
{
    public class ExcludeConstraintViolationException : Exception
    {
        public ExcludeConstraintViolationException(string message) : base(message) { }
        public ExcludeConstraintViolationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
