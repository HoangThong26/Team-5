using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class TopicSubmitRequest
    {
        public int GroupId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
