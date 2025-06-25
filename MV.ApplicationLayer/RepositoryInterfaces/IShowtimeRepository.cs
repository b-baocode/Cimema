using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IShowtimeRepository
    {
        Task AddAsync(Showtime showtime);

        Task<Showtime?> GetByIdAsync(int showtimeId);

        Task<string?> GetMovieTitleForScheduling(int showtimeId);
    }
}
