using Microsoft.EntityFrameworkCore;

namespace Eventhat.Database;

public class MessageContext : DbContext
{
    public MessageContext(DbContextOptions<MessageContext> options) : base(options)
    {
    }

    public DbSet<MessageEntity> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MessageEntity>()
            .HasKey(e => new { e.GlobalPosition, e.Position, e.StreamName })
            .IsClustered(false);

        modelBuilder.Entity<MessageEntity>()
            .HasIndex(e => new { e.Position, e.StreamName })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        foreach (var entry in ChangeTracker.Entries<MessageEntity>())
            if (entry.State == EntityState.Added && entry.Entity.Position == default)
            {
                var existingMaxPosition = Messages
                    .Where(m => m.StreamName == entry.Entity.StreamName)
                    .Max(m => (int?)m.Position);

                // Note: There is a potential race condition here when multiple parallel executions
                // reach this code for the same stream simultaneously. The first execution correctly
                // increments the position, but subsequent executions may update the position to
                // a deprecated value. The database index will detect this, and Entity Framework
                // will throw a unique constraint violation exception.
                //
                // To mitigate this race condition, consider moving the incrementation logic into
                // a database function with proper locking mechanisms to ensure atomicity and
                // prevent concurrent updates to the same stream's position.
                if (existingMaxPosition.HasValue)
                    entry.Entity.Position = existingMaxPosition.Value + 1;
                else
                    entry.Entity.Position = 1;
            }

        return base.SaveChangesAsync(cancellationToken);
    }
}