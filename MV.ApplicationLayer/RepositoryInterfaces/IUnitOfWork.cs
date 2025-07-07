namespace MV.ApplicationLayer.RepositoryInterfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //Repo interfaces
        IUserRepository userRepository { get; }
        IEmployeeRepository employeeRepository { get; }
        IMovieRepository movieRepository { get; }
        IGenreRepository genreRepository { get; }
        IPromotionRepository promotionRepository { get; }
        IRoomRepository roomRepository { get; }
        ISeatRepository seatRepository { get; }
        ICoupleSeatRepository coupleSeatRepository { get; }
        IFoodCategoryRepository foodCategoryRepository { get; }
        IFoodRepository foodRepository { get; }
        ISeatTypeRepository seatTypeRepository { get; }
        IRoomTypeRepository roomTypeRepository { get; }
        ICommentRatingRepository commentRatingRepository { get; }
        IShowtimeRoomInstanceRepository showtimeRoomInstanceRepository { get; }
        ISeatDataForShowtimeRepository seatDataForShowtimeRepository { get; }
        ITicketInvoiceRepository ticketInvoiceRepository { get; }
        IScoreRepository scoreRepository { get; }
        IScoreHistoryRepository scoreHistoryRepository { get; }

        IShowtimeRepository showtimeRepository { get; }

        IDashboardRepository dashboardRepository { get; }
        //Single commit point
        Task<int> SaveChangesAsync();
    }
}
