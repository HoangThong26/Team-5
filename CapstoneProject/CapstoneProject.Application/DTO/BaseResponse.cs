namespace CapstoneProject.Application.DTO
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; } = true; // Thành công hay thất bại
        public string Message { get; set; } = string.Empty; // Thông báo (Lỗi hoặc Thành công)
        public T? Data { get; set; } // Dữ liệu thực tế trả về (List, Object, int,...)
    }
}
