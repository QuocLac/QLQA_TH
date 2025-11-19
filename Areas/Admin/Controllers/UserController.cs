using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Areas.Admin.Models;
using QLQA_TH.Data;
using QLQA_TH.Models;

namespace QLQA_TH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdminRole")]
    public class UserController : Controller
    {
        private readonly QLQAContext _context;
        private readonly UserManager<TaiKhoan> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(QLQAContext context, UserManager<TaiKhoan> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. INDEX (Giữ nguyên)
        public async Task<IActionResult> Index(string searchString)
        {
            var query = _context.NhanViens.Include(n => n.TaiKhoan).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n => n.HoTen.Contains(searchString) || n.SDT.Contains(searchString));
            }
            var listNhanVien = await query.OrderByDescending(n => n.MaNV).ToListAsync();
            ViewData["CurrentFilter"] = searchString;
            return View(listNhanVien);
        }

        // 2. UPSERT (GET) - CHUẨN BỊ DỮ LIỆU
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            UserUpsertViewModel model = new UserUpsertViewModel();

            // Danh sách quyền cho Dropdown
            ViewBag.ListRole = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Quản Lý (Admin)" },
                new SelectListItem { Value = "2", Text = "Thu Ngân" },
                new SelectListItem { Value = "1", Text = "Phục Vụ" },
                new SelectListItem { Value = "3", Text = "Bếp" }
            };

            if (id == null || id == 0)
            {
                // THÊM MỚI NHÂN VIÊN
                model.TrangThaiNV = true; // Mặc định đang làm
                model.NgaySinh = new DateTime(2000, 1, 1);
                model.HasAccount = false;
            }
            else
            {
                // SỬA NHÂN VIÊN
                var nv = await _context.NhanViens.Include(n => n.TaiKhoan).FirstOrDefaultAsync(n => n.MaNV == id);
                if (nv == null) return NotFound();

                // Map dữ liệu Nhân viên sang ViewModel
                model.MaNV = nv.MaNV;
                model.HoTen = nv.HoTen;
                model.SDT = nv.SDT;
                model.Email = nv.Email;
                model.ChucVu = nv.ChucVu;
                model.GioiTinh = nv.GioiTinh;
                model.NgaySinh = nv.NgaySinh;
                model.DiaChi = nv.DiaChi;
                model.HinhAnh = nv.HinhAnh;
                model.TrangThaiNV = nv.TrangThai;

                // Map dữ liệu Tài khoản (Nếu có)
                if (nv.TaiKhoan != null)
                {
                    model.HasAccount = true;
                    model.Id_TK = nv.TaiKhoan.Id;
                    model.TenTK = nv.TaiKhoan.UserName;
                    model.VaiTro = nv.TaiKhoan.VaiTro;
                    model.TrangThaiTK = nv.TaiKhoan.TrangThai;
                }
                else
                {
                    model.HasAccount = false;
                }
            }
            return View(model);
        }

        // 3. UPSERT (POST) - XỬ LÝ CẬP NHẬT
        // 3. UPSERT (POST) - XỬ LÝ CẬP NHẬT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(UserUpsertViewModel model, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                // A. XỬ LÝ NHÂN VIÊN
                NhanVien nv;
                if (model.MaNV == 0)
                {
                    nv = new NhanVien();
                    _context.NhanViens.Add(nv);
                }
                else
                {
                    nv = await _context.NhanViens.FindAsync(model.MaNV);
                    if (nv == null) return NotFound();
                }

                nv.HoTen = model.HoTen;
                nv.SDT = model.SDT;
                nv.Email = model.Email;
                nv.ChucVu = model.ChucVu;
                nv.GioiTinh = model.GioiTinh;
                nv.NgaySinh = model.NgaySinh;
                nv.DiaChi = model.DiaChi;
                nv.TrangThai = model.TrangThaiNV;

                // Xử lý ảnh
                if (file != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string path = Path.Combine(wwwRootPath, @"images\users");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                    if (!string.IsNullOrEmpty(nv.HinhAnh))
                    {
                        var oldPath = Path.Combine(wwwRootPath, nv.HinhAnh.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    nv.HinhAnh = "/images/users/" + fileName;
                }

                // B. XỬ LÝ TÀI KHOẢN (Chỉ khi đang sửa và có tài khoản)
                if (model.HasAccount && model.Id_TK.HasValue)
                {
                    var user = await _userManager.FindByIdAsync(model.Id_TK.Value.ToString());
                    if (user != null)
                    {
                        // 1. Cập nhật Quyền
                        if (model.VaiTro.HasValue) user.VaiTro = model.VaiTro.Value;

                        // 2. Cập nhật Trạng thái (ĐÃ SỬA: Bỏ check HasValue)
                        user.TrangThai = model.TrangThaiTK;

                        if (user.TrangThai == false)
                        {
                            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                        }
                        else
                        {
                            await _userManager.SetLockoutEndDateAsync(user, null);
                        }

                        // 3. Đặt lại mật khẩu
                        if (!string.IsNullOrEmpty(model.NewPassword))
                        {
                            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                            await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                        }

                        await _userManager.UpdateAsync(user);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload danh sách quyền nếu lỗi
            ViewBag.ListRole = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Quản Lý (Admin)" },
                new SelectListItem { Value = "2", Text = "Thu Ngân" },
                new SelectListItem { Value = "1", Text = "Phục Vụ" },
                new SelectListItem { Value = "3", Text = "Bếp" }
            };
            return View(model);
        }

        // ... (Giữ nguyên hàm CreateAccount và Delete) ...
        // Bạn copy lại hàm CreateAccount và Delete từ phiên bản trước vào đây nhé
        // Hoặc nếu cần tôi sẽ paste lại cho đầy đủ
        [HttpGet]
        public async Task<IActionResult> CreateAccount(int maNV)
        {
            var nv = await _context.NhanViens.FindAsync(maNV);
            if (nv == null) return NotFound();

            var tk = await _context.Users.FirstOrDefaultAsync(u => u.MaNV == maNV);
            if (tk != null) return RedirectToAction(nameof(Index));

            var model = new CreateAccountViewModel
            {
                MaNV = maNV,
                HoTenNhanVien = nv.HoTen,
                TenTK = nv.SDT ?? "",
            };

            ViewBag.ListRole = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Quản Lý (Admin)" },
                new SelectListItem { Value = "2", Text = "Thu Ngân" },
                new SelectListItem { Value = "1", Text = "Phục Vụ" },
                new SelectListItem { Value = "3", Text = "Bếp" }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newUser = new TaiKhoan
                {
                    UserName = model.TenTK,
                    VaiTro = model.VaiTro,
                    MaNV = model.MaNV,
                    TrangThai = true,
                    NgayTao = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, model.MatKhau);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.ListRole = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Quản Lý (Admin)" },
                new SelectListItem { Value = "2", Text = "Thu Ngân" },
                new SelectListItem { Value = "1", Text = "Phục Vụ" },
                new SelectListItem { Value = "3", Text = "Bếp" }
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null) return Json(new { success = false, message = "Không tìm thấy nhân viên" });

            if (!string.IsNullOrEmpty(nv.HinhAnh))
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, nv.HinhAnh.TrimStart('/'));
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _context.NhanViens.Remove(nv);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Xóa thành công" });
        }
    }
}