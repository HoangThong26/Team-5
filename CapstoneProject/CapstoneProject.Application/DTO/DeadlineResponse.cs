namespace CapstoneProject.Application.DTO
{
    public class DeadlineResponse
    {
        public string CurrentDate { get; set; }        // Ngày hiện tại (Format: dd/MM/yyyy)
        public string WeeklyDeadline { get; set; }     // Hạn cuối tuần (Format: dd/MM/yyyy)
        public int DaysRemaining { get; set; }         // Số ngày còn lại (VD: 3)
        public string MessageStatus { get; set; }      // Thông báo trạng thái (VD: "3 days left until deadline")
    }
}
