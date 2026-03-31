using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IWeeklyEvaluationRepository
    {
        Task AddAsync(WeeklyEvaluation evaluation);
        Task<WeeklyEvaluation?> GetByReportIdAsync(int reportId);
        Task SaveChangesAsync();
    }
}
