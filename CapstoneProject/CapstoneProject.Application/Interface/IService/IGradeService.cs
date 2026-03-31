using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IGradeService
    {
        Task<decimal> CalculateAndSaveFinalGrade(int groupId);
    }
}
