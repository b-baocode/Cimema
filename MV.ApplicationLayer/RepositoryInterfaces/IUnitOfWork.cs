using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //Repo interfaces
        IUserRepository userRepository { get; }
        IEmployeeRepository employeeRepository { get; }
        IMovieRepository movieRepository { get; }
        IGenreRepository genreRepository { get; }
        IPromotionRepository promotionRepository { get; }

        //Single commit point
        Task<int> SaveChangesAsync();
    }
}
