using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;

namespace QLQA_TH.Controllers
{
    [Authorize(Policy = "ThuNganAccess")]
    public class InvoiceController : Controller
    {
        private readonly QLQAContext _context;
        private readonly IConfiguration _configuration;

        public InvoiceController(QLQAContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. DANH SÁCH HÓA ĐƠN
        public async Task<IActionResult> Index(int? status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.HoaDons
                .Include(h => h.Ban)
                .Include(h => h.NhanVien)
                // Cần Include sâu để hiển thị tổng tiền ngay ở danh sách (nếu cần)
                .Include(h => h.PhieuDats).ThenInclude(p => p.DonHangs)
                .AsQueryable();

            if (!status.HasValue) status = 0; // Mặc định xem đơn chưa thanh toán

            if (status.HasValue)
                query = query.Where(h => h.TrangThaiHoaDon == status.Value);

            if (fromDate.HasValue)
                query = query.Where(h => h.ThoiGianVao.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(h => h.ThoiGianVao.Date <= toDate.Value.Date);

            var listHD = await query.OrderByDescending(h => h.ThoiGianVao).ToListAsync();

            // Tính toán lại tổng tiền cho từng hóa đơn trong danh sách (để hiển thị đúng)
            foreach (var hd in listHD)
            {
                hd.TongTien = hd.PhieuDats
                    .Where(p => p.TrangThai != 3) // Trừ phiếu hủy
                    .SelectMany(p => p.DonHangs)
                    .Sum(d => d.SoLuong * d.Gia);

                hd.ThanhTien = hd.TongTien - hd.GiamGia;
            }

            return View(listHD);
        }

        // 2. CHI TIẾT THANH TOÁN (VIEW CHECKOUT)
        [HttpGet]
        public async Task<IActionResult> Checkout(int id)
        {
            var hd = await _context.HoaDons
                .Include(h => h.Ban)
                // QUAN TRỌNG: Phải Include DonHangs và MonAn để hiển thị tên món và tính tiền
                .Include(h => h.PhieuDats)
                    .ThenInclude(p => p.DonHangs)
                        .ThenInclude(d => d.MonAn)
                .Include(h => h.ThanhToans)
                .FirstOrDefaultAsync(h => h.MaHD == id);

            if (hd == null) return NotFound();

            // --- 1. LOGIC CHẶN THANH TOÁN ---
            // Chặn nếu có phiếu chưa xong (Khác 3-Hủy và 4-Đã phục vụ)
            bool conMonChuaRa = hd.PhieuDats.Any(p => p.TrangThai != 3 && p.TrangThai != 4);
            ViewBag.CanCheckout = !conMonChuaRa;

            // --- 2. LOGIC TÍNH TIỀN (SỬA LỖI TẠI ĐÂY) ---
            // Chỉ lấy các phiếu KHÔNG bị hủy (TrangThai != 3)
            var validPhieus = hd.PhieuDats.Where(p => p.TrangThai != 3).ToList();

            // Tính tổng tiền hàng
            // Lưu ý: DonHang.ThanhTien là computed column trong DB, nhưng ở đây ta tính thủ công để chắc chắn
            decimal tongTienHang = validPhieus
                .SelectMany(p => p.DonHangs)
                .Sum(d => (decimal)(d.SoLuong * d.Gia)); // Ép kiểu decimal để tránh tràn số nếu Gia là int

            hd.TongTien = (int)tongTienHang;
            hd.ThanhTien = (int)tongTienHang - hd.GiamGia;

            // Cấu hình QR
            var qr = _configuration.GetSection("VietQR");
            ViewBag.BankId = qr["BankId"];
            ViewBag.AccountNo = qr["AccountNo"];
            ViewBag.AccountName = qr["AccountName"];
            ViewBag.Template = qr["Template"];

            return View(hd);
        }

        // 3. XÁC NHẬN THANH TOÁN (API POST)
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int maHD, int amountGiven, string paymentMethod)
        {
            // --- QUAN TRỌNG: SỬA LỖI Ở ĐÂY ---
            // Trước đây bạn có thể thiếu .ThenInclude(DonHangs), khiến Sum = 0
            var hd = await _context.HoaDons
                .Include(h => h.PhieuDats)
                    .ThenInclude(p => p.DonHangs) // <--- BẮT BUỘC PHẢI CÓ DÒNG NÀY
                .FirstOrDefaultAsync(h => h.MaHD == maHD);

            if (hd == null || hd.TrangThaiHoaDon != 0)
                return Json(new { success = false, msg = "Hóa đơn lỗi hoặc đã thanh toán!" });

            // 1. Check trạng thái món (Server side validation)
            bool conMonChuaRa = hd.PhieuDats.Any(p => p.TrangThai != 3 && p.TrangThai != 4);
            if (conMonChuaRa)
            {
                return Json(new { success = false, msg = "Không thể thanh toán! Vẫn còn món chưa phục vụ." });
            }

            // 2. Tính toán lại số tiền TRƯỚC KHI LƯU DB
            // Đề phòng client gửi lên sai hoặc hack, ta tự tính lại ở Server
            var validPhieus = hd.PhieuDats.Where(p => p.TrangThai != 3);

            decimal realTotal = validPhieus
                .SelectMany(p => p.DonHangs)
                .Sum(d => (decimal)(d.SoLuong * d.Gia));

            decimal finalAmount = realTotal - hd.GiamGia;

            // Cập nhật lại vào Object Hóa Đơn để lưu cứng vào DB
            hd.TongTien = (int)realTotal;
            hd.ThanhTien = (int)finalAmount;

            // 3. Tạo bản ghi thanh toán
            var tt = new ThanhToan
            {
                MaHD = maHD,
                SoTien = (int)finalAmount, // Lưu số tiền thực tế phải trả
                PhuongThuc = paymentMethod,
                ThoiGian = DateTime.Now,
                TrangThai = 1,
                GhiChu = $"Thu: {amountGiven} | Tổng đơn: {finalAmount}"
            };
            _context.ThanhToans.Add(tt);

            // 4. Cập nhật trạng thái hóa đơn
            hd.TrangThaiHoaDon = 1; // Đã thanh toán
            hd.ThoiGianRa = DateTime.Now;
            hd.PhuongThucThanhToan = paymentMethod;

            // 5. Trả bàn
            if (hd.MaBan.HasValue)
            {
                var ban = await _context.Bans.FindAsync(hd.MaBan);
                if (ban != null) ban.TrangThai = 0;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, msg = "Thanh toán thành công!" });
        }

        // 4. API CHECK TRẠNG THÁI (POLLING)
        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(int maHD)
        {
            var hd = await _context.HoaDons.AsNoTracking()
                .Select(h => new { h.MaHD, h.TrangThaiHoaDon, h.PhuongThucThanhToan })
                .FirstOrDefaultAsync(h => h.MaHD == maHD);

            if (hd != null && hd.TrangThaiHoaDon == 1)
            {
                return Json(new { paid = true, method = hd.PhuongThucThanhToan });
            }
            return Json(new { paid = false });
        }
    }
}