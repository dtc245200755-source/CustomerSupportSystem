using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using TicketService.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình CSDL SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tickets.db"));

// 2. Bật CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();

// Tự động tạo CSDL nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// -------------------------------------------------------------
// CÁC ENDPOINT REST API
// -------------------------------------------------------------

// API 1: Lấy danh sách tất cả yêu cầu hỗ trợ
app.MapGet("/api/tickets", async (AppDbContext db) =>
    await db.Tickets.OrderByDescending(t => t.CreatedAt).ToListAsync());

// API 2: Lấy chi tiết 1 yêu cầu theo ID
app.MapGet("/api/tickets/{id:int}", async (int id, AppDbContext db) =>
    await db.Tickets.FindAsync(id) is Ticket ticket 
        ? Results.Ok(ticket) 
        : Results.NotFound(new { message = $"Không tìm thấy Ticket #{id}" }));

// API 3: Khách hàng gửi yêu cầu hỗ trợ mới
app.MapPost("/api/tickets", async (Ticket ticket, AppDbContext db, ILogger<Program> logger) =>
{
    ticket.CreatedAt = DateTime.UtcNow;
    ticket.Status = "Open";

    db.Tickets.Add(ticket);
    await db.SaveChangesAsync();

    logger.LogInformation("Đã tiếp nhận Ticket #{Id} từ khách hàng: {Customer}", ticket.Id, ticket.CustomerName);
    return Results.Created($"/api/tickets/{ticket.Id}", ticket);
});

// API 4: Cập nhật trạng thái Ticket (Open -> InProgress -> Resolved)
app.MapPut("/api/tickets/{id:int}/status", async (int id, string status, AppDbContext db, ILogger<Program> logger) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket is null) 
        return Results.NotFound(new { message = $"Không tìm thấy Ticket #{id}" });

    ticket.Status = status;
    await db.SaveChangesAsync();

    logger.LogInformation("Cập nhật trạng thái Ticket #{Id} thành: {Status}", id, status);
    return Results.Ok(ticket);
});

app.Run();