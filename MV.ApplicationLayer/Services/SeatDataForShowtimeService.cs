using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class SeatDataForShowtimeService : ISeatDataForShowtimeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SeatDataForShowtimeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Dictionary<int, SeatDataForShowtime>> GetSeatsDictionaryByShowtimeInstanceIdAsync(int showtimeInstanceId)
        {
            var seatDataList = await _unitOfWork.seatDataForShowtimeRepository.GetSeatsByShowtimeInstanceIdAsync(showtimeInstanceId);
            return seatDataList.ToDictionary(s => s.SeatDataId);
        }

        public async Task<ShowtimeRoomInstance?> GetShowtimeInstanceByShowtimeIdAsync(int showtimeId)
        {
            return await _unitOfWork.showtimeRoomInstanceRepository.GetByShowtimeIdAsync(showtimeId);
        }

        public Task ValidateSeatsAsync(List<int> requestedSeatIds, Dictionary<int, SeatDataForShowtime> seatDataDict)
        {
            var invalidSeats = requestedSeatIds.Where(id => !seatDataDict.ContainsKey(id)).ToList();
            if (invalidSeats.Any())
            {
                throw new Exception($"Các ghế không hợp lệ: {string.Join(", ", invalidSeats)}");
            }

            foreach (var seatId in requestedSeatIds)
            {
                var seatData = seatDataDict[seatId];
                if (seatData.Status != null && seatData.Status != "Active")
                {
                    throw new Exception($"Ghế {seatId} đã được đặt hoặc không khả dụng.");
                }
            }
            return Task.CompletedTask;
        }

        public async Task UpdateSeatsStatusAsync(IEnumerable<int> seatIds, string newStatus, int showtimeInstanceId)
        {
            var seatDataList = await _unitOfWork.seatDataForShowtimeRepository.GetSeatsByShowtimeInstanceIdAsync(showtimeInstanceId);
            
            foreach (var seatId in seatIds)
            {
                var seatToUpdate = seatDataList.FirstOrDefault(s => s.SeatDataId == seatId);
                if (seatToUpdate != null)
                {
                    Console.WriteLine($"[Before] SeatId: {seatToUpdate.SeatDataId}, Status: {seatToUpdate.Status}");
                    seatToUpdate.Status = newStatus;
                    _unitOfWork.seatDataForShowtimeRepository.AttachIfNotTracked(seatToUpdate);
                    await _unitOfWork.seatDataForShowtimeRepository.UpdateAsync(seatToUpdate);
                    Console.WriteLine($"[After] SeatId: {seatToUpdate.SeatDataId}, Status: {seatToUpdate.Status}");
                }
                else
                {
                    Console.WriteLine($"[Warning] SeatId {seatId} not found in showtimeInstanceId {showtimeInstanceId}");
                }
            }
        }
    }
} 