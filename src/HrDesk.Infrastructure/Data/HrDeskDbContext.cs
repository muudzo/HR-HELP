using HrDesk.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HrDesk.Infrastructure.Data;

public class HrDeskDbContext : DbContext
{
    public HrDeskDbContext(DbContextOptions<HrDeskDbContext> options) : base(options)
    {
    }

    public DbSet<ChatHistory> ChatHistories { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Escalation> Escalations { get; set; }
    public DbSet<EmployeeCache> EmployeeCache { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ChatHistory config
        modelBuilder.Entity<ChatHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserMessage).IsRequired();
        });

        // Ticket config
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
                .HasConversion<string>();
            entity.Property(e => e.Severity)
                .HasConversion<string>();
            
            // Ticket -> ChatHistories (One-to-Many)
            entity.HasMany(t => t.ChatHistories)
                .WithOne(c => c.Ticket)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Ticket -> Escalations (One-to-Many)
            entity.HasMany(t => t.Escalations)
                .WithOne(e => e.Ticket)
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Escalation config
        modelBuilder.Entity<Escalation>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // EmployeeCache config
        modelBuilder.Entity<EmployeeCache>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EmployeeId).IsUnique();
        });
    }
}
