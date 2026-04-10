using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapstoneProject.Application.DTO
{
    public class AdminDashboardStatsDto
    {
        // Core KPI Cards
        public int TotalStudents { get; set; }
        public int TotalMentors { get; set; }
        public int TotalGroups { get; set; }
        public TopicApprovalStatsDto TopicApprovalStats { get; set; }

        // Actionable Insights & Alerts
        public int OrphanGroupsCount { get; set; }
        public int PendingTopicsCount { get; set; }
        public ReportHealthStatsDto ReportHealthStats { get; set; }

        // Cross-sectional Charts (simplified for now)
        public PassFailStatsDto PassFailStats { get; set; }
    }

    public class TopicApprovalStatsDto
    {
        public int ApprovedTopics { get; set; }
        public int TotalTopics { get; set; }
        public double ApprovalRate => TotalTopics > 0 ? (double)ApprovedTopics / TotalTopics * 100 : 0;
        public string DisplayText => $"{ApprovedTopics}/{TotalTopics} ({ApprovalRate:F1}%)";
    }

    public class ReportHealthStatsDto
    {
        public int GroupsNotSubmittedThisWeek { get; set; }
        public int MentorsNotGradedThisWeek { get; set; }
        public List<string> GroupsNotSubmitted { get; set; }
        public List<string> MentorsNotGraded { get; set; }
    }

    public class PassFailStatsDto
    {
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Total => Passed + Failed;
        public double PassRate => Total > 0 ? (double)Passed / Total * 100 : 0;
    }
}