using Microsoft.EntityFrameworkCore;
using TaskReminderGrpc.Models;

namespace TaskReminderGrpc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){

    }

    public DbSet<TaskReminder> taskReminderDbSet => Set<TaskReminder>();
}