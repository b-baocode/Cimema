using System;
using System.Collections.Generic;
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

    public virtual DbSet<CoupleSeat> CoupleSeats { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SeatType> SeatTypes { get; set; }

    public virtual DbSet<Showtime> Showtimes { get; set; }

    public virtual DbSet<TicketDetail> TicketDetails { get; set; }

    public virtual DbSet<TicketInvoice> TicketInvoices { get; set; }

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

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(25);
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

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("Genres_pkey");

            entity.HasIndex(e => e.Name, "Genres_Name_key").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.MovieId).HasName("Movies_pkey");

            entity.Property(e => e.MovieId)
                .UseIdentityColumn()
                .HasColumnName("MovieId");

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

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("Payments_pkey");

            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BankAccId).HasMaxLength(255);
            entity.Property(e => e.BankName).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.PaymentMethod).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(25);

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("fk_payments_invoiceid");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("Promotions_pkey");

            entity.Property(e => e.DiscountRate).HasPrecision(5, 2);
            entity.Property(e => e.EndDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");
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

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId).HasName("Seats_pkey");

            entity.HasIndex(e => new { e.RoomId, e.RowLabel, e.ColumnNumber }, "Seats_RoomId_RowLabel_ColumnNumber_key").IsUnique();

            entity.Property(e => e.RowLabel).HasMaxLength(10);

            entity.HasOne(d => d.Room).WithMany(p => p.Seats)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("fk_seats_roomid");

            entity.HasOne(d => d.SeatType).WithMany(p => p.Seats)
                .HasForeignKey(d => d.SeatTypeId)
                .HasConstraintName("fk_seats_seattypeid");
        });

        modelBuilder.Entity<SeatType>(entity =>
        {
            entity.HasKey(e => e.SeatTypeId).HasName("SeatTypes_pkey");

            entity.HasIndex(e => e.SeatTypeName, "SeatTypes_SeatTypeName_key").IsUnique();

            entity.Property(e => e.SeatTypeName).HasMaxLength(50);
            entity.Property(e => e.SeatTypePrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasKey(e => e.ShowtimeId).HasName("Showtimes_pkey");

            entity.Property(e => e.EndTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.MoviePrice).HasPrecision(18, 2);
            entity.Property(e => e.StartTime).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Movie).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.MovieId)
                .HasConstraintName("fk_showtimes_movieid");

            entity.HasOne(d => d.Room).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("fk_showtimes_roomid");
        });

        modelBuilder.Entity<TicketDetail>(entity =>
        {
            entity.HasKey(e => new { e.ShowtimeId, e.SeatId }).HasName("TicketDetails_pkey");

            entity.Property(e => e.TicketPrice).HasPrecision(18, 2);

            entity.HasOne(d => d.Invoice).WithMany(p => p.TicketDetails)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("fk_ticketdetails_invoiceid");

            entity.HasOne(d => d.Seat).WithMany(p => p.TicketDetails)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticketdetails_seatid");

            entity.HasOne(d => d.Showtime).WithMany(p => p.TicketDetails)
                .HasForeignKey(d => d.ShowtimeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ticketdetails_showtimeid");
        });

        modelBuilder.Entity<TicketInvoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("TicketInvoices_pkey");

            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
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
