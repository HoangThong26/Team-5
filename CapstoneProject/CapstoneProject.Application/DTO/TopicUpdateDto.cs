using System.ComponentModel.DataAnnotations;

namespace CapstoneProject.Application.DTO
{
    public class TopicUpdateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}