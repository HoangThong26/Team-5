using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.Interface.IRepository
{
    public interface IGroupMemberRepository
    {
        Task<List<string>> GetMemberEmailsByGroupIdAsync(int groupId);
    }
}
