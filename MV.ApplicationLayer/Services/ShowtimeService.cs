using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.GenericExceptionReport;
using MV.ApplicationLayer.HelperMethodsForThirdParty;
using MV.ApplicationLayer.QuarztInterfaces;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
using MV.ApplicationLayer.SpecificExceptionReport;
using MV.DomainLayer.CustomQueryModels;
using MV.DomainLayer.Entities;


namespace MV.ApplicationLayer.Services
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJobScheduler _jobScheduler;


        public ShowtimeService(IUnitOfWork unitOfWork, IJobScheduler jobScheduler)
        {
            _unitOfWork = unitOfWork;
            _jobScheduler = jobScheduler;
        }

        private DateTime AdjustShowtimeDateTime(DateTime dateTime)
        {
            // 1. Strip seconds, milliseconds, and ticks to ensure 0 seconds
            // Create a new DateTime up to the minute level, preserving the original kind
            DateTime adjustedDateTime = new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                0, // Set seconds to 0
                dateTime.Kind // Preserve the DateTimeKind (Unspecified in your case)
            );

            // 2. Round minutes up to the nearest 0 or 5
            int currentMinute = adjustedDateTime.Minute;
            int minutesToRoundUp = 0;

            if (currentMinute % 5 != 0)
            {
                minutesToRoundUp = 5 - (currentMinute % 5);
            }

            // Add the calculated minutes. This correctly handles hour/day rollovers.
            adjustedDateTime = adjustedDateTime.AddMinutes(minutesToRoundUp);

            return adjustedDateTime;
        }

        public async Task<ShowtimeAddResponse> AddShowTimeAsync(ShowtimeAddRequest showtimeAddRequest)
        {
            var movieStatusCheck = await _unitOfWork.movieRepository.CheckMovieStatusByIdAsync(showtimeAddRequest.MovieId);

            if (movieStatusCheck == null)
            {
                return null!;
            }

            //var rawStartTime = DateTime.SpecifyKind(showtimeAddRequest.StartTime, DateTimeKind.Unspecified);

            DateTime startTimeUtc = showtimeAddRequest.StartTime;

            DateTime finalStartTime = AdjustShowtimeDateTime(startTimeUtc);

            //var rawEndTime = finalStartTime.AddMinutes(movieStatusCheck.Duration);

            DateTime finalEndTime = AdjustShowtimeDateTime(finalStartTime.AddMinutes(movieStatusCheck.Duration));


            var newShowtime = new Showtime
            {
                StartTime = finalStartTime,
                //Test
                //EndTime = DateTime.SpecifyKind(showtimeAddRequest.EndTime, DateTimeKind.Unspecified), 
                EndTime = finalEndTime,
                MovieId = showtimeAddRequest.MovieId,
                MovieDuration = movieStatusCheck.Duration,
                Status = "Scheduled",
            };

            var listAddRoomId = showtimeAddRequest.OriginalRoomIdList;

            var listRoomDataFromId = await _unitOfWork.roomRepository.GetListRoomDataForShowtimeAddAsync(listAddRoomId!);

            var allSeats = await _unitOfWork.seatRepository.GetSeatsForRoomInstanceAsync(listAddRoomId!);

            var seatsByRoomId = allSeats
                .GroupBy(s => s.OriginalRoomId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var room in listRoomDataFromId)
            {
                var seatsForThisRoom = seatsByRoomId.GetValueOrDefault(room.OriginalRoomId, new List<SeatOfRoomForAddShowtimeInstance>());

                var instance = new ShowtimeRoomInstance
                {
                    OriginalRoomId = room.OriginalRoomId,
                    RoomName = room.RoomName,
                    RoomRows = room.RoomRows,
                    RoomColumns = room.RoomColumns,
                    RoomTypeName = room.RoomTypeName,
                    RoomTypePrice = room.RoomTypePrice,
                    ActualStartTime = finalStartTime,
                    ActualEndTime = finalEndTime,
                    MoviePrice = movieStatusCheck.MoviePrice,
                    Status = newShowtime.Status,
                    AddedAt = DateTime.UtcNow,
                    SeatDataForShowtimes = seatsForThisRoom
                    .Select(
                        seat => new SeatDataForShowtime
                        {
                            RowLabel = seat.RowLabel,
                            ColumnNumber = seat.ColumnNumber,
                            SeatTypeName = seat.SeatTypeName,
                            SeatTypePrice = seat.SeatTypePrice,
                            PairedWithSeatLocation = seat.PairedWithSeatLocation,
                            Status = "Active"
                        })
                    .ToList()
                };
                newShowtime.ShowtimeRoomInstances.Add(instance);
            }

            await _unitOfWork.showtimeRepository.AddAsync(newShowtime);

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (ExcludeConstraintViolationException ex)
            {
                throw new ShowtimeRoomInstanceIsUnAvailableException(newShowtime.ShowtimeId.ToString(), "Some rooms is being used at this time range", ex);
            }
            catch (Exception ex)
            {
                throw;
            }

            await _jobScheduler.ScheduleShowtimeStatusUpdateAsync(newShowtime);

            var addResult = new ShowtimeAddResponse
            {
                ShowtimeId = newShowtime.ShowtimeId,
                MovieId = newShowtime.MovieId,
                StartTime = newShowtime.StartTime,
                EndTime = newShowtime.EndTime,
                MovieDuration = newShowtime.MovieDuration,
                Status = newShowtime.Status,
                RoomInstanceCount = newShowtime.ShowtimeRoomInstances.Count(),
            };

            return addResult;
        }

        public async Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetAllShowtimeDataOnly
            (GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest)
        {
            var showtimes = await _unitOfWork.showtimeRepository.GetAllShowtimeWithDataOnlyAsync
                ((getAllShowtimeWithDataOnlyRequest.Page - 1) * getAllShowtimeWithDataOnlyRequest.PageSize
                , getAllShowtimeWithDataOnlyRequest.PageSize);

            var totalItems = await _unitOfWork.showtimeRepository.GetTotalShowtimeCountAsync();

            var showtimeResponse = showtimes.Select(
                s => new GetAllShowtimeWithDataOnlyResponse
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.MovieTitle,
                    Status = s.Status,
                });

            return new PagedResult<GetAllShowtimeWithDataOnlyResponse>
            {
                Items = showtimeResponse.ToList(),
                TotalItems = totalItems,
                Page = getAllShowtimeWithDataOnlyRequest.Page,
                PageSize = getAllShowtimeWithDataOnlyRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)getAllShowtimeWithDataOnlyRequest.PageSize)
            };

        }

        public async Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetNowShowingShowtimeDataOnly
            (GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest)
        {
            var showtimes = await _unitOfWork.showtimeRepository.GetNowShowingShowtimeWithDataOnlyAsync
                ((getAllShowtimeWithDataOnlyRequest.Page - 1) * getAllShowtimeWithDataOnlyRequest.PageSize
                , getAllShowtimeWithDataOnlyRequest.PageSize);

            var totalItems = await _unitOfWork.showtimeRepository.GetTotalNowShowingShowtimeCountAsync();

            var showtimeResponse = showtimes.Select(
                s => new GetAllShowtimeWithDataOnlyResponse
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.MovieTitle,
                    Status = s.Status,
                });

            return new PagedResult<GetAllShowtimeWithDataOnlyResponse>
            {
                Items = showtimeResponse.ToList(),
                TotalItems = totalItems,
                Page = getAllShowtimeWithDataOnlyRequest.Page,
                PageSize = getAllShowtimeWithDataOnlyRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)getAllShowtimeWithDataOnlyRequest.PageSize)
            };

        }

        public async Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetScheduledShowtimeDataOnly
            (GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest)
        {
            var showtimes = await _unitOfWork.showtimeRepository.GetScheduledShowtimeWithDataOnlyAsync
                ((getAllShowtimeWithDataOnlyRequest.Page - 1) * getAllShowtimeWithDataOnlyRequest.PageSize
                , getAllShowtimeWithDataOnlyRequest.PageSize);

            var totalItems = await _unitOfWork.showtimeRepository.GetTotalScheduledShowtimeCountAsync();

            var showtimeResponse = showtimes.Select(
                s => new GetAllShowtimeWithDataOnlyResponse
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.MovieTitle,
                    Status = s.Status,
                });

            return new PagedResult<GetAllShowtimeWithDataOnlyResponse>
            {
                Items = showtimeResponse.ToList(),
                TotalItems = totalItems,
                Page = getAllShowtimeWithDataOnlyRequest.Page,
                PageSize = getAllShowtimeWithDataOnlyRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)getAllShowtimeWithDataOnlyRequest.PageSize)
            };
        }

        public async Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetFinishedShowtimeDataOnly
            (GetAllShowtimeWithDataOnlyRequest getAllShowtimeWithDataOnlyRequest)
        {
            var showtimes = await _unitOfWork.showtimeRepository.GetFinishedShowtimeWithDataOnlyAsync
                ((getAllShowtimeWithDataOnlyRequest.Page - 1) * getAllShowtimeWithDataOnlyRequest.PageSize
                , getAllShowtimeWithDataOnlyRequest.PageSize);

            var totalItems = await _unitOfWork.showtimeRepository.GetTotalFinishedShowtimeCountAsync();

            var showtimeResponse = showtimes.Select(
                s => new GetAllShowtimeWithDataOnlyResponse
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.MovieTitle,
                    Status = s.Status,
                });

            return new PagedResult<GetAllShowtimeWithDataOnlyResponse>
            {
                Items = showtimeResponse.ToList(),
                TotalItems = totalItems,
                Page = getAllShowtimeWithDataOnlyRequest.Page,
                PageSize = getAllShowtimeWithDataOnlyRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)getAllShowtimeWithDataOnlyRequest.PageSize)
            };
        }

        public async Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetShowtimeDataOnlyByMovie
            (GetAllShowtimeWithDataOnlyByMovieRequest getAllShowtimeWithDataOnlyByMovieRequest)
        {
            var showtimes = await _unitOfWork.showtimeRepository.GetShowtimeWithDataOnlyByMovieAsync
                ((getAllShowtimeWithDataOnlyByMovieRequest.Page - 1) * getAllShowtimeWithDataOnlyByMovieRequest.PageSize
                , getAllShowtimeWithDataOnlyByMovieRequest.PageSize, getAllShowtimeWithDataOnlyByMovieRequest.MovieId);

            var totalItems = await _unitOfWork.showtimeRepository.GetTotalShowtimeByMovieCountAsync(getAllShowtimeWithDataOnlyByMovieRequest.MovieId);

            var showtimeResponse = showtimes.Select(
                s => new GetAllShowtimeWithDataOnlyResponse
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.MovieTitle,
                    Status = s.Status,
                });

            return new PagedResult<GetAllShowtimeWithDataOnlyResponse>
            {
                Items = showtimeResponse.ToList(),
                TotalItems = totalItems,
                Page = getAllShowtimeWithDataOnlyByMovieRequest.Page,
                PageSize = getAllShowtimeWithDataOnlyByMovieRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)getAllShowtimeWithDataOnlyByMovieRequest.PageSize)
            };
        }

        public async Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetShowtimeDataOnlyByDate
            (ShowtimeGetByDateRequest showtimeGetByDateRequest)
        {
            var showtimes = await _unitOfWork.showtimeRepository.GetShowtimeWithDataOnlyByDateAsync
                ((showtimeGetByDateRequest.Page - 1) * showtimeGetByDateRequest.PageSize
                , showtimeGetByDateRequest.PageSize, showtimeGetByDateRequest.SearchedDate);

            var totalItems = await _unitOfWork.showtimeRepository.GetTotalShowtimeByDateCountAsync(showtimeGetByDateRequest.SearchedDate);

            var showtimeResponse = showtimes.Select(
                s => new GetAllShowtimeWithDataOnlyResponse
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.MovieTitle,
                    Status = s.Status,
                });

            return new PagedResult<GetAllShowtimeWithDataOnlyResponse>
            {
                Items = showtimeResponse.ToList(),
                TotalItems = totalItems,
                Page = showtimeGetByDateRequest.Page,
                PageSize = showtimeGetByDateRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)showtimeGetByDateRequest.PageSize)
            };
        }

        public async Task<PagedResult<GetAllShowtimeWithDataOnlyResponse>> GetShowtimeDataOnlyByDateRange
            (ShowtimeGetByDateRangeRequest showtimeGetByDateRangeRequest)
        {
            var showtimes = await _unitOfWork.showtimeRepository.GetShowtimeWithDataOnlyByDateRangeAsync
                ((showtimeGetByDateRangeRequest.Page - 1) * showtimeGetByDateRangeRequest.PageSize
                , showtimeGetByDateRangeRequest.PageSize, showtimeGetByDateRangeRequest.StartDate, showtimeGetByDateRangeRequest.EndDate);

            var totalItems = await _unitOfWork.showtimeRepository.GetTotalShowtimeByDateRangeCountAsync(showtimeGetByDateRangeRequest.StartDate, showtimeGetByDateRangeRequest.EndDate);

            var showtimeResponse = showtimes.Select(
                s => new GetAllShowtimeWithDataOnlyResponse
                {
                    ShowtimeId = s.ShowtimeId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    MovieId = s.MovieId,
                    MovieTitle = s.MovieTitle,
                    Status = s.Status,
                });

            return new PagedResult<GetAllShowtimeWithDataOnlyResponse>
            {
                Items = showtimeResponse.ToList(),
                TotalItems = totalItems,
                Page = showtimeGetByDateRangeRequest.Page,
                PageSize = showtimeGetByDateRangeRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)showtimeGetByDateRangeRequest.PageSize)
            };
        }

        public async Task<PagedResult<RoomGetByTimeRangeForShowtimeAddResponse>> GetAllAvailableRoomForShowtimeAdd
            (RoomGetByTimeRangeForShowtimeAddRequest roomGetByTimeRangeForShowtimeAddRequest)
        {
            var movieStatusCheck = await _unitOfWork.movieRepository.CheckMovieStatusByIdAsync(roomGetByTimeRangeForShowtimeAddRequest.MovieId);

            if (movieStatusCheck == null)
            {
                return null!;
            }

            //var rawStartTime = DateTime.SpecifyKind(roomGetByTimeRangeForShowtimeAddRequest.StartTime, DateTimeKind.Unspecified);

            DateTime startTimeUtc = roomGetByTimeRangeForShowtimeAddRequest.StartTime;

            DateTime finalStartTime = AdjustShowtimeDateTime(startTimeUtc);

            var rawEndTime = finalStartTime.AddMinutes(movieStatusCheck.Duration);

            DateTime finalEndTime = AdjustShowtimeDateTime(rawEndTime);

            var listUnAvailableRoomId = await _unitOfWork.showtimeRoomInstanceRepository.GetListUnAvailableRoomIdAtTimeAsync
                (finalStartTime, finalEndTime);

            var availableRoomList = await _unitOfWork.roomRepository.GetListAvailalbeRoomForInstanceAsync
                ((roomGetByTimeRangeForShowtimeAddRequest.Page - 1) * roomGetByTimeRangeForShowtimeAddRequest.PageSize
                , roomGetByTimeRangeForShowtimeAddRequest.PageSize, listUnAvailableRoomId);

            var totalItems = await _unitOfWork.roomRepository.GetTotalRoomForInstanceCountAsync(listUnAvailableRoomId);

            var roomForInstanceResponse = availableRoomList.Select
                (
                rfs => new RoomGetByTimeRangeForShowtimeAddResponse
                {
                    OriginalRoomId = rfs.RoomId,
                    RoomName = rfs.RoomName,
                    RoomStatus = rfs.RoomStatus,
                    Rows = rfs.Rows,
                    Columns = rfs.Column,
                    RoomTypeName = rfs.RoomTypeName,
                    RoomTypePrice = rfs.RoomTypePrice,
                });
            return new PagedResult<RoomGetByTimeRangeForShowtimeAddResponse>
            {
                Items = roomForInstanceResponse.ToList(),
                TotalItems = totalItems,
                Page = roomGetByTimeRangeForShowtimeAddRequest.Page,
                PageSize = roomGetByTimeRangeForShowtimeAddRequest.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)roomGetByTimeRangeForShowtimeAddRequest.PageSize)
            };
        }


    }
}
