namespace CapstoneProject.Application.DTO
{
    public class DefenseCommitteeDto
    {
        public int CouncilId { get; set; }
        public string CouncilName { get; set; } = string.Empty;
        public List<DefenseCommitteeMemberDto> Members { get; set; } = new();
    }

    public class DefenseCommitteeMemberDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
