using CapstoneProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Group> Groups { get; }
        // Bạn có thể khai báo thêm các DbSet khác nếu cần dùng ở Service sau này

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}