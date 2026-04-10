using CapstoneProject.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IGradeService
    {
        Task<decimal> CalculateAndSaveFinalGrade(int groupId);
        Task PublishGrade(int groupId);
        Task<FinalGrade?> GetGradeForStudent(int groupId);
        Task<List<FinalGrade>> GetAllFinalGrades();
    }
}
