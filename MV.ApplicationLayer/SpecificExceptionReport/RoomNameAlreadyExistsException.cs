using MV.ApplicationLayer.GenericExceptionReport;

namespace MV.ApplicationLayer.SpecificExceptionReport
{
    public class RoomNameAlreadyExistsException : UniqueConstraintViolationException
    {
        public string ConflictingName { get; }

        public RoomNameAlreadyExistsException(string name, string message = "A room with this name already exists.")
            : base(message)
        {
            ConflictingName = name;
        }

        public RoomNameAlreadyExistsException(string name, string message, Exception innerException)
            : base(message, innerException)
        {
            ConflictingName = name;
        }
    }
}
