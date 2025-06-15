using MV.ApplicationLayer.GenericExceptionReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.SpecificExceptionReport
{
    public class RoomTypeNameAlreadyExistException : UniqueConstraintViolationException
    {
        public string ConflictingName { get; }

        public RoomTypeNameAlreadyExistException(string name, string message = "A room with this name already exists.") : base(message)
        {
            ConflictingName = name;
        }

        public RoomTypeNameAlreadyExistException(string name, string message, Exception innerException) : base(message, innerException)
        {
            ConflictingName = name;
        }
    }
}
