using MV.ApplicationLayer.DTO.RequestModel;
using MV.ApplicationLayer.DTO.ResponseModel;
using MV.ApplicationLayer.HelperMethodsForThirdParty;
using MV.ApplicationLayer.QuarztInterfaces;
using MV.ApplicationLayer.RepositoryInterfaces;
using MV.ApplicationLayer.ServiceInterfaces;
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

        public async Task<bool> AddShowTimeAsync(ShowtimeAddRequest showtimeAddRequest)
        {
            var movieStatusCheck = await _unitOfWork.movieRepository.CheckMovieStatusByIdAsync(showtimeAddRequest.MovieId);

            if (movieStatusCheck == null)
            {
                return false;
            }

            var rawStartTime = DateTime.SpecifyKind(showtimeAddRequest.StartTime, DateTimeKind.Unspecified);
            
            DateTime finalStartTime = AdjustShowtimeDateTime(rawStartTime);

            var rawEndTime = finalStartTime.AddMinutes(movieStatusCheck.Duration);

            DateTime finalEndTime = AdjustShowtimeDateTime(rawEndTime);


            var obj = new Showtime
            {
                StartTime = finalStartTime,
                //Test
                //EndTime = DateTime.SpecifyKind(showtimeAddRequest.EndTime, DateTimeKind.Unspecified), 
                EndTime = finalEndTime,
                MovieId = showtimeAddRequest.MovieId,
                MovieDuration = movieStatusCheck.Duration,
                Status = "Scheduled",
            };

            await _unitOfWork.showtimeRepository.AddAsync(obj);

            await _unitOfWork.SaveChangesAsync();

            await _jobScheduler.ScheduleShowtimeStatusUpdateAsync(obj);


            return true;
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

    }
}
