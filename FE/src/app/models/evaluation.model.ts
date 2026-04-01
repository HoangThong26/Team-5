export interface EvaluationDetails {
  evaluationId?: number; // ID của bản ghi chấm điểm (nếu có)
  defenseId: number;     // ID buổi bảo vệ
  councilMemberUserId: number; // ID của thành viên hội đồng đang chấm
  presentationScore: number;   // Điểm thuyết trình (30%)
  demoScore: number;           // Điểm demo (50%)
  qaScore: number;             // Điểm hỏi đáp (20%)
  score: number;
  comment: string;             // Nhận xét của hội đồng
  updatedAt?: Date;            // Thời gian cập nhật cuối cùng
}
export interface AssignedDefense {
  defenseId: number;            // ID buổi bảo vệ
  groupName: string;            // Tên nhóm sinh viên
  topicTitle: string;           // Tên đề tài đồ án
  scheduledDate: string;        // Ngày giờ bắt đầu (đã khớp với API của bạn)
  endTime: string;              // Giờ kết thúc
  room: string;                 // Phòng bảo vệ
  status: 'Scheduled' | 'Completed' | 'In Progress'; // Trạng thái hiện tại
  evaluation?: EvaluationDetails; // Dữ liệu điểm đi kèm (nếu Backend đã hỗ trợ trả về chung)
}