using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.GenericExceptionReport
{
    public class UniqueConstraintViolationException : Exception
    {
        public UniqueConstraintViolationException(string message) : base(message) { }
        public UniqueConstraintViolationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
