using MV.ApplicationLayer.GenericExceptionReport;

namespace MV.ApplicationLayer.SpecificExceptionReport
{
    public class ShowtimeRoomInstanceIsUnAvailableException : ExcludeConstraintViolationException
    {
        public string ConflictingName { get; }

        public ShowtimeRoomInstanceIsUnAvailableException(string name, string message = "A room is already being used at this time.")
            : base(message)
        {
            ConflictingName = name;
        }

        public ShowtimeRoomInstanceIsUnAvailableException(string name, string message, Exception innerException)
            : base(message, innerException)
        {
            ConflictingName = name;
        }
    }
}
