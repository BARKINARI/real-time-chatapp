using ChatAppServer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAppServer.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Chat> Chats => Set<Chat>();
        public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
}
