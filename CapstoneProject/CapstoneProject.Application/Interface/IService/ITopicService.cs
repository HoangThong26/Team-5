using CapstoneProject.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IService
{
    public interface ITopicService
    {
        Task SubmitTopicAsync(int userId, TopicSubmitRequest request);
        Task ApproveTopicAsync(int reviewerId, TopicApprovalRequest request);
    }
}
