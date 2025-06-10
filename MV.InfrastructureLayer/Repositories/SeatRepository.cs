using Microsoft.EntityFrameworkCore;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;
using MV.InfrastructureLayer.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.InfrastructureLayer.Repositories
{
    public class SeatRepository : ISeatRepository
    {
        private readonly MovietheatermanagementContext _context;
        public SeatRepository(MovietheatermanagementContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SeatsOfRoomCustom>> GetSeatsForRoomAsync(int roomId)
        {
            var getSeats = await _context.Set<Seat>()
                .Where(s => s.RoomId == roomId)
                .Select(s => new SeatsOfRoomCustom
                {
                    SeatId = s.SeatId,
                    RowLabel = s.RowLabel,
                    ColumnNumber = s.ColumnNumber,
                    SeatTypeName = s.SeatType != null ? s.SeatType.SeatTypeName : null,
                    SeatPrice = s.SeatType != null ? s.SeatType.SeatTypePrice : 0,

                    PairedWithSeatId = (s.SeatType != null && s.SeatType.SeatTypeName == "Couple")
                    ? (s.CoupleSeatSeatId1Navigation != null
                        ? s.CoupleSeatSeatId1Navigation.SeatId2Navigation.SeatId
                        : (s.CoupleSeatSeatId2Navigation != null
                            ? s.CoupleSeatSeatId2Navigation.SeatId1Navigation.SeatId
                            : (int?)null))
                    : (int?)null,

                    PairedWithSeatLocation = (s.SeatType != null && s.SeatType.SeatTypeName == "Couple")
                    ? (s.CoupleSeatSeatId1Navigation != null
                        ? $"{s.CoupleSeatSeatId1Navigation.SeatId2Navigation.RowLabel}{s.CoupleSeatSeatId1Navigation.SeatId2Navigation.ColumnNumber}"
                        : (s.CoupleSeatSeatId2Navigation != null
                            ? $"{s.CoupleSeatSeatId2Navigation.SeatId1Navigation.RowLabel}{s.CoupleSeatSeatId2Navigation.SeatId1Navigation.ColumnNumber}"
                            : null))
                    : null

                })
                .ToListAsync();

            return getSeats;
        }


        
    }
}
