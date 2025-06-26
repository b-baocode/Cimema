using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class ShowtimeRoomInstanceService : IShowtimeRoomInstanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShowtimeRoomInstanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ShowtimeRoomInstance> GetByShowtimeIdAsync(int showtimeId)
        {
            var showtimeRoomInstance = await _unitOfWork.showtimeRoomInstanceRepository.GetByShowtimeIdAsync(showtimeId);
            if (showtimeRoomInstance == null)
            {
                throw new Exception("ShowtimeRoomInstance không tồn tại cho suất chiếu này.");
            }
            return showtimeRoomInstance;
        }
    }
} 