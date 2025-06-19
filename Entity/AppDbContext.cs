using Microsoft.EntityFrameworkCore;
using RealTime_APIDev.Model;

namespace RealTime_APIDev.Entity
{
    public class AppDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }

        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    }
}
