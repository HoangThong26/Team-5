namespace CapstoneProject.Application.Interface.IService
{
    public interface ITopicAiService
    {
        Task<string> GenerateTopicsForSubmissionAsync(string message);
    }
}