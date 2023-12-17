using Eventhat.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eventhat.Database;

public class ViewDataContext : DbContext
{
    public ViewDataContext(DbContextOptions<ViewDataContext> options) : base(options)
    {
    }

    public DbSet<Page> Pages { get; set; }
    public DbSet<UserCredentials> UserCredentials { get; set; }
    public DbSet<VideoOperation> VideoOperations { get; set; }
    public DbSet<CreatorVideo> CreatorVideos { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<AdminStream> AdminStreams { get; set; }
}