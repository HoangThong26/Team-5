using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class TopicApprovalRequest
    {
        public int VersionId { get; set; }
        public int TopicId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ReviewComment { get; set; }
    }
}
