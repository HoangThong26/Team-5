using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Infrastructure.Database.AppDbContext;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class SystemRepository : ISystemRepository
    {
        private readonly ApplicationDbContext _context;
        public SystemRepository(ApplicationDbContext context) => _context = context;

        public DateTime GetSystemDateTime() => DateTime.Now;
    }
}
