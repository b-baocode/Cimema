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
            var standardSeatTypeId = await _unitOfWork.seatTypeRepository.GetStandardSeatTypeIdAsync();

            //var standardRoomTypeId = await _unitOfWork.roomTypeRepository.GetStandardRoomTypeIdAsync();

            var room = new CinemaRoom
            {
                Name = roomCreateRequest.Name,
                Rows = roomCreateRequest.Rows,
                Columns = roomCreateRequest.Columns,
                RoomTypeId = roomCreateRequest.RoomTypeId,
                CreatedAt = DateTime.Now,
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
                        SeatTypeId = standardSeatTypeId,
                        Status = "Active",
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
                throw;
            }

            int roomCount = 1;
            int seatsCount = room.Seats.Count();

            return MapToRoomCreateResponse(room, rowAffected, roomCount, seatsCount);
        }


        public async Task<GetRoomWithSeatsByIdResponse?> GetRoomWithSeatsByIdAsync(int searchedRoomId)
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
                RoomTypeName = roomResult.RoomType.RoomTypeName,
                RoomTypePrice = roomResult.RoomType.RoomTypePrice,
                RoomTypeStatus = roomResult.RoomType.Status,
                RoomCreateTime = roomResult.CreatedAt,
                RoomUpdateTime = roomResult.UpdatedAt,
                RoomStatus = roomResult.Status,
                StandardSeatCount = seatsOfRoomList.Count(s => s.SeatTypeName == "Standard"),
                VipSeatCount = seatsOfRoomList.Count(s => s.SeatTypeName == "VIP"),
                CoupleSeatCount = seatsOfRoomList.Count(s => s.SeatTypeName == "Couple") / 2,
                ListOfSeats = seatsOfRoomList?.Select(s => new SeatOfRoomDTO
                {
                    SeatId = s.SeatId,
                    RowLabel = s.RowLabel,
                    ColumnNumber = s.ColumnNumber,
                    SeatTypeName = s.SeatTypeName,
                    SeatPrice = s.SeatPrice,
                    PairedWithSeatId = s.PairedWithSeatId,
                    PairedWithSeatLocation = s.PairedWithSeatLocation,
                    SeatStatus = s.SeatStatus,

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

            findRoomToDelete.Status = "InActive";

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
                    RoomTypeName = s.RoomTypeName,
                    RoomTypePrice = s.RoomTypePrice,
                    RoomTypeStatus = s.RoomTypeStatus,
                    RoomCreateTime = s.CreatedAt,
                    RoomUpdateTime = s.UpdatedAt,
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

        public async Task<RoomUpdateResponse?> UpdateRoomWithSeatsAsync(RoomUpdateRequest roomUpdateRequest, int updateRoomId)
        {
            var existingRoom = await _unitOfWork.roomRepository.GetRoomByIdAsync(updateRoomId);

            if (existingRoom == null)
            {
                return null;
            }

            int standardSeatTypeId = await _unitOfWork.seatTypeRepository.GetStandardSeatTypeIdAsync();

            //Delete all couple seats
            var allCoupleSeatsInRoom = await _unitOfWork.coupleSeatRepository.GetAllCoupleSeatsForRoomAsync(updateRoomId);
            int coupleSeatsRemovedCount = 0;
            foreach (var coupleSeat in allCoupleSeatsInRoom)
            {
                _unitOfWork.coupleSeatRepository.Remove(coupleSeat);
                coupleSeatsRemovedCount++;
            }

            var existingSeatsToCheck = await _unitOfWork.seatRepository
                .GetExistingSeatsForRoomUpdateCheckAsync(updateRoomId);

            existingRoom.Name = roomUpdateRequest.Name;
            existingRoom.Rows = roomUpdateRequest.Rows;
            existingRoom.Columns = roomUpdateRequest.Columns;
            existingRoom.RoomTypeId = roomUpdateRequest.RoomTypeId;
            existingRoom.UpdatedAt = DateTime.Now;
            existingRoom.Status = "Active";

            var seatsToDeactivate = new List<Seat>();
            var seatsToReactivate = new List<Seat>();
            var seatsToAdd = new List<Seat>();
            //var seatsToKeep = new List<Seat>();

            var newTargetSeatLocations = new HashSet<string>();
            for (int i = 0; i < roomUpdateRequest.Rows; i++)
            {
                string rowLabel = ConvertIntToRowLabel(i);
                for (int j = 1; j <= roomUpdateRequest.Columns; j++)
                {
                    newTargetSeatLocations.Add($"{rowLabel}-{j}");
                }
            }

            foreach (var existingSeat in existingSeatsToCheck)
            {
                string existingSeatLocationKey = $"{existingSeat.RowLabel}-{existingSeat.ColumnNumber}";

                if (newTargetSeatLocations.Contains(existingSeatLocationKey))
                {
                    bool wasInactive = existingSeat.Status == "InActive";

                    if (wasInactive)
                    {
                        existingSeat.Status = "Active";
                        seatsToReactivate.Add(existingSeat);
                    }


                    //seatsToKeep.Add(existingSeat);
                    newTargetSeatLocations.Remove(existingSeatLocationKey);
                }
                else
                {
                    if (existingSeat.Status == "Active")
                    {
                        existingSeat.Status = "InActive";
                        seatsToDeactivate.Add(existingSeat);
                    }
                }
                existingSeat.SeatTypeId = standardSeatTypeId;
            }

            if (newTargetSeatLocations.Count > 0)
            {
                foreach (string newLocationKey in newTargetSeatLocations)
                {
                    var parts = newLocationKey.Split('-');
                    var newSeat = new Seat
                    {
                        RoomId = existingRoom.RoomId,
                        RowLabel = parts[0],
                        ColumnNumber = int.Parse(parts[1]),
                        SeatTypeId = standardSeatTypeId,
                        Status = "Active"
                    };
                    seatsToAdd.Add(newSeat);
                    await _unitOfWork.seatRepository.AddAsync(newSeat);
                }
            }

            //var deactivatedSeatIds = seatsToDeactivate.Select(s => s.SeatId).ToList();

            //if (deactivatedSeatIds.Any())
            //{
            //    var coupleSeatsToDelete = await _unitOfWork.coupleSeatRepository.GetCoupleSeatsBySeatIdsAsync(deactivatedSeatIds);

            //    foreach (var coupleSeat in coupleSeatsToDelete)
            //    {
            //        _unitOfWork.coupleSeatRepository.Remove(coupleSeat);
            //    }
            //}

            int rowsAffected;

            try
            {
                rowsAffected = await _unitOfWork.SaveChangesAsync();
            }
            catch (UniqueConstraintViolationException ex)
            {
                throw new RoomNameAlreadyExistsException(roomUpdateRequest.Name, "The room name already exist.", ex);
            }
            catch (Exception ex)
            {
                throw;
            }

            var response = new RoomUpdateResponse
            {
                RoomId = existingRoom.RoomId,
                RoomName = existingRoom.Name,
                Rows = existingRoom.Rows,
                Columns = existingRoom.Columns,
                RoomTypeId = existingRoom.RoomTypeId,
                RowsAffected = rowsAffected,
                RoomStatus = existingRoom.Status,
                SeatsAddedCount = seatsToAdd.Count,
                SeatsDeactivatedCount = seatsToDeactivate.Count,
                SeatsReactivatedCount = seatsToReactivate.Count,
                CoupleSeatsDeleted = coupleSeatsRemovedCount,
            };

            return response;

        }
    }
}
