namespace TicketService.Models;

public class Ticket
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty; // Tên khách hàng
    public string Email { get; set; } = string.Empty;        // Email liên hệ
    public string Title { get; set; } = string.Empty;        // Tiêu đề yêu cầu
    public string Description { get; set; } = string.Empty;  // Nội dung chi tiết
    public string Priority { get; set; } = "Normal";         // Mức độ ưu tiên: Low, Normal, High
    public string Status { get; set; } = "Open";             // Trạng thái: Open, InProgress, Resolved
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Thời gian gửi
}