using Microsoft.EntityFrameworkCore;

namespace QRCodeGeneratorAndEmployeeManager.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Employee> EmployeeQRTable { get; set; }
    }
}
