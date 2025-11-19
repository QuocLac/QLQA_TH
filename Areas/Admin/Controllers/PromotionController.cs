using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;

namespace QLQA_TH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdminRole")]
    public class PromotionController : Controller
    {
        private readonly QLQAContext _context;

        public PromotionController(QLQAContext context)
        {
            _context = context;
        }

        // 1. INDEX
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.KhuyenMais.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(k => k.TenKM.Contains(searchString) || k.MaVoucher.Contains(searchString));
            }

            var listKM = await query.OrderByDescending(k => k.NgayKetThuc).ToListAsync();
            ViewData["CurrentFilter"] = searchString;
            return View(listKM);
        }

        // 2. UPSERT (GET)
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            KhuyenMai km = new KhuyenMai();

            if (id == null || id == 0)
            {
                // Mặc định: Bắt đầu hôm nay, kết thúc sau 1 tuần
                km.NgayBatDau = DateTime.Now;
                km.NgayKetThuc = DateTime.Now.AddDays(7);
                km.SoLuong = 100;
                km.DieuKienApDung = 0;
            }
            else
            {
                km = await _context.KhuyenMais.FindAsync(id);
                if (km == null) return NotFound();
            }
            return PartialView("_UpsertPartial", km);
        }

        // 3. UPSERT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(KhuyenMai km)
        {
            // --- VALIDATION LOGIC ---

            // 1. Check trùng mã Voucher
            bool isDuplicate = await _context.KhuyenMais
                .AnyAsync(k => k.MaVoucher == km.MaVoucher && k.MaKM != km.MaKM);

            if (isDuplicate)
            {
                ModelState.AddModelError("MaVoucher", "Mã Voucher này đã tồn tại.");
            }

            // 2. Check Ngày kết thúc < Ngày bắt đầu
            if (km.NgayKetThuc < km.NgayBatDau)
            {
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            }

            // 3. Check phải có ít nhất 1 loại giảm giá (% hoặc Tiền)
            if ((km.PhanTramGiam == null || km.PhanTramGiam == 0) && (km.SoTienGiam == null || km.SoTienGiam == 0))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập % Giảm hoặc Số tiền giảm.");
            }

            if (ModelState.IsValid)
            {
                if (km.MaKM == 0)
                {
                    _context.KhuyenMais.Add(km);
                }
                else
                {
                    _context.KhuyenMais.Update(km);
                }
                await _context.SaveChangesAsync();
                return Json(new { isValid = true });
            }

            return PartialView("_UpsertPartial", km);
        }

        // 4. DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var km = await _context.KhuyenMais.FindAsync(id);
            if (km == null) return Json(new { success = false, message = "Không tìm thấy khuyến mãi" });

            _context.KhuyenMais.Remove(km);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa thành công" });
        }
    }
}