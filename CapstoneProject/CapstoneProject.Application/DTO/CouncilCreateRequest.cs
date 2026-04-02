namespace CapstoneProject.Application.DTO
{
    public class CouncilCreateRequest
    {
        public string Name { get; set; }
        public List<int> MemberIds { get; set; }
    }
}
