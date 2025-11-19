using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;
using System.Security.Claims;

namespace QLQA_TH.Controllers
{
    [Authorize(Policy = "BepAccess")]
    public class KitchenController : Controller
    {
        private readonly QLQAContext _context;

        public KitchenController(QLQAContext context)
        {
            _context = context;
        }

        // Helper: Lấy ID nhân viên hiện tại (Đầu bếp)
        private async Task<int?> GetCurrentMaNV()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _context.Users.FindAsync(userId);
                return user?.MaNV;
            }
            return null;
        }

        // 1. DANH SÁCH PHIẾU CHỜ (INDEX)
        public async Task<IActionResult> Index()
        {
            // A. KIỂM TRA SESSION: Bắt buộc hoàn thành món đang làm dở
            var currentCookingId = HttpContext.Session.GetInt32("CurrentCookingTicket");
            if (currentCookingId.HasValue)
            {
                var checkPhieu = await _context.PhieuDats.FindAsync(currentCookingId.Value);

                // Nếu phiếu vẫn đang "Đang làm" (1) -> Redirect vào làm tiếp
                if (checkPhieu != null && checkPhieu.TrangThai == 1)
                {
                    return RedirectToAction("CookingDetail", new { id = currentCookingId.Value });
                }
                else
                {
                    // Nếu phiếu đã xong/hủy -> Xóa session rác
                    HttpContext.Session.Remove("CurrentCookingTicket");
                }
            }

            // B. LẤY DANH SÁCH PHIẾU CHỜ (Chỉ lấy Status = 0)
            // Không hiển thị Status 1 vì nó đã thuộc về đầu bếp cụ thể
            var listPhieu = await _context.PhieuDats
                .Include(p => p.HoaDon).ThenInclude(h => h.Ban)
                .Include(p => p.DonHangs).ThenInclude(d => d.MonAn)
                .Where(p => p.TrangThai == 0)
                .OrderBy(p => p.ThoiGianDat) // FIFO: Cũ nhất lên đầu
                .ToListAsync();

            return View(listPhieu);
        }

        // 2. BẮT ĐẦU LÀM (AJAX) -> CHUYỂN 0 SANG 1
        [HttpPost]
        public async Task<IActionResult> StartCooking(int maPhieu)
        {
            var phieu = await _context.PhieuDats.FindAsync(maPhieu);

            // Validate: Chỉ nhận phiếu Chờ (0)
            if (phieu == null || phieu.TrangThai != 0)
                return Json(new { success = false, msg = "Phiếu không hợp lệ hoặc đã có người nhận!" });

            // 1. Cập nhật trạng thái
            phieu.TrangThai = 1; // Đang làm

            // 2. Lưu vết Đầu bếp
            phieu.MaNV_Bep = await GetCurrentMaNV();

            await _context.SaveChangesAsync();

            // 3. Set Session để khóa màn hình vào phiếu này
            HttpContext.Session.SetInt32("CurrentCookingTicket", maPhieu);

            return Json(new { success = true });
        }

        // 3. GIAO DIỆN CHẾ ĐỘ TẬP TRUNG (FULLSCREEN)
        [HttpGet]
        public async Task<IActionResult> CookingDetail(int id)
        {
            // Bảo mật: Chỉ cho phép vào đúng phiếu đang làm trong Session
            var sessionTicket = HttpContext.Session.GetInt32("CurrentCookingTicket");
            if (sessionTicket != id)
            {
                return RedirectToAction("Index");
            }

            var phieu = await _context.PhieuDats
                .Include(p => p.HoaDon).ThenInclude(h => h.Ban)
                .Include(p => p.DonHangs).ThenInclude(d => d.MonAn)
                .FirstOrDefaultAsync(p => p.MaPhieu == id);

            // Nếu phiếu không tồn tại hoặc không phải đang làm -> Đá về Index
            if (phieu == null || phieu.TrangThai != 1)
            {
                HttpContext.Session.Remove("CurrentCookingTicket");
                return RedirectToAction("Index");
            }

            return View(phieu);
        }

        // 4. HOÀN TẤT CHẾ BIẾN (AJAX) -> CHUYỂN 1 SANG 2
        [HttpPost]
        public async Task<IActionResult> CompleteCooking(int maPhieu)
        {
            var phieu = await _context.PhieuDats.FindAsync(maPhieu);
            if (phieu == null) return Json(new { success = false, msg = "Lỗi dữ liệu" });

            phieu.TrangThai = 2; // 2: Bếp xong (Chờ cung ứng)
            await _context.SaveChangesAsync();

            // Xóa Session để mở khóa màn hình, quay lại Index chọn món mới
            HttpContext.Session.Remove("CurrentCookingTicket");

            return Json(new { success = true });
        }

        // ====================================================================================
        // 5. LỊCH SỬ CHẾ BIẾN (HISTORY)
        // ====================================================================================
        [HttpGet]
        public async Task<IActionResult> History(int? status, DateTime? fromDate, DateTime? toDate)
        {
            // 1. Lấy ID Đầu bếp hiện tại
            int? currentMaNV = await GetCurrentMaNV();
            if (currentMaNV == null) return RedirectToAction("Login", "Account");

            // 2. Truy vấn: Chỉ lấy phiếu do chính đầu bếp này làm (MaNV_Bep)
            var query = _context.PhieuDats
                .Include(p => p.HoaDon).ThenInclude(h => h.Ban)
                // Không cần Include DonHangs ở đây vì không hiển thị chi tiết trên lưới
                .Where(p => p.MaNV_Bep == currentMaNV);

            // 3. Bộ lọc
            if (status.HasValue)
            {
                query = query.Where(p => p.TrangThai == status.Value);
            }
            // Nếu không chọn status, mặc định chỉ hiện những phiếu Đã xong (2) hoặc Đã phục vụ (4)
            // Để tránh lẫn lộn với phiếu đang làm (1) hoặc chờ (0)
            else
            {
                query = query.Where(p => p.TrangThai == 2 || p.TrangThai == 4);
            }

            if (fromDate.HasValue)
                query = query.Where(p => p.ThoiGianDat.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(p => p.ThoiGianDat.Date <= toDate.Value.Date);

            var listPhieu = await query.OrderByDescending(p => p.ThoiGianDat).ToListAsync();

            // 4. View Data
            ViewBag.StatusList = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Tất cả lịch sử --" },
                new SelectListItem { Value = "2", Text = "Đã hoàn tất (Chờ bưng)" },
                new SelectListItem { Value = "4", Text = "Đã phục vụ khách" }
            };

            ViewData["CurrentStatus"] = status;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            return View(listPhieu);
        }

        // 6. API XEM CHI TIẾT
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int maPhieu)
        {
            var phieuDat = await _context.PhieuDats
                .Include(p => p.HoaDon).ThenInclude(h => h.Ban)
                .Include(p => p.DonHangs).ThenInclude(d => d.MonAn)
                .Include(p => p.NhanVienDat)
                .Include(p => p.NhanVienBep)
                .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

            if (phieuDat == null) return NotFound();

            return PartialView("~/Views/Kitchen/_KitchenOrderDetailPartial.cshtml", phieuDat);
        }
    }
}