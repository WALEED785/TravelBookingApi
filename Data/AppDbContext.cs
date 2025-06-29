using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TravelBookingApi.Models.Entities;

namespace TravelBookingApi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {

        // Database Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Hotel>()
                .HasOne(h => h.Destination)
                .WithMany(d => d.Hotels)
                .HasForeignKey(h => h.DestinationId);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DepartureDestination)
                .WithMany(d => d.DepartureFlights)
                .HasForeignKey(f => f.DepartureDestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.ArrivalDestination)
                .WithMany(d => d.ArrivalFlights)
                .HasForeignKey(f => f.ArrivalDestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<UserPreference>()
                .HasOne(up => up.User)
                .WithOne(u => u.UserPreference)
                .HasForeignKey<UserPreference>(up => up.UserId);
        }
    }
}