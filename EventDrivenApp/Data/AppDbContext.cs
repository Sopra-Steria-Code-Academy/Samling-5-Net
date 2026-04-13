using EventDrivenApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ConsumedMessage> ConsumedMessages => Set<ConsumedMessage>();
}
