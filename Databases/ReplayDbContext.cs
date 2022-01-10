using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using vault.Entities;

namespace vault.Databases
{
    public class ReplayDbContext : DbContext
    {
        public ReplayDbContext(DbContextOptions<ReplayDbContext> options) : base(options) {}
        public DbSet<Replay> Replays { get; set; }
        public DbSet<MostPlayedMapsAggregateRecord> AggregateDbSet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Replay>().ToTable("replays").HasKey(r => r.Sha256);
            modelBuilder.Entity<MostPlayedMapsAggregateRecord>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }
}