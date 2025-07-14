namespace MV.ApplicationLayer.SpecificExceptionReport
{
    public class UserHasNotWatchedMovieException : Exception
    {
        public UserHasNotWatchedMovieException(string message) : base(message) { }
        public UserHasNotWatchedMovieException(string message, Exception innerException) : base(message, innerException) { }
    }
} 