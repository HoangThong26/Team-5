using Microsoft.AspNetCore.SignalR;

namespace CapstoneProject.API.Hubs
{
    public class TopicHub : Hub
    {
        public async Task NotifyNewTopic(int mentorId)
        {
            await Clients.User(mentorId.ToString()).SendAsync("ReceiveNewTopic");
        }
    }
}
