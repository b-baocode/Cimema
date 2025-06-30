using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IShowtimeService
    {
        Task<ShowtimeAddResponse> AddShowTimeAsync(ShowtimeAddRequest showtimeAddRequest);
        Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetAllShowtimeDataOnly(GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest);
        Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetNowShowingShowtimeDataOnly
            (GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest);
        Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetScheduledShowtimeDataOnly
            (GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest);
        Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetFinishedShowtimeDataOnly
            (GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest);
        Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetShowtimeDataOnlyByMovie
            (GetAllShowtimeWithDataOnlyByMovieRequest getAllShowtimeWithDataOnlyByMovieRequest);
        Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetShowtimeDataOnlyByDate
            (ShowtimeGetByDateRequest showtimeGetByDateRequest);
        Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetShowtimeDataOnlyByDateRange
            (ShowtimeGetByDateRangeRequest showtimeGetByDateRangeRequest);

        Task<PagedResult<RoomGetByTimeRangeForShowtimeAddResponse>> GetAllAvailableRoomForShowtimeAdd
            (RoomGetByTimeRangeForShowtimeAddRequest roomGetByTimeRangeForShowtimeAddRequest);

        Task<ShowtimeGetByIdResponse> GetShowtimeByIdWithAllRoomInstance(ShowtimeGetByIdRequest showtimeGetByIdRequest);
    }
}
