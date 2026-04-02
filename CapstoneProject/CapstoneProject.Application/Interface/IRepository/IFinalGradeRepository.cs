using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IFinalGradeRepository
    {
        Task<FinalGrade?> GetByGroupIdAsync(int groupId);
        Task<List<FinalGrade>> GetAllWithGroupsAsync();
        Task AddAsync(FinalGrade finalGrade);
        void Update(FinalGrade finalGrade);
        Task SaveChangesAsync();
    }
}
