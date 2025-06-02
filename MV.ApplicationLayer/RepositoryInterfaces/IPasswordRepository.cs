using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IPasswordRepository
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string storedHash);
    }
}
