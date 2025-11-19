using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;

namespace QLQA_TH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdminRole")]
    public class TableController : Controller
    {
        private readonly QLQAContext _context;

        public TableController(QLQAContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH & TÌM KIẾM
        public async Task<IActionResult> Index(string searchString)
        {
            var banQuery = _context.Bans.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                if (int.TryParse(searchString, out int id))
                {
                    banQuery = banQuery.Where(b => b.TenBan.Contains(searchString) || b.MaBan == id);
                }
                else
                {
                    banQuery = banQuery.Where(b => b.TenBan.Contains(searchString));
                }
            }

            var listBan = await banQuery.OrderBy(b => b.TenBan).ToListAsync();
            ViewData["CurrentFilter"] = searchString;

            return View(listBan);
        }

        // 2. HIỂN THỊ FORM (GET)
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            Ban ban = new Ban();
            if (id == null || id == 0)
            {
                // Tạo mới: Giá trị mặc định
                ban.TrangThai = 0;
                ban.SoGhe = 4;
            }
            else
            {
                // Cập nhật: Tìm trong DB
                ban = await _context.Bans.FindAsync(id);
                if (ban == null) return NotFound();
            }
            return PartialView("_UpsertPartial", ban);
        }

        // 3. XỬ LÝ LƯU DỮ LIỆU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Ban ban)
        {
            // --- KIỂM TRA TRÙNG TÊN ---
            // Tìm bàn nào có cùng Tên nhưng khác ID
            bool isDuplicate = await _context.Bans.AnyAsync(b => b.TenBan == ban.TenBan && b.MaBan != ban.MaBan);
            if (isDuplicate)
            {
                ModelState.AddModelError("TenBan", "Tên bàn này đã tồn tại, vui lòng đặt tên khác.");
            }

            if (ModelState.IsValid)
            {
                if (ban.MaBan == 0)
                {
                    _context.Bans.Add(ban);
                }
                else
                {
                    _context.Bans.Update(ban);
                }
                await _context.SaveChangesAsync();

                // Trả về JSON báo thành công để JavaScript reload trang
                return Json(new { isValid = true });
            }

            // Nếu lỗi, trả về PartialView để hiện lỗi trên Modal
            return PartialView("_UpsertPartial", ban);
        }

        // 4. XÓA BÀN (POST)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return Json(new { success = false, message = "Không tìm thấy bàn" });

            // Kiểm tra ràng buộc trước khi xóa (VD: Bàn đang có khách hoặc có hóa đơn)
            // var dangSuDung = ...

            _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa thành công" });
        }
    }
}