using CapstoneProject.Application.DTO;
using CapstoneProject.Application.Interface.IRepository;
using CapstoneProject.Application.Interface.IService;

namespace CapstoneProject.Infrastructure.Services
{
    public class DateTimeService : IDateTimeService
    {
        private readonly ISystemRepository _systemRepo;

        public DateTimeService(ISystemRepository systemRepo)
        {
            _systemRepo = systemRepo;
        }

        public async Task<ServiceResponse<DeadlineResponse>> GetWeeklyDeadlineAsync()
        {
            var response = new ServiceResponse<DeadlineResponse>();
            try
            {
                DateTime now = _systemRepo.GetSystemDateTime();

                int daysUntilSunday = (7 - (int)now.DayOfWeek) % 7;
                DateTime sundayDeadline = now.AddDays(daysUntilSunday).Date
                                             .AddHours(23).AddMinutes(59).AddSeconds(59);

                TimeSpan remaining = sundayDeadline - now;

                var data = new DeadlineResponse
                {
                    CurrentDate = now.ToString("dd/MM/yyyy HH:mm:ss"),
                    WeeklyDeadline = sundayDeadline.ToString("dd/MM/yyyy HH:mm:ss"),
                    DaysRemaining = remaining.Days,
                    MessageStatus = remaining.TotalSeconds > 0
                        ? $"{remaining.Days} days and {remaining.Hours} hours left until submission deadline."
                        : "The weekly deadline has passed."
                };

                response.Data = data;
                response.Message = "Weekly deadline status retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving deadline: {ex.Message}";
            }

            return response;
        }

        public bool IsWithinDeadline(DateTime currentDate)
        {
            int daysUntilSunday = (7 - (int)currentDate.DayOfWeek) % 7;
            DateTime deadline = currentDate.AddDays(daysUntilSunday).Date
                                           .AddHours(23).AddMinutes(59).AddSeconds(59);

            return currentDate <= deadline;
        }
    }
}