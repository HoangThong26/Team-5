using CapstoneProject.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface IWeeklyEvaluationService
    {
        Task EvaluateAsync(int mentorId, EvaluationRequest request);
        Task<EvaluationResponseDTO?> GetEvaluationByReportIdAsync(int reportId);
    }
}
