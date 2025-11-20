using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;
using QLQA_TH.Models.ViewModels;
using System.Security.Claims;

namespace QLQA_TH.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<TaiKhoan> _signInManager;
        private readonly UserManager<TaiKhoan> _userManager;
        private readonly QLQAContext _context;

        public AccountController(
            SignInManager<TaiKhoan> signInManager,
            UserManager<TaiKhoan> userManager,
            QLQAContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        // ============================================================
        // 1. ĐĂNG NHẬP (LOGIN)
        // ============================================================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.ListVaiTro = GetRoleList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.ListVaiTro = GetRoleList();

            if (ModelState.IsValid)
            {
                // 1. Tìm user (bảng TaiKhoan)
                var user = await _userManager.FindByNameAsync(model.TenTK);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản không tồn tại.");
                    return View(model);
                }

                if (user.VaiTro != model.VaiTroId)
                {
                    ModelState.AddModelError(string.Empty, "Sai vai trò.");
                    return View(model);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.MatKhau, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    // --- ĐOẠN MỚI THÊM: LẤY HỌ TÊN NHÂN VIÊN ---
                    var nhanVienInfo = await _context.NhanViens
                        .FirstOrDefaultAsync(nv => nv.MaNV == user.MaNV);

                    string hoTenHienThi = nhanVienInfo?.HoTen ?? user.UserName; // Nếu lỗi thì lấy tạm UserName
                    string avatarPath = nhanVienInfo?.HinhAnh ?? "";
                    // -------------------------------------------

                    // --- CẬP NHẬT LIST CLAIMS ---
                    var claims = new List<Claim>
                    {
                        new Claim("VaiTro", user.VaiTro.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        
                        // Nạp Họ Tên thật
                        new Claim("FullName", hoTenHienThi), 
                        
                        // Nạp đường dẫn ảnh vào Claim tên là "HinhAnh"
                        // (Để khớp với code trong Layout của bạn)
                        new Claim("HinhAnh", avatarPath)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToRolePage(model.VaiTroId, returnUrl);
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản bị khóa tạm thời do đăng nhập sai quá nhiều lần.");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác.");
            }

            return View(model);
        }

        // ============================================================
        // 2. ĐĂNG XUẤT (LOGOUT)
        // ============================================================
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ============================================================
        // 3. TRANG TỪ CHỐI TRUY CẬP (ACCESS DENIED)
        // ============================================================
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ============================================================
        // 4. HỒ SƠ CÁ NHÂN (PROFILE)
        // ============================================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Giờ đây hàm này sẽ hoạt động vì Claim NameIdentifier đã có
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                // Nếu vẫn null (do cookie cũ chưa có ID), bắt đăng nhập lại
                await _signInManager.SignOutAsync();
                return RedirectToAction("Login");
            }

            // SỬA LẠI ĐOẠN NÀY: Dùng _context.Users thay vì _context.TaiKhoans
            var userInfo = await _context.Users
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(t => t.Id == user.Id);

            if (userInfo == null || userInfo.NhanVien == null)
            {
                return NotFound("Không tìm thấy thông tin nhân viên trong hệ thống.");
            }

            var model = new UserProfileViewModel
            {
                TenTK = userInfo.UserName,
                NgayTao = userInfo.NgayTao,
                VaiTroHienThi = GetRoleName(userInfo.VaiTro),

                HoTen = userInfo.NhanVien.HoTen,
                SDT = userInfo.NhanVien.SDT,
                Email = userInfo.NhanVien.Email,
                ChucVu = userInfo.NhanVien.ChucVu,
                GioiTinhHienThi = userInfo.NhanVien.GioiTinh ? "Nam" : "Nữ",
                NgaySinh = userInfo.NhanVien.NgaySinh,
                DiaChi = userInfo.NhanVien.DiaChi,

                // Thêm HinhAnh nếu view cần dùng
                HinhAnh = userInfo.NhanVien.HinhAnh
            };

            return View(model);
        }

        // ============================================================
        // 5. CÁC HÀM PHỤ TRỢ (HELPERS)
        // ============================================================

        private IActionResult RedirectToRolePage(int vaiTro, string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return vaiTro switch
            {
                0 => RedirectToAction("Index", "Home", new { area = "Admin" }),
                _ => RedirectToAction("Index", "Home", new { area = "" })
            };
        }

        private string GetRoleName(int vaiTroId)
        {
            return vaiTroId switch
            {
                0 => "Quản Lý (Admin)",
                1 => "Phục Vụ",
                2 => "Thu Ngân",
                3 => "Bếp",
                _ => "Nhân Viên"
            };
        }

        private List<SelectListItem> GetRoleList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Quản Lý (Admin)" },
                new SelectListItem { Value = "2", Text = "Thu Ngân" },
                new SelectListItem { Value = "1", Text = "Phục Vụ" },
                new SelectListItem { Value = "3", Text = "Bếp" }
            };
        }
    }
}