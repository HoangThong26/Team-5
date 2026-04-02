using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Database.AppDbContext;
using Microsoft.EntityFrameworkCore;

namespace CapstoneProject.Infrastructure.Repostitory
{
    public class FinalGradeRepository : IFinalGradeRepository
    {
        private readonly ApplicationDbContext _context;
        public FinalGradeRepository(ApplicationDbContext context) => _context = context;

        public async Task<FinalGrade?> GetByGroupIdAsync(int groupId)
            => await _context.FinalGrades.FirstOrDefaultAsync(g => g.GroupId == groupId);

        public async Task<List<FinalGrade>> GetAllWithGroupsAsync()
            => await _context.FinalGrades.Include(g => g.Group).ToListAsync();

        public async Task AddAsync(FinalGrade finalGrade) => await _context.FinalGrades.AddAsync(finalGrade);

        public void Update(FinalGrade finalGrade)
        {
            _context.FinalGrades.Update(finalGrade);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
