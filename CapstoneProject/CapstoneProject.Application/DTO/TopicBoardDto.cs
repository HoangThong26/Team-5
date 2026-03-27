using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class TopicBoardDto
    {
        public int VersionId { get; set; }
        public string TeamName { get; set; }
        public string TopicName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } 
        public DateTime? SubmittedAt { get; set; }
    }
}
