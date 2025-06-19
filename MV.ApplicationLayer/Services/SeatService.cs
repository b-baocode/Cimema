using MV.ApplicationLayer.DTO.RequestModel;
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
    public class SeatService : ISeatService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SeatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> SetTypeForSeatsAsync(SeatSetTypeRequest seatSetTypeRequest, int roomId)
        {
            var findRoomWithSeatsById = await _unitOfWork.roomRepository.CheckRoomExistAsync(roomId);

            if (!findRoomWithSeatsById)
            {
                return false;
            }

            var currentSeatsInRoom = (await _unitOfWork.seatRepository.GetSeatsOfRoomForSetTypeAsync(roomId))
                                 .ToDictionary(s => s.SeatId);

            var requestedStandardIds = new HashSet<int>(seatSetTypeRequest.standardSeatsIdList ?? new List<int>());

            var requestedVipIds = new HashSet<int>(seatSetTypeRequest.vipSeatsIdList ?? new List<int>());

            var requestedCouplePairs = new HashSet<(int, int)>();

            foreach (var reqCouple in seatSetTypeRequest.coupleSeatsList ?? new List<CoupleSeatRequest>())
            {
                if (!currentSeatsInRoom.ContainsKey(reqCouple.Seat1Id) || !currentSeatsInRoom.ContainsKey(reqCouple.Seat2Id))
                {
                    continue;
                }
                requestedCouplePairs.Add((Math.Min(reqCouple.Seat1Id, reqCouple.Seat2Id), Math.Max(reqCouple.Seat1Id, reqCouple.Seat2Id)));
            }

            var explicitlyAssignedSeatIds = new HashSet<int>();

            var coupleSeatsToAdd = new List<CoupleSeat>();

            var coupleSeatsToDelete = new List<CoupleSeat>();

            foreach (var seat in currentSeatsInRoom.Values)
            {
                if (requestedStandardIds.Contains(seat.SeatId))
                {
                    seat.SeatTypeId = 1; //need fix not hard code
                    explicitlyAssignedSeatIds.Add(seat.SeatId);
                }
                else if (requestedVipIds.Contains(seat.SeatId))
                {
                    seat.SeatTypeId = 2; //need fix not hard code
                    explicitlyAssignedSeatIds.Add(seat.SeatId);
                }
            }

            //var existingCoupleEntities = await _unitOfWork.coupleSeatRepository.GetAllCoupleSeatsOfRoomToAdd(currentSeatsInRoom);

            var existingCoupleEntities = await _unitOfWork.coupleSeatRepository.GetAllCoupleSeatsOfRoomInPairsByRoomIdAsync(roomId);

            var existingCouplePairs = new HashSet<(int, int)>();

            foreach (var existingCouple in existingCoupleEntities)
            {
                existingCouplePairs.Add((Math.Min(existingCouple.SeatId1, existingCouple.SeatId2), Math.Max(existingCouple.SeatId1, existingCouple.SeatId2)));
            }


            // Identify CoupleSeats to Add and Update Seat Types for new couples
            foreach (var requestedPair in requestedCouplePairs)
            {
                var seat1Id = requestedPair.Item1;
                var seat2Id = requestedPair.Item2;

                // Update SeatType for these two seats
                if (currentSeatsInRoom.TryGetValue(seat1Id, out var seat1))
                {
                    seat1.SeatTypeId = 3; //need fix not hard code
                    explicitlyAssignedSeatIds.Add(seat1Id);
                }
                if (currentSeatsInRoom.TryGetValue(seat2Id, out var seat2))
                {
                    seat2.SeatTypeId = 3; //need fix not hard code
                    explicitlyAssignedSeatIds.Add(seat2Id);
                }

                // If this pair doesn't exist as a CoupleSeat entity yet, add it
                if (!existingCouplePairs.Contains(requestedPair))
                {
                    coupleSeatsToAdd.Add(new CoupleSeat { SeatId1 = seat1Id, SeatId2 = seat2Id });
                }
            }

            // Identify CoupleSeats to Delete and revert Seat Types for broken couples
            foreach (var existingCouple in existingCoupleEntities)
            {
                var existingPair = (Math.Min(existingCouple.SeatId1, existingCouple.SeatId2), Math.Max(existingCouple.SeatId1, existingCouple.SeatId2));

                // If an existing couple pair is NOT in the requested list, it needs to be deleted
                if (!requestedCouplePairs.Contains(existingPair))
                {
                    coupleSeatsToDelete.Add(existingCouple);

                    // Revert SeatType for the individual seats if they weren't reassigned
                    if (currentSeatsInRoom.TryGetValue(existingCouple.SeatId1, out var seat1))
                    {
                        if (!explicitlyAssignedSeatIds.Contains(seat1.SeatId))
                        {
                            seat1.SeatTypeId = 1; // Revert to standard
                        }
                    }
                    if (currentSeatsInRoom.TryGetValue(existingCouple.SeatId2, out var seat2))
                    {
                        if (!explicitlyAssignedSeatIds.Contains(seat2.SeatId))
                        {
                            seat2.SeatTypeId = 1; // Revert to standard
                        }
                    }
                }
            }


            if (coupleSeatsToAdd.Any())
            {
                await _unitOfWork.coupleSeatRepository.AddRangeAsync(coupleSeatsToAdd);
            }

            if (coupleSeatsToDelete.Any())
            {
                _unitOfWork.coupleSeatRepository.RemoveRangeAsync(coupleSeatsToDelete);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public (bool checkCouple,string errorMessage) CheckInvalidDoubleSeats(List<CoupleSeatRequest>? coupleSeatList)
        {
            if(coupleSeatList != null)
            {
                var requestedCouplePairs = new HashSet<(int, int)>();

                foreach (var reqCouple in coupleSeatList ?? new List<CoupleSeatRequest>())
                {
                    requestedCouplePairs.Add((Math.Min(reqCouple.Seat1Id, reqCouple.Seat2Id), Math.Max(reqCouple.Seat1Id, reqCouple.Seat2Id)));
                }

                var seatCounts = requestedCouplePairs
                    .SelectMany(pair => new[] { pair.Item1, pair.Item2 })
                    .GroupBy(seatId => seatId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var duplicates = seatCounts.Where(s => s.Value > 1).ToList();

                if (duplicates.Any())
                {
                    string errorMessage = "Invalid double seats: " +
                        string.Join(", ", duplicates.Select(d => $"{d.Key} (Count: {d.Value})"));
                    return (true, errorMessage);
                }
            }
            return (false, "Valid couple seats");
        }
    }
}
