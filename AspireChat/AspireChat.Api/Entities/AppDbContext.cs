using Microsoft.EntityFrameworkCore;

namespace AspireChat.Api.Entities;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Chat> Chats { get; set; }
}