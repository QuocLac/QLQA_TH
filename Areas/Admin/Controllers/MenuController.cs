using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;

namespace QLQA_TH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdminRole")]
    public class MenuController : Controller
    {
        private readonly QLQAContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MenuController(QLQAContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ============================================================
        // 1. INDEX (DANH SÁCH + TÌM KIẾM + LỌC LOẠI)
        // ============================================================
        public async Task<IActionResult> Index(string searchString, int? maLoai)
        {
            // 1. Khởi tạo query, Include loại món để hiển thị tên
            var menuQuery = _context.MonAns.Include(m => m.LoaiMon).AsQueryable();

            // 2. Lọc theo từ khóa tìm kiếm (Tên món)
            if (!string.IsNullOrEmpty(searchString))
            {
                menuQuery = menuQuery.Where(m => m.TenMon.Contains(searchString));
            }

            // 3. Lọc theo Loại món (Dropdown)
            if (maLoai.HasValue && maLoai.Value > 0)
            {
                menuQuery = menuQuery.Where(m => m.MaLoai == maLoai);
            }

            // 4. Lấy dữ liệu để hiển thị lại trên View
            // Danh sách món ăn
            var listMenu = await menuQuery.OrderByDescending(m => m.MaMon).ToListAsync();

            // Danh sách loại món cho Dropdown lọc
            ViewBag.ListLoai = new SelectList(_context.LoaiMons, "MaLoai", "TenLoai");

            // Giữ lại giá trị filter để hiển thị trên UI
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = maLoai;

            return View(listMenu);
        }

        // ============================================================
        // 2. CREATE (THÊM MỚI)
        // ============================================================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.ListLoai = new SelectList(_context.LoaiMons, "MaLoai", "TenLoai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonAn monAn, IFormFile? file)
        {
            // Kiểm tra trùng tên
            if (await _context.MonAns.AnyAsync(m => m.TenMon == monAn.TenMon))
            {
                ModelState.AddModelError("TenMon", "Tên món ăn này đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\menu");

                    if (!Directory.Exists(productPath)) Directory.CreateDirectory(productPath);

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    monAn.HinhAnh = @"\images\menu\" + fileName;
                }

                _context.MonAns.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ListLoai = new SelectList(_context.LoaiMons, "MaLoai", "TenLoai", monAn.MaLoai);
            return View(monAn);
        }

        // ============================================================
        // 3. EDIT (CHỈNH SỬA)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null) return NotFound();

            ViewBag.ListLoai = new SelectList(_context.LoaiMons, "MaLoai", "TenLoai", monAn.MaLoai);
            return View(monAn);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonAn monAn, IFormFile? file)
        {
            if (id != monAn.MaMon) return NotFound();

            // Kiểm tra trùng tên (trừ chính nó)
            if (await _context.MonAns.AnyAsync(m => m.TenMon == monAn.TenMon && m.MaMon != id))
            {
                ModelState.AddModelError("TenMon", "Tên món ăn này đã được sử dụng!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (file != null)
                    {
                        string wwwRootPath = _webHostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = Path.Combine(wwwRootPath, @"images\menu");

                        if (!Directory.Exists(productPath)) Directory.CreateDirectory(productPath);

                        if (!string.IsNullOrEmpty(monAn.HinhAnh))
                        {
                            var oldImagePath = Path.Combine(wwwRootPath, monAn.HinhAnh.TrimStart('\\'));
                            if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        monAn.HinhAnh = @"\images\menu\" + fileName;
                    }

                    _context.Update(monAn);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MonAns.Any(e => e.MaMon == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ListLoai = new SelectList(_context.LoaiMons, "MaLoai", "TenLoai", monAn.MaLoai);
            return View(monAn);
        }

        // 4. DELETE (XÓA - AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var monAn = await _context.MonAns.FindAsync(id);
            if (monAn == null) return Json(new { success = false, message = "Không tìm thấy món ăn" });

            if (!string.IsNullOrEmpty(monAn.HinhAnh))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, monAn.HinhAnh.TrimStart('\\'));
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }

            _context.MonAns.Remove(monAn);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa thành công" });
        }
    }
}