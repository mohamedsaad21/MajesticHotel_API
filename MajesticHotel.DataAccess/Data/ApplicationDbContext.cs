using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MajesticHotel.Models;
namespace MajesticHotel_HotelAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<City> Cities { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<RoomClass> RoomClasses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<HotelAmenities> HotelAmenities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<HotelAmenities>().HasKey(x => new { x.HotelId, x.AmenityId });
            
            modelBuilder.Entity<City>().HasData(
                new City { Id = 1, Name = "Las Vegas", PostalCode = "88901" },
                new City { Id = 2, Name = "New York", PostalCode = "07008" },
                new City { Id = 3, Name = "Chicago", PostalCode = "60007" },
                new City { Id = 4, Name = "Los Angeles", PostalCode = "90001" }
                );

            modelBuilder.Entity<Hotel>().HasData(
                new Hotel
                {
                    Id = 1,
                    Name = "Grand Hotel",
                    Description = "Lorem ipsum dolor sit amet consectetur adipisicing elit. Enim aspernatur sint, cumque dolores impedit vel!",
                    Phone = "01059855423",
                    Email = "owner@grand.com",
                    CityId = 1
                },
                new Hotel
                {
                    Id = 2,
                    Name = "Residence Inn",
                    Description = "Lorem ipsum dolor sit, amet consectetur adipisicing elit. Nisi officia consectetur atque, quod delectus cum.",
                    Phone = "01049512423",
                    Email = "owner@residence.com",
                    CityId = 2
                },
                new Hotel
                {
                    Id = 3,
                    Name = "Comfort Inn",
                    Description = "Lorem ipsum dolor sit amet consectetur adipisicing elit. Voluptatibus odit, ullam neque corporis officia amet!",
                    Phone = "01099859413",
                    Email = "owner@comfort.com",
                    CityId = 3
                },
                new Hotel
                {
                    Id = 4,
                    Name = "Princess Royale",
                    Description = "Lorem ipsum, dolor sit amet consectetur adipisicing elit. Possimus nulla repudiandae asperiores cumque!",
                    Phone = "01059175421",
                    Email = "owner@princess.com",
                    CityId = 4
                });

            modelBuilder.Entity<RoomClass>().HasData(
                new RoomClass
                {
                    Id = 1,
                    Name = "Standard Room",
                    Description = "A cozy room with basic amenities",
                    AdultsCapacity = 6,
                    ChildrenCapacity = 2,
                    PricePerNight = 100
                },
                new RoomClass
                {
                    Id = 2,
                    Name = "Deluxe Room",
                    Description = "Spacious room with a view and premium features",
                    AdultsCapacity = 7,
                    ChildrenCapacity = 3,
                    PricePerNight = 150
                },
                new RoomClass
                {
                    Id = 3,
                    Name = "Family Suite",
                    Description = "Perfect for families, includes a separate area",
                    AdultsCapacity = 12,
                    ChildrenCapacity = 5,
                    PricePerNight = 250
                },
                new RoomClass
                {
                    Id = 4,
                    Name = "Executive Suite",
                    Description = "Luxurious suite with a workspace and lounge",
                    AdultsCapacity = 5,
                    ChildrenCapacity = 2,
                    PricePerNight = 300
                },
                new RoomClass
                {
                    Id = 5,
                    Name = "Single Room",
                    Description = "Compact room ideal for solo travelers",
                    AdultsCapacity = 2,
                    ChildrenCapacity = 0,
                    PricePerNight = 80
                },
                new RoomClass
                {
                    Id = 6,
                    Name = "Twin Room",
                    Description = "Room with two single beds, great for friends",
                    AdultsCapacity = 2,
                    ChildrenCapacity = 0,
                    PricePerNight = 120
                }
                );
            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, RoomClassId = 5, HotelId = 3, IsAvailable = true },
                new Room { Id = 2, RoomClassId = 4, HotelId = 1, IsAvailable = true },
                new Room { Id = 3, RoomClassId = 2, HotelId = 4, IsAvailable = true },
                new Room { Id = 4, RoomClassId = 6, HotelId = 2, IsAvailable = true }
                );

            modelBuilder.Entity<Amenity>().HasData(

                new Amenity { Id = 1, Name = "Free Wi-Fi", Description = "High-speed internet access available throughout the hotel" },
                new Amenity { Id = 2, Name = "Swimming Pool", Description = "Outdoor pool with a temperature-controlled environment" },
                new Amenity { Id = 3, Name = "Fitness Center", Description = "Fully equipped gym with modern exercise machines and weights" },
                new Amenity { Id = 4, Name = "24/7 Room Service", Description = "Round-the-clock dining service with a variety of cuisines" }
                );
        }
    }
}
