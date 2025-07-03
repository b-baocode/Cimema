using MV.ApplicationLayer.DTO.ResponseModel;
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

        public async Task<ShowTimeRoomInstanceGetByIdResponse?> GetRoomInstanceWithSeatById(int roomInstanceId)
        {
            var roomInstanceResult = await _unitOfWork.showtimeRoomInstanceRepository.GetRoomInstanceByIdAsync(roomInstanceId);

            if (roomInstanceResult == null)
            {
                return null;
            }

            var seatResult = await _unitOfWork.seatDataForShowtimeRepository.GetSeatsForRoomInstanceAsync(roomInstanceId);

            var searchedResult = new ShowTimeRoomInstanceGetByIdResponse
            {
                RoomInstanceId = roomInstanceResult.RoomInstanceId,
                RoomName = roomInstanceResult.RoomName,
                RoomRows = roomInstanceResult.RoomRows,
                RoomColumns = roomInstanceResult.RoomColumns,
                RoomStatus = roomInstanceResult.RoomStatus,
                RoomTypeName = roomInstanceResult.RoomTypeName,
                RoomTypePrice = roomInstanceResult.RoomTypePrice,
                TotalSeatCounts = roomInstanceResult.TotalSeatCounts,
                StandardSeatCount = roomInstanceResult.StandardSeatCount,
                VipSeatCount = roomInstanceResult.VipSeatCount,
                CoupleSeatCount = roomInstanceResult.CoupleSeatCount,
                RemainSeatsCount = roomInstanceResult.RemainSeatsCount,
                SeatForRoomInstances = seatResult.Select(
                    srDto => new SeatForRoomInstanceDTO
                    {
                        SeatDataId = srDto.SeatDataId,
                        ShowtimeInstanceId = srDto.ShowtimeInstanceId,
                        RowLabel = srDto.RowLabel,
                        ColumnNumber = srDto.ColumnNumber,
                        SeatTypeName = srDto.SeatTypeName,
                        SeatTypePrice = srDto.SeatTypePrice,
                        PairedWithSeatLocation = srDto.PairedWithSeatLocation,
                        SeatStatus = srDto.Status,
                    })
                .OrderBy(srDto => srDto.RowLabel)
                .ThenBy(srtDto => srtDto.ColumnNumber)
                .ToList()
            };

            return searchedResult;
        }

        public async Task<string?> GetRoomInstanceNameById(int roomInstanceId)
        {
            return await _unitOfWork.showtimeRoomInstanceRepository.GetRoomInstanceNameByIdAsync(roomInstanceId);
        }

        public async Task<ShowtimeRoomInstance?> GetByShowtimeInstanceIdAsync(int showtimeInstanceId)
        {
            return await _unitOfWork.showtimeRoomInstanceRepository.GetByShowtimeInstanceIdAsync(showtimeInstanceId);
        }

    }
} 