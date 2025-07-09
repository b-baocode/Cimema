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

        public Task ValidateSeatsAsync(List<int> requestedSeatIds, Dictionary<int, SeatDataForShowtime> seatDataDict)
        {
            var invalidSeats = requestedSeatIds.Where(id => !seatDataDict.ContainsKey(id)).ToList();
            if (invalidSeats.Any())
            {
                throw new Exception($"Invalid seats: {string.Join(", ", invalidSeats)}.");
            }

            foreach (var seatId in requestedSeatIds)
            {
                var seatData = seatDataDict[seatId];
                if (seatData.Status != null && seatData.Status != "Active")
                {
                    throw new Exception($"Seat {seatId} is booked or unavailable.");
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
                    seatToUpdate.Status = newStatus;
                    await _unitOfWork.seatDataForShowtimeRepository.UpdateAsync(seatToUpdate);
                }
            }
        }
    }
} 