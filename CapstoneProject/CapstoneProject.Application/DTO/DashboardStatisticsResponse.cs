using System.Collections.Generic;

namespace CapstoneProject.Application.DTO
{
    public class DashboardStatisticsResponse
    {
        public KpiCardsResponse KpiCards { get; set; } = new();
        public OverviewStatsResponse Overview { get; set; } = new();
    }

    public class KpiCardsResponse
    {
        public int TotalStudents { get; set; }
        public int TotalMentors { get; set; }
        public int TotalGroups { get; set; }
        public int TotalTopics { get; set; }
    }

    public class OverviewStatsResponse
    {
        public int StudentsWithGroup { get; set; }
        public int StudentsWithoutGroup { get; set; }
        public List<StatusCountResponse> TopicStatus { get; set; } = new();
        public List<StatusCountResponse> GroupStatus { get; set; } = new();
    }

    public class StatusCountResponse
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}