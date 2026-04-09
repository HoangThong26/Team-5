using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IRepository
{
   public interface IMentorAssignmentRepository
    {
        Task<string?> GetMentorEmailByGroupIdAsync(int groupId);
    }
}
