using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class TopicApprovalRequest
    {
        public int TopicId { get; set; }
        public bool IsApproved { get; set; }
        public string? Comment { get; set; }
    }
}
