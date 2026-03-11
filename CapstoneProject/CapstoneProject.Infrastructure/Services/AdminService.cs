    using CapstoneProject.Application.DTO;
    using CapstoneProject.Application.Interface.IRepository;
    using CapstoneProject.Application.Interface.IService;
    using CapstoneProject.Domain.Entities;
using CapstoneProject.Infrastructure.Repostitory;
using ClosedXML.Excel;
using ExcelDataReader;
using System.Data;
using System.Drawing;

namespace CapstoneProject.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;

        public AdminService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<string> CreateUserByRoleAsync(AdminCreateUserRequest request)
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email);
            if (existing != null) throw new Exception("Email Exited.");
            var newUser = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Phone = request.Phone,
                Role = request.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Status = "Active",
                EmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser);

            return $"Created.";
        }

        public async Task<List<User>> GetAllUsersAsync(int currentUserId)
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            return allUsers.Where(user => user.UserId != currentUserId).ToList();
        }

        public async Task DeleteAsync(int userId)
        {   
            await _userRepository.DeleteAsync(userId);
        }

        public async Task UnlockAccountAsync(int userId)
        {
            await _userRepository.ChangeStatusAsync(userId);
        }

        // Thêm currentUserId vào tham số
        public async Task<List<AdminUserResponse>> SearchUsersAsync(string keyword, int currentUserId)
        {
            var users = await _userRepository.SearchUsersAsync(keyword);

            return users
                .Where(u => u.UserId != currentUserId) 
                .Select(u => new AdminUserResponse
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Status = u.Status,
                    Role = u.Role
                }).ToList();
        }


        public async Task<int> ImportUsersFromExcelAsync(Stream excelStream)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var usersToImport = new List<User>();
            var existingEmails = await _userRepository.GetAllEmailsAsync();
            var existingEmailSet = new HashSet<string>(existingEmails, StringComparer.OrdinalIgnoreCase);

            var validRoles = new HashSet<string> { "Student", "Mentor", "Council", "Admin" };

            using (var reader = ExcelReaderFactory.CreateReader(excelStream))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                });

                DataTable dt = result.Tables[0];

                foreach (DataRow row in dt.Rows)
                {
                    string email = row["Email"]?.ToString()?.Trim();
                    string fullName = row["FullName"]?.ToString()?.Trim();
                    string phone = row["Phone"]?.ToString()?.Trim();
                    string roleInput = dt.Columns.Contains("Role") ? row["Role"]?.ToString()?.Trim() : "Student";

                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone))
                        continue;

                    if (existingEmailSet.Contains(email))
                        continue;

                    string finalRole = validRoles.Contains(roleInput) ? roleInput : "Student";

                    string emailPrefix = email.Contains("@") ? email.Split('@')[0] : email;
                    string phoneSuffix = phone.Length >= 3 ? phone.Substring(phone.Length - 3) : phone;
                    string rawPassword = $"{emailPrefix}{phoneSuffix}@";
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(rawPassword);

                    var newUser = new User
                    {
                        Email = email,
                        FullName = fullName,
                        Phone = phone,
                        Role = finalRole,
                        PasswordHash = hashedPassword,
                        Status = "Active",
                        EmailVerified = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    usersToImport.Add(newUser);
                    existingEmailSet.Add(email);
                }
            }
            if (usersToImport.Any())
            {
                await _userRepository.AddRangeAsync(usersToImport);
            }

            return usersToImport.Count;
        }


        public async Task<byte[]> ExportStudentsToExcelAsync()
        {

            var students = await _userRepository.GetStudentsAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Student List");

                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "FullName";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "Phone Number";
                worksheet.Range("A1:D1").Style.Font.Bold = true;
                worksheet.Range("A1:D1").Style.Fill.BackgroundColor = XLColor.LightGray;
                int currentRow = 2;
                foreach (var student in students)
                {
                    worksheet.Cell(currentRow, 1).Value = student.UserId;
                    worksheet.Cell(currentRow, 2).Value = student.FullName;
                    worksheet.Cell(currentRow, 3).Value = student.Email;
                    worksheet.Cell(currentRow, 4).Value = $"'{student.Phone}";

                    currentRow++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}

    
