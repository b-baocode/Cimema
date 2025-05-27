using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepoInterfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //Repo interfaces
        IUserRepository userRepository { get; }

        //Single commit point
        Task<int> SaveChangesAsync();
    }
}
