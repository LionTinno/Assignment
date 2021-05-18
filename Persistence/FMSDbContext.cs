using Microsoft.EntityFrameworkCore;
using FMS.Models;

namespace FMS.Persistence
{
    public class FMSDbContext: DbContext
    {
        public FMSDbContext(DbContextOptions<FMSDbContext> options)
            :base(options)
        {
            
        }

        public DbSet<POTransaction> POTransaction {get;set;}
    }
}