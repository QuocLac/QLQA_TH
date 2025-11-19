using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Extensions;
using QLQA_TH.Models;
using QLQA_TH.Models.ViewModels;
using System.Security.Claims;

namespace QLQA_TH.Controllers
{
    [Authorize(Policy = "PhucVuAccess")]
    public class OrderController : Controller
    {
        private readonly QLQAContext _context;

        public OrderController(QLQAContext context)
        {
            _context = context;
        }

        // === HELPER: LẤY ID NHÂN VIÊN HIỆN TẠI ===
        private async Task<int?> GetCurrentMaNV()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var user = await _context.Users.FindAsync(userId);
                return user?.MaNV;
            }
            return null; // Hoặc trả về 1 (Admin) nếu null để test
        }

        // 1. DANH SÁCH BÀN
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.Bans.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
                query = query.Where(b => b.TenBan.Contains(searchString));

            return View(await query.OrderBy(b => b.TenBan).ToListAsync());
        }

        // 2. CHI TIẾT GỌI MÓN
        [HttpGet]
        public async Task<IActionResult> Detail(int tableId, string searchString, int? categoryId)
        {
            var ban = await _context.Bans.FindAsync(tableId);
            if (ban == null) return NotFound();

            var cart = HttpContext.Session.Get<List<CartItem>>($"Cart_{tableId}") ?? new List<CartItem>();

            // Lấy Hóa đơn kèm theo danh sách Phiếu Đặt để hiển thị trạng thái
            var hoaDon = await _context.HoaDons
                .Include(h => h.PhieuDats).ThenInclude(p => p.DonHangs)
                .FirstOrDefaultAsync(h => h.MaBan == tableId && h.TrangThaiHoaDon == 0);

            var menuQuery = _context.MonAns.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
                menuQuery = menuQuery.Where(m => m.TenMon.Contains(searchString));
            if (categoryId.HasValue)
                menuQuery = menuQuery.Where(m => m.MaLoai == categoryId);

            var model = new OrderDetailViewModel
            {
                Ban = ban,
                HoaDon = hoaDon,
                CurrentCart = cart,
                MenuItems = await menuQuery.ToListAsync(),
                Categories = await _context.LoaiMons.ToListAsync(),
                CurrentSearch = searchString,
                CurrentCategory = categoryId
            };

            return View(model);
        }

        // 3. THÊM MÓN VÀO SESSION (Giữ nguyên)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int tableId, int maMon)
        {
            var monAn = await _context.MonAns.FindAsync(maMon);
            if (monAn == null) return Json(new { success = false, msg = "Lỗi món" });

            var cart = HttpContext.Session.Get<List<CartItem>>($"Cart_{tableId}") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.MaMon == maMon);

            if (item != null) item.SoLuong++;
            else cart.Add(new CartItem
            {
                MaMon = maMon,
                TenMon = monAn.TenMon,
                HinhAnh = monAn.HinhAnh,
                Gia = monAn.Gia,
                SoLuong = 1
            });

            HttpContext.Session.Set($"Cart_{tableId}", cart);
            return Json(new { success = true });
        }

        // 4. GIẢM MÓN (SESSION - Giữ nguyên)
        [HttpPost]
        public IActionResult DecreaseItem(int tableId, int maMon)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>($"Cart_{tableId}");
            if (cart != null)
            {
                var item = cart.FirstOrDefault(c => c.MaMon == maMon);
                if (item != null)
                {
                    if (item.SoLuong > 1) item.SoLuong--;
                    else cart.Remove(item);
                    HttpContext.Session.Set($"Cart_{tableId}", cart);
                }
            }
            return Json(new { success = true });
        }

        // 5. RELOAD CART (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCartComponent(int tableId)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>($"Cart_{tableId}") ?? new List<CartItem>();

            var hoaDon = await _context.HoaDons
                .Include(h => h.PhieuDats).ThenInclude(p => p.DonHangs)
                .FirstOrDefaultAsync(h => h.MaBan == tableId && h.TrangThaiHoaDon == 0);

            var ban = await _context.Bans.FindAsync(tableId);

            var model = new OrderDetailViewModel
            {
                Ban = ban ?? new Ban { MaBan = tableId },
                HoaDon = hoaDon,
                CurrentCart = cart
            };

            return PartialView("_CartPartial", model);
        }

        // 6. ĐẶT MÓN - TẠO PHIẾU ĐẶT (LOGIC MỚI)
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int tableId)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>($"Cart_{tableId}");
            if (cart == null || !cart.Any())
                return Json(new { success = false, msg = "Giỏ hàng trống!" });

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                int? currentMaNV = await GetCurrentMaNV(); // Lấy người đang thao tác

                // Tìm Hóa đơn mở
                var hoaDon = await _context.HoaDons
                    .FirstOrDefaultAsync(h => h.MaBan == tableId && h.TrangThaiHoaDon == 0);

                // A. Nếu chưa có Hóa đơn -> Tạo mới
                if (hoaDon == null)
                {
                    hoaDon = new HoaDon
                    {
                        MaBan = tableId,
                        ThoiGianVao = DateTime.Now,
                        TrangThaiHoaDon = 0,
                        MaNV = currentMaNV ?? 1, // Người mở bàn (Thu ngân/Phục vụ)
                        TongTien = 0,
                        ThanhTien = 0
                    };
                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync();

                    var ban = await _context.Bans.FindAsync(tableId);
                    if (ban != null) { ban.TrangThai = 1; _context.Bans.Update(ban); }
                }

                // B. Tạo PHIẾU ĐẶT (Batch mới)
                var phieuDat = new PhieuDat
                {
                    MaHD = hoaDon.MaHD,
                    ThoiGianDat = DateTime.Now,
                    TrangThai = 0, // Chờ bếp
                    MaNV_Dat = currentMaNV // <--- LƯU VẾT NGƯỜI GỌI MÓN
                };
                _context.PhieuDats.Add(phieuDat);
                await _context.SaveChangesAsync();

                // C. Lưu chi tiết món vào Phiếu này
                foreach (var item in cart)
                {
                    _context.DonHangs.Add(new DonHang
                    {
                        MaPhieu = phieuDat.MaPhieu,
                        MaMon = item.MaMon,
                        SoLuong = item.SoLuong,
                        Gia = item.Gia
                    });
                }
                await _context.SaveChangesAsync();

                // D. Tính lại tổng tiền Hóa đơn (Cộng dồn các phiếu chưa hủy)
                var total = await _context.DonHangs
                    .Where(d => d.PhieuDat.MaHD == hoaDon.MaHD && d.PhieuDat.TrangThai != 3)
                    .SumAsync(d => d.SoLuong * d.Gia);

                hoaDon.TongTien = total;
                hoaDon.ThanhTien = total - hoaDon.GiamGia;
                _context.HoaDons.Update(hoaDon);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                HttpContext.Session.Remove($"Cart_{tableId}");

                return Json(new { success = true, msg = "Đã gửi đơn xuống bếp!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, msg = "Lỗi: " + ex.Message });
            }
        }

        // 7. HỦY PHIẾU CHỜ
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int maHD)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var hoaDon = await _context.HoaDons
                    .Include(h => h.PhieuDats).ThenInclude(p => p.DonHangs)
                    .FirstOrDefaultAsync(h => h.MaHD == maHD);

                if (hoaDon == null || hoaDon.TrangThaiHoaDon != 0)
                    return Json(new { success = false, msg = "Không thể thao tác!" });

                // Chỉ hủy các phiếu đang CHỜ (0)
                var phieuCho = hoaDon.PhieuDats.Where(p => p.TrangThai == 0).ToList();
                if (!phieuCho.Any())
                    return Json(new { success = false, msg = "Không có phiếu chờ nào để hủy!" });

                foreach (var phieu in phieuCho) phieu.TrangThai = 3; // Hủy
                await _context.SaveChangesAsync();

                // Tính lại tiền
                long newTotal = 0;
                var validPhieus = hoaDon.PhieuDats.Where(p => p.TrangThai != 3).ToList();
                foreach (var p in validPhieus) newTotal += p.DonHangs.Sum(d => (long)d.SoLuong * d.Gia);

                hoaDon.TongTien = (int)newTotal;
                hoaDon.ThanhTien = (int)newTotal - hoaDon.GiamGia;

                // Nếu hủy hết -> Hủy luôn hóa đơn & Trả bàn
                if (!validPhieus.Any())
                {
                    hoaDon.TrangThaiHoaDon = 2; // Hủy
                    hoaDon.ThoiGianRa = DateTime.Now;

                    if (hoaDon.MaBan.HasValue)
                    {
                        var ban = await _context.Bans.FindAsync(hoaDon.MaBan);
                        if (ban != null) { ban.TrangThai = 0; _context.Bans.Update(ban); }
                        HttpContext.Session.Remove($"Cart_{hoaDon.MaBan}");
                    }
                    _context.HoaDons.Update(hoaDon);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, msg = "Đã hủy đơn và trả bàn!" });
                }

                _context.HoaDons.Update(hoaDon);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, msg = "Đã hủy các món chờ!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, msg = "Lỗi: " + ex.Message });
            }
        }

        // 8. XÁC NHẬN ĐÃ PHỤC VỤ
        [HttpPost]
        public async Task<IActionResult> ConfirmServed(int maPhieu)
        {
            var phieu = await _context.PhieuDats.FindAsync(maPhieu);
            if (phieu == null || phieu.TrangThai != 2)
                return Json(new { success = false, msg = "Phiếu chưa sẵn sàng!" });

            phieu.TrangThai = 4; // Đã phục vụ
            phieu.MaNV_PhucVu = await GetCurrentMaNV(); // <--- LƯU VẾT NGƯỜI BƯNG

            await _context.SaveChangesAsync();
            return Json(new { success = true, msg = "Đã cập nhật!" });
        }

        // 9. LỊCH SỬ PHIẾU ĐẶT (HISTORY) - LỌC THEO NGƯỜI ĐẶT
        [HttpGet]
        public async Task<IActionResult> History(int? status, DateTime? fromDate, DateTime? toDate)
        {
            int? currentMaNV = await GetCurrentMaNV();
            if (currentMaNV == null) return RedirectToAction("Login", "Account");

            var query = _context.PhieuDats
                .Include(p => p.HoaDon).ThenInclude(h => h.Ban)
                .Include(p => p.DonHangs).ThenInclude(d => d.MonAn)
                // QUAN TRỌNG: Lọc theo MaNV_Dat (Người tạo phiếu)
                .Where(p => p.MaNV_Dat == currentMaNV);

            if (status.HasValue)
                query = query.Where(p => p.TrangThai == status.Value);

            if (fromDate.HasValue)
                query = query.Where(p => p.ThoiGianDat.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(p => p.ThoiGianDat.Date <= toDate.Value.Date);

            var listPhieu = await query.OrderByDescending(p => p.ThoiGianDat).ToListAsync();

            ViewBag.StatusList = GetPhieuDatStatusList();
            ViewData["CurrentStatus"] = status;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            return View(listPhieu);
        }

        // 10. API NOTIFICATION
        [HttpGet]
        public async Task<IActionResult> GetReadyNotifications()
        {
            var readyPhieus = await _context.PhieuDats
                .Include(p => p.HoaDon).ThenInclude(h => h.Ban)
                .Include(p => p.DonHangs).ThenInclude(d => d.MonAn)
                .Where(p => p.TrangThai == 2)
                .OrderBy(p => p.ThoiGianDat)
                .Select(p => new {
                    p.MaPhieu,
                    TenBan = p.HoaDon.Ban.TenBan,
                    ThoiGian = p.ThoiGianDat.ToString("HH:mm"),
                    MonAnSummary = string.Join(", ", p.DonHangs.Select(d => $"{d.SoLuong}x {d.MonAn.TenMon}"))
                })
                .ToListAsync();
            return Json(readyPhieus);
        }

        // 11. API MODAL CHI TIẾT PHIẾU
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int maPhieu)
        {
            var phieuDat = await _context.PhieuDats
                .Include(p => p.HoaDon).ThenInclude(h => h.Ban)
                .Include(p => p.DonHangs).ThenInclude(d => d.MonAn)
                .FirstOrDefaultAsync(p => p.MaPhieu == maPhieu);

            if (phieuDat == null) return NotFound();
            return PartialView("_OrderDetailPartial", phieuDat);
        }

        private List<SelectListItem> GetPhieuDatStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Tất cả --" },
                new SelectListItem { Value = "0", Text = "Chờ xác nhận" },
                new SelectListItem { Value = "1", Text = "Đang làm" },
                new SelectListItem { Value = "2", Text = "Bếp xong" },
                new SelectListItem { Value = "3", Text = "Đã hủy" },
                new SelectListItem { Value = "4", Text = "Đã phục vụ" }
            };
        }
    }
}