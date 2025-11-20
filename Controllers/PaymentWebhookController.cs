using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;
using System.Text.Json.Serialization; // Bắt buộc
using System.Text.RegularExpressions;

namespace QLQA_TH.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentWebhookController : ControllerBase
    {
        private readonly QLQAContext _context;

        public PaymentWebhookController(QLQAContext context)
        {
            _context = context;
        }

        [HttpPost("Receive")]
        public async Task<IActionResult> ReceivePayment([FromBody] BankTransactionDto data)
        {
            try
            {
                // 1. Log kiểm tra
                Console.WriteLine($"--- WEBHOOK DATA ---");
                Console.WriteLine($"Content: {data.Content} | Amount: {data.TransferAmount} | ID: {data.Id}");

                if (data == null) return BadRequest(new { success = false, message = "Data Null" });

                string contentToCheck = data.Content ?? data.Description ?? "";

                // 2. Regex lấy mã hóa đơn
                // Chuỗi của bạn: "HD11 GD 608273..." -> Regex sẽ bắt được số 11
                var match = Regex.Match(contentToCheck, @"HD\s*(\d+)", RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    Console.WriteLine("=> Không tìm thấy mã HD trong nội dung ck");
                    return Ok(new { success = false, message = "Không tìm thấy mã HD" });
                }

                int maHD = int.Parse(match.Groups[1].Value);

                // 3. Tìm hóa đơn
                var hoaDon = await _context.HoaDons.FindAsync(maHD);
                if (hoaDon == null)
                    return Ok(new { success = false, message = "Hóa đơn không tồn tại" });

                // Check nếu đã thanh toán rồi thì bỏ qua
                if (hoaDon.TrangThaiHoaDon != 0)
                    return Ok(new { success = true, message = "Đơn đã thanh toán rồi" });

                // 4. SO SÁNH TIỀN
                // Ép kiểu từ decimal (JSON) sang int (Database của bạn)
                int soTienNhan = (int)data.TransferAmount;

                // Cho phép sai số nhỏ hoặc khách chuyển thiếu 1 chút cũng báo lỗi
                if (soTienNhan < hoaDon.ThanhTien)
                {
                    return Ok(new { success = false, message = $"Thiếu tiền: Nhận {soTienNhan}, Cần {hoaDon.ThanhTien}" });
                }

                // 5. LƯU THANH TOÁN
                var tt = new ThanhToan
                {
                    MaHD = maHD,
                    ThoiGian = DateTime.Now,
                    SoTien = soTienNhan,
                    PhuongThuc = "Chuyển khoản (Auto)",
                    TrangThai = 1,
                    GhiChu = $"Auto Bank: {contentToCheck}",
                    // Lưu ID giao dịch ngân hàng (chuyển long sang string để lưu vào cột nvarchar)
                    MaGiaoDichCong = data.Id.ToString()
                };
                _context.ThanhToans.Add(tt);

                // Cập nhật Hóa Đơn
                hoaDon.TrangThaiHoaDon = 1;
                hoaDon.ThoiGianRa = DateTime.Now;
                hoaDon.PhuongThucThanhToan = "Chuyển khoản";

                // Trả bàn (nếu có)
                if (hoaDon.MaBan.HasValue)
                {
                    var ban = await _context.Bans.FindAsync(hoaDon.MaBan);
                    if (ban != null) ban.TrangThai = 0;
                }

                await _context.SaveChangesAsync();

                Console.WriteLine($"=> SUCCESS: Đã thanh toán HD #{maHD}");
                return Ok(new { success = true, message = "Thanh toán thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI SERVER: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    // --- DTO KHỚP 100% VỚI JSON CỦA SEPAY ---
    public class BankTransactionDto
    {
        // SỬA QUAN TRỌNG: Id là long (số), không phải string
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("gateway")]
        public string? Gateway { get; set; }

        [JsonPropertyName("transactionDate")]
        public string? TransactionDate { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("subAccount")]
        public string? SubAccount { get; set; } // Thêm trường này cho khớp JSON (dù null)

        [JsonPropertyName("code")]
        public string? Code { get; set; } // Thêm trường này cho khớp JSON (dù null)

        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("transferType")]
        public string? TransferType { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        // SỬA QUAN TRỌNG: TransferAmount là decimal
        [JsonPropertyName("transferAmount")]
        public decimal TransferAmount { get; set; }

        [JsonPropertyName("referenceCode")]
        public string? ReferenceCode { get; set; }
    }
}