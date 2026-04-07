using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CapstoneProject.Application.DTO
{
    public class TopicSubmitRequest
    {
        public int GroupId { get; set; }

        [Required(ErrorMessage = "Topic name is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
    }
}
