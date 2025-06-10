using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.SpecificExceptionReport;
using MV.ApplicationLayer.GenericExceptionReport;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private RoomCreateResponse MapToRoomCreateResponse(CinemaRoom cinemaRoom, int rowsAffected, int roomAddCount, int seatAddCount)
        {
            return new RoomCreateResponse
            {
                RowsAffected = rowsAffected,
                RoomAddCount = roomAddCount,
                SeatAddCount = seatAddCount,
                RoomId = cinemaRoom.RoomId,
                Name = cinemaRoom.Name,
                Rows = cinemaRoom.Rows,
                Columns = cinemaRoom.Columns,
                Status = cinemaRoom.Status,
                ListOfSeatsCreated = cinemaRoom.Seats.Select(seat => new SeatForRoomCreateResponse
                {
                    RowLabel = seat.RowLabel,
                    ColumnNumber = seat.ColumnNumber,
                    Location = $"{seat.RowLabel}{seat.ColumnNumber}"
                }).ToList() ?? new List<SeatForRoomCreateResponse>(),
            };
        }

        private string ConvertIntToRowLabel(int rowIndex)
        {
            int num = rowIndex + 1;
            string label = "";

            while (num > 0)
            {
                int remainder = num % 26;

                if (remainder == 0)
                {
                    label = 'Z' + label;
                    num = (num / 26) - 1;
                }
                else
                {
                    label = (char)('A' + remainder - 1) + label;
                    num = num / 26;
                }
            }
            return label;
        }

        public async Task<RoomCreateResponse> AddRoomWithSeatsAsync(RoomCreateRequest roomCreateRequest)
        {
            var room = new CinemaRoom
            {
                Name = roomCreateRequest.Name,
                Rows = roomCreateRequest.Rows,
                Columns = roomCreateRequest.Columns,
                Status = "Active"
            };

            for (int i = 0; i < roomCreateRequest.Rows; i++)
            {
                string rowLabel = ConvertIntToRowLabel(i);

                for (int j = 1; j <= roomCreateRequest.Columns; j++)
                {
                    var seat = new Seat
                    {
                        RowLabel = rowLabel,
                        ColumnNumber = j,
                        SeatTypeId = 1,
                    };

                    room.Seats.Add(seat);
                }
            }

            await _unitOfWork.roomRepository.AddAsync(room);

            int rowAffected;

            try
            {
                rowAffected = await _unitOfWork.SaveChangesAsync();
            }
            catch (UniqueConstraintViolationException ex)
            {
                throw new RoomNameAlreadyExistsException(room.Name, "The provided room name is already in use.", ex);
            }
            catch (Exception ex)
            {
                // Catch any other unexpected exceptions from SaveChangesAsync or other service logic
                throw;
            }

            int roomCount = 1;
            int seatsCount = room.Seats.Count();

            return MapToRoomCreateResponse(room, rowAffected, roomCount, seatsCount);
        }


        public async Task<GetRoomWithSeatsByIdResponse?> GetRoomWithSeatsAsync(int searchedRoomId)
        {
            var roomResult = await _unitOfWork.roomRepository.GetRoomByIdAsync(searchedRoomId);

            if (roomResult == null)
            {
                return null;
            }

            var seatsOfRoomList = await _unitOfWork.seatRepository.GetSeatsForRoomAsync(searchedRoomId);

            var searchResult = new GetRoomWithSeatsByIdResponse
            {
                RoomId = roomResult.RoomId,
                RoomName = roomResult.Name,
                Rows = roomResult.Rows,
                Columns = roomResult.Columns,
                Status = roomResult.Status,
                ListOfSeats = seatsOfRoomList?.Select(s => new SeatOfRoomDTO
                {
                    SeatId = s.SeatId,
                    RowLabel = s.RowLabel,
                    ColumnNumber = s.ColumnNumber,
                    SeatTypeName = s.SeatTypeName,
                    SeatPrice = s.SeatPrice,
                    PairedWithSeatId = s.PairedWithSeatId,
                    PairedWithSeatLocation = s.PairedWithSeatLocation,
                })
                .OrderBy(sDto => sDto.RowLabel)
                .ThenBy(sDto => sDto.ColumnNumber)
                .ToList(),
            };

            return searchResult;
        }


        public async Task<bool> DeleteRoomAsync(int deleteRoomId)
        {
            var findRoomToDelete = await _unitOfWork.roomRepository.GetRoomByIdAsync(deleteRoomId);

            if (findRoomToDelete == null)
            {
                return false;
            }

            findRoomToDelete.Status = "UnActive";

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UnDeleteRoomAsync(int unDeleteRoomId)
        {
            var findRoomToUnDelete = await _unitOfWork.roomRepository.GetRoomByIdAsync(unDeleteRoomId);

            if (findRoomToUnDelete == null)
            {
                return false;
            }

            findRoomToUnDelete.Status = "Active";

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<PagedResult<GetAllRoomResponse>> GetAllRoomAsync(GetAllRoomRequest getAllRoomRequest)
        {
            var rooms = await _unitOfWork.roomRepository
                .GetAllRoomAsync((getAllRoomRequest.Page - 1) * getAllRoomRequest.PageSize, getAllRoomRequest.PageSize);

            var totalItems = await _unitOfWork.roomRepository.GetTotalRoomsCountAsync();

            var roomResponse = rooms.Select(
                s => new GetAllRoomResponse
                {
                    RoomId = s.RoomId,
                    RoomName = s.RoomName,
                    Rows = s.Rows,
                    Columns = s.Columns,
                    RoomStatus = s.RoomStatus,
                    SeatsCount = s.SeatsCountTotal,
                });


            return new PagedResult<GetAllRoomResponse>
            {
                Items = roomResponse.ToList(),
                TotalItems = totalItems,
                Page = getAllRoomRequest.Page,
                PageSize = getAllRoomRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)getAllRoomRequest.PageSize)
            };
        }
    }
}
