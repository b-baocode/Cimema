using Microsoft.EntityFrameworkCore;
using MV.DomainLayer.Entities;

namespace MV.InfrastructureLayer.DBContext;

public partial class MovietheatermanagementContext : DbContext
{
    public MovietheatermanagementContext()
    {
    }

    public MovietheatermanagementContext(DbContextOptions<MovietheatermanagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CinemaRoom> CinemaRooms { get; set; }

    public virtual DbSet<CommentRating> CommentRatings { get; set; }

    public virtual DbSet<CoupleSeat> CoupleSeats { get; set; }

    public virtual DbSet<Food> Foods { get; set; }

    public virtual DbSet<FoodCategory> FoodCategories { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<PaymentOnline> PaymentOnlines { get; set; }

    public virtual DbSet<PaymentUpFront> PaymentUpFronts { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionEvent> PromotionEvents { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoomType> RoomTypes { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<ScoreHistory> ScoreHistories { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SeatDataForShowtime> SeatDataForShowtimes { get; set; }

    public virtual DbSet<SeatType> SeatTypes { get; set; }

    public virtual DbSet<Showtime> Showtimes { get; set; }

    public virtual DbSet<ShowtimeRoomInstance> ShowtimeRoomInstances { get; set; }

    public virtual DbSet<TicketDetail> TicketDetails { get; set; }

    public virtual DbSet<TicketInvoice> TicketInvoices { get; set; }

    public virtual DbSet<TicketInvoiceFoodItem> TicketInvoiceFoodItems { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
    //        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=movietheatermanagement;Username=postgres;Password=1;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("btree_gist");

        modelBuilder.Entity<CinemaRoom>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("CinemaRooms_pkey");

            entity.HasIndex(e => e.Name, "CinemaRooms_Name_key").IsUnique();

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(25);
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp(0) without time zone");

            entity.HasOne(d => d.RoomType).WithMany(p => p.CinemaRooms)
                .HasForeignKey(d => d.RoomTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cinemarooms_roomtypeid");
        });

        modelBuilder.Entity<CommentRating>(entity =>
        {
            entity.HasKey(e => e.CommentRatingId).HasName("CommentRating_pkey");

            entity.ToTable("CommentRating");

            entity.HasIndex(e => new { e.Userid, e.MovieId }, "uq_user_movie_rating").IsUnique();

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");

            entity.HasOne(d => d.Movie).WithMany(p => p.CommentRatings)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_commentrating_movieid");

            entity.HasOne(d => d.User).WithMany(p => p.CommentRatings)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_commentrating_userid");
        });

        modelBuilder.Entity<CoupleSeat>(entity =>
        {
            entity.HasKey(e => new { e.SeatId1, e.SeatId2 }).HasName("CoupleSeats_pkey");

            entity.HasIndex(e => e.SeatId1, "CoupleSeats_SeatId1_key").IsUnique();

            entity.HasIndex(e => e.SeatId2, "CoupleSeats_SeatId2_key").IsUnique();

            entity.HasOne(d => d.SeatId1Navigation).WithOne(p => p.CoupleSeatSeatId1Navigation)
                .HasForeignKey<CoupleSeat>(d => d.SeatId1)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_coupleseats_seatid1");

            entity.HasOne(d => d.SeatId2Navigation).WithOne(p => p.CoupleSeatSeatId2Navigation)
                .HasForeignKey<CoupleSeat>(d => d.SeatId2)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_coupleseats_seatid2");
        });

        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(e => e.FoodId).HasName("Food_pkey");

            entity.ToTable("Food");

            entity.HasIndex(e => e.FoodName, "Food_FoodName_key").IsUnique();

            entity.Property(e => e.FoodName).HasMaxLength(255);
            entity.Property(e => e.FoodPrice).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(25);

            entity.HasMany(d => d.FoodCates).WithMany(p => p.Foods)
                .UsingEntity<Dictionary<string, object>>(
                    "FoodAndCategory",
                    r => r.HasOne<FoodCategory>().WithMany()
                        .HasForeignKey("FoodCateId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_foodandcategory_foodcateid"),
                    l => l.HasOne<Food>().WithMany()
                        .HasForeignKey("FoodId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_foodandcategory_foodid"),
                    j =>
                    {
                        j.HasKey("FoodId", "FoodCateId").HasName("FoodAndCategory_pkey");
                        j.ToTable("FoodAndCategory");
                    });
        });

        modelBuilder.Entity<FoodCategory>(entity =>
        {
            entity.HasKey(e => e.FoodCateId).HasName("FoodCategory_pkey");

            entity.ToTable("FoodCategory");

            entity.HasIndex(e => e.CateName, "FoodCategory_CateName_key").IsUnique();

            entity.Property(e => e.CateName).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(25);
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("Genres_pkey");

            entity.HasIndex(e => e.Name, "Genres_Name_key").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(25);
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieId).HasName("Movies_pkey");

            entity.HasIndex(e => e.Title, "Movies_Title_key").IsUnique();

            entity.Property(e => e.Director).HasMaxLength(255);
            entity.Property(e => e.FromDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasMaxLength(25);
            entity.Property(e => e.Studio).HasMaxLength(255);
            entity.Property(e => e.ToDate).HasColumnType("timestamp without time zone");

            entity.HasMany(d => d.Genres).WithMany(p => p.Movies)
                .UsingEntity<Dictionary<string, object>>(
                    "MovieGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_moviegenres_genreid"),
                    l => l.HasOne<Movie>().WithMany()
                        .HasForeignKey("MovieId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_moviegenres_movieid"),
                    j =>
                    {
                        j.HasKey("MovieId", "GenreId").HasName("MovieGenres_pkey");
                        j.ToTable("MovieGenres");
                    });
        });

        modelBuilder.Entity<PaymentOnline>(entity =>
        {
            entity.HasKey(e => e.PaymentOnlineId).HasName("Payments_pkey");

            entity.ToTable("PaymentOnline");

            entity.Property(e => e.PaymentOnlineId).HasDefaultValueSql("nextval('\"Payments_PaymentId_seq\"'::regclass)");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BankAccId).HasMaxLength(255);
            entity.Property(e => e.BankName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PaymentMethod).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(25);

            entity.HasOne(d => d.Invoice).WithMany(p => p.PaymentOnlines)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("fk_paymentonline_invoiceid");
        });

        modelBuilder.Entity<PaymentUpFront>(entity =>
        {
            entity.HasKey(e => e.PaymentUpFrontId).HasName("PaymentUpFront_pkey");

            entity.ToTable("PaymentUpFront");

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CustomerGive).HasPrecision(18, 2);
            entity.Property(e => e.RemainChange).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(25);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(d => d.Invoice).WithMany(p => p.PaymentUpFronts)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("fk_paymentupfront_invoiceid");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("Promotions_pkey");

            entity.Property(e => e.DiscountRate).HasPrecision(5, 2);
            entity.Property(e => e.EndDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasMaxLength(25);

            entity.HasOne(d => d.Event).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("fk_promotions_eventid");
        });

        modelBuilder.Entity<PromotionEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PromotionEvent_pkey");

            entity.ToTable("PromotionEvent");

            entity.Property(e => e.EventFromDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.EventToDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasMaxLength(25);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.RoomTypeId).HasName("RoomTypes_pkey");

            entity.HasIndex(e => e.RoomTypeName, "RoomTypes_RoomTypeName_key").IsUnique();

            entity.Property(e => e.RoomTypeName).HasMaxLength(100);
            entity.Property(e => e.RoomTypePrice).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(25);
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.ScoreId).HasName("Score_pkey");

            entity.ToTable("Score");

            entity.HasIndex(e => e.Userid, "Score_userid_key").IsUnique();

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Score)
                .HasForeignKey<Score>(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_score_userid");
        });

        modelBuilder.Entity<ScoreHistory>(entity =>
        {
            entity.HasKey(e => e.ScoreHistoryId).HasName("ScoreHistory_pkey");

            entity.ToTable("ScoreHistory");

            entity.Property(e => e.ChangeDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Invoice).WithMany(p => p.ScoreHistories)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("fk_scorehistory_invoiceid");

            entity.HasOne(d => d.Score).WithMany(p => p.ScoreHistories)
                .HasForeignKey(d => d.ScoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_scorehistory_scoreid");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId).HasName("Seats_pkey");

            entity.HasIndex(e => new { e.RoomId, e.RowLabel, e.ColumnNumber }, "Seats_RoomId_RowLabel_ColumnNumber_key").IsUnique();

            entity.Property(e => e.RowLabel).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(25);

            entity.HasOne(d => d.Room).WithMany(p => p.Seats)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_seats_roomid");

            entity.HasOne(d => d.SeatType).WithMany(p => p.Seats)
                .HasForeignKey(d => d.SeatTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_seats_seattypeid");
        });

        modelBuilder.Entity<SeatDataForShowtime>(entity =>
        {
            entity.HasKey(e => e.SeatDataId).HasName("SeatDataForShowtime_pkey");

            entity.ToTable("SeatDataForShowtime");

            entity.Property(e => e.PairedWithSeatLocation).HasMaxLength(10);
            entity.Property(e => e.RowLabel).HasMaxLength(10);
            entity.Property(e => e.SeatTypeName).HasMaxLength(50);
            entity.Property(e => e.SeatTypePrice).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(25);

            entity.HasOne(d => d.ShowtimeInstance).WithMany(p => p.SeatDataForShowtimes)
                .HasForeignKey(d => d.ShowtimeInstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_seatdataforshowtime_showtimeinstanceid");
        });

        modelBuilder.Entity<SeatType>(entity =>
        {
            entity.HasKey(e => e.SeatTypeId).HasName("SeatTypes_pkey");

            entity.HasIndex(e => e.SeatTypeName, "SeatTypes_SeatTypeName_key").IsUnique();

            entity.Property(e => e.SeatTypeName).HasMaxLength(50);
            entity.Property(e => e.SeatTypePrice).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(25);
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasKey(e => e.ShowtimeId).HasName("Showtimes_pkey");

            entity.Property(e => e.EndTime).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.StartTime).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.Status).HasMaxLength(25);

            entity.HasOne(d => d.Movie).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("fk_showtimes_movieid");
        });

        modelBuilder.Entity<ShowtimeRoomInstance>(entity =>
        {
            entity.HasKey(e => e.ShowtimeInstanceId).HasName("ShowtimeRoomInstance_pkey");

            entity.ToTable("ShowtimeRoomInstance");

            entity.Property(e => e.ActualEndTime).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.ActualStartTime).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.AddedAt).HasColumnType("timestamp(0) without time zone");
            entity.Property(e => e.MoviePrice).HasPrecision(18, 2);
            entity.Property(e => e.RoomName).HasMaxLength(100);
            entity.Property(e => e.RoomTypeName).HasMaxLength(100);
            entity.Property(e => e.RoomTypePrice).HasPrecision(18, 2);

            entity.HasOne(d => d.OriginalRoom).WithMany(p => p.ShowtimeRoomInstances)
                .HasForeignKey(d => d.OriginalRoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_showtimeroominstance_originalroomid");

            entity.HasOne(d => d.Showtime).WithMany(p => p.ShowtimeRoomInstances)
                .HasForeignKey(d => d.ShowtimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_showtimeroominstance_showtimeid");
        });

        modelBuilder.Entity<TicketDetail>(entity =>
        {
            entity.HasKey(e => new { e.ShowtimeInstanceId, e.SeatDataId }).HasName("TicketDetails_pkey");

            entity.Property(e => e.Status).HasMaxLength(25);
            entity.Property(e => e.TicketPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Invoice).WithMany(p => p.TicketDetails)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("fk_ticketdetails_invoiceid");

            entity.HasOne(d => d.SeatData).WithMany(p => p.TicketDetails)
                .HasForeignKey(d => d.SeatDataId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticketdetails_seatdataid");

            entity.HasOne(d => d.ShowtimeInstance).WithMany(p => p.TicketDetails)
                .HasForeignKey(d => d.ShowtimeInstanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticketdetails_showtimeinstanceid");
        });

        modelBuilder.Entity<TicketInvoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("TicketInvoices_pkey");

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PaymentType).HasMaxLength(50);
            entity.Property(e => e.ScoreDiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(25);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");

            entity.HasOne(d => d.Promotion).WithMany(p => p.TicketInvoices)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("fk_ticketinvoices_promotionid");

            entity.HasOne(d => d.User).WithMany(p => p.TicketInvoices)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("fk_ticketinvoices_userid");
        });

        modelBuilder.Entity<TicketInvoiceFoodItem>(entity =>
        {
            entity.HasKey(e => new { e.InvoiceId, e.FoodId }).HasName("TicketInvoiceFoodItems_pkey");

            entity.Property(e => e.TotalFoodPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Food).WithMany(p => p.TicketInvoiceFoodItems)
                .HasForeignKey(d => d.FoodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticketinvoicefooditems_foodid");

            entity.HasOne(d => d.Invoice).WithMany(p => p.TicketInvoiceFoodItems)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticketinvoicefooditems_invoiceid");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Userid)
                .HasMaxLength(50)
                .HasColumnName("userid");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Birthdate).HasColumnName("birthdate");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(100)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Identitynumber)
                .HasMaxLength(50)
                .HasColumnName("identitynumber");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.Joindate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("joindate");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("fk_users_roleid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
