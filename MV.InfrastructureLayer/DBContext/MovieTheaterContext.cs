using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MV.DomainLayer.Entities;

namespace MV.InfrastructureLayer.DBContext;

public partial class MovieTheaterContext : DbContext
{
    public MovieTheaterContext()
    {
    }

    public MovieTheaterContext(DbContextOptions<MovieTheaterContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cinemaroom> Cinemarooms { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Scorehistory> Scorehistories { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Showtime> Showtimes { get; set; }

    public virtual DbSet<Showtimeseat> Showtimeseats { get; set; }

    public virtual DbSet<Ticketdetail> Ticketdetails { get; set; }

    public virtual DbSet<Ticketinvoice> Ticketinvoices { get; set; }

    public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=MovieTheater;Username=postgres;Password=1;Trust Server Certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cinemaroom>(entity =>
        {
            entity.HasKey(e => e.Roomid).HasName("cinemarooms_pkey");

            entity.ToTable("cinemarooms");

            entity.Property(e => e.Roomid).HasColumnName("roomid");
            entity.Property(e => e.Columns).HasColumnName("columns");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Rows).HasColumnName("rows");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Genreid).HasName("genres_pkey");

            entity.ToTable("genres");

            entity.Property(e => e.Genreid).HasColumnName("genreid");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Movieid).HasName("movies_pkey");

            entity.ToTable("movies");

            entity.Property(e => e.Movieid).HasColumnName("movieid");
            entity.Property(e => e.Actors).HasColumnName("actors");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Director)
                .HasMaxLength(100)
                .HasColumnName("director");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Fromdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fromdate");
            entity.Property(e => e.Poster)
                .HasMaxLength(255)
                .HasColumnName("poster");
            entity.Property(e => e.Publishdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("publishdate");
            entity.Property(e => e.Studio)
                .HasMaxLength(100)
                .HasColumnName("studio");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Todate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("todate");
            entity.Property(e => e.Trailerurl)
                .HasMaxLength(255)
                .HasColumnName("trailerurl");
            entity.Property(e => e.Version).HasColumnName("version");

            entity.HasMany(d => d.Genres).WithMany(p => p.Movies)
                .UsingEntity<Dictionary<string, object>>(
                    "Moviegenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("Genreid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("moviegenres_genreid_fkey"),
                    l => l.HasOne<Movie>().WithMany()
                        .HasForeignKey("Movieid")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("moviegenres_movieid_fkey"),
                    j =>
                    {
                        j.HasKey("Movieid", "Genreid").HasName("moviegenres_pkey");
                        j.ToTable("moviegenres");
                        j.IndexerProperty<int>("Movieid").HasColumnName("movieid");
                        j.IndexerProperty<int>("Genreid").HasColumnName("genreid");
                    });
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Orderid).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.Orderid)
                .HasMaxLength(50)
                .HasColumnName("orderid");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Totalamount)
                .HasPrecision(18, 2)
                .HasColumnName("totalamount");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Paymentid).HasName("payments_pkey");

            entity.ToTable("payments");

            entity.Property(e => e.Paymentid)
                .HasMaxLength(50)
                .HasColumnName("paymentid");
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Invoiceid)
                .HasMaxLength(50)
                .HasColumnName("invoiceid");
            entity.Property(e => e.Paymentmethod)
                .HasMaxLength(50)
                .HasColumnName("paymentmethod");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.Invoiceid)
                .HasConstraintName("payments_invoiceid_fkey");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Promotionid).HasName("promotions_pkey");

            entity.ToTable("promotions");

            entity.Property(e => e.Promotionid)
                .HasMaxLength(50)
                .HasColumnName("promotionid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Discountrate)
                .HasPrecision(5, 2)
                .HasColumnName("discountrate");
            entity.Property(e => e.Enddate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("enddate");
            entity.Property(e => e.Image)
                .HasMaxLength(255)
                .HasColumnName("image");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Startdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("startdate");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("Roles_pkey");

            entity.Property(e => e.RoleId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Scorehistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("scorehistories_pkey");

            entity.ToTable("scorehistories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Changedpoints)
                .HasPrecision(18, 2)
                .HasColumnName("changedpoints");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Seatid).HasName("seats_pkey");

            entity.ToTable("seats");

            entity.Property(e => e.Seatid).HasColumnName("seatid");
            entity.Property(e => e.Columnnumber).HasColumnName("columnnumber");
            entity.Property(e => e.Roomid).HasColumnName("roomid");
            entity.Property(e => e.Rowlabel)
                .HasMaxLength(10)
                .HasColumnName("rowlabel");
            entity.Property(e => e.Seattype).HasColumnName("seattype");

            entity.HasOne(d => d.Room).WithMany(p => p.Seats)
                .HasForeignKey(d => d.Roomid)
                .HasConstraintName("seats_roomid_fkey");
        });

        modelBuilder.Entity<Showtime>(entity =>
        {
            entity.HasKey(e => e.Showtimeid).HasName("showtimes_pkey");

            entity.ToTable("showtimes");

            entity.Property(e => e.Showtimeid).HasColumnName("showtimeid");
            entity.Property(e => e.Movieid).HasColumnName("movieid");
            entity.Property(e => e.Roomid).HasColumnName("roomid");
            entity.Property(e => e.Starttime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("starttime");

            entity.HasOne(d => d.Movie).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.Movieid)
                .HasConstraintName("showtimes_movieid_fkey");

            entity.HasOne(d => d.Room).WithMany(p => p.Showtimes)
                .HasForeignKey(d => d.Roomid)
                .HasConstraintName("showtimes_roomid_fkey");
        });

        modelBuilder.Entity<Showtimeseat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("showtimeseats_pkey");

            entity.ToTable("showtimeseats");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Seatid).HasColumnName("seatid");
            entity.Property(e => e.Showtimeid).HasColumnName("showtimeid");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Seat).WithMany(p => p.Showtimeseats)
                .HasForeignKey(d => d.Seatid)
                .HasConstraintName("showtimeseats_seatid_fkey");

            entity.HasOne(d => d.Showtime).WithMany(p => p.Showtimeseats)
                .HasForeignKey(d => d.Showtimeid)
                .HasConstraintName("showtimeseats_showtimeid_fkey");
        });

        modelBuilder.Entity<Ticketdetail>(entity =>
        {
            entity.HasKey(e => new { e.Invoiceid, e.Seatid }).HasName("ticketdetails_pkey");

            entity.ToTable("ticketdetails");

            entity.Property(e => e.Invoiceid)
                .HasMaxLength(50)
                .HasColumnName("invoiceid");
            entity.Property(e => e.Seatid).HasColumnName("seatid");
            entity.Property(e => e.Ticketprice)
                .HasPrecision(18, 2)
                .HasColumnName("ticketprice");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Ticketdetails)
                .HasForeignKey(d => d.Invoiceid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticketdetails_invoiceid_fkey");

            entity.HasOne(d => d.Seat).WithMany(p => p.Ticketdetails)
                .HasForeignKey(d => d.Seatid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticketdetails_seatid_fkey");
        });

        modelBuilder.Entity<Ticketinvoice>(entity =>
        {
            entity.HasKey(e => e.Invoiceid).HasName("ticketinvoices_pkey");

            entity.ToTable("ticketinvoices");

            entity.Property(e => e.Invoiceid)
                .HasMaxLength(50)
                .HasColumnName("invoiceid");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Exchangedtickets).HasColumnName("exchangedtickets");
            entity.Property(e => e.Promotionid)
                .HasMaxLength(50)
                .HasColumnName("promotionid");
            entity.Property(e => e.Showtimeid).HasColumnName("showtimeid");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Totalprice)
                .HasPrecision(18, 2)
                .HasColumnName("totalprice");
            entity.Property(e => e.Usedpoints)
                .HasPrecision(18, 2)
                .HasColumnName("usedpoints");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Ticketinvoices)
                .HasForeignKey(d => d.Promotionid)
                .HasConstraintName("ticketinvoices_promotionid_fkey");

            entity.HasOne(d => d.Showtime).WithMany(p => p.Ticketinvoices)
                .HasForeignKey(d => d.Showtimeid)
                .HasConstraintName("ticketinvoices_showtimeid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.HasIndex(e => e.IdentityNumber, "Users_IdentityNumber_key").IsUnique();

            entity.HasIndex(e => e.Phone, "Users_Phone_key").IsUnique();

            entity.HasIndex(e => e.Username, "Users_Username_key").IsUnique();

            entity.Property(e => e.UserId).HasMaxLength(50);
            entity.Property(e => e.AccumulatedPoints).HasPrecision(18, 2);
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IdentityNumber).HasMaxLength(50);
            entity.Property(e => e.JoinDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RoleId).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
