using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;
using QLQA_TH.Models.ViewModels;

namespace QLQA_TH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdminRole")] // Chỉ Admin mới vào được đây
    public class ProfileController : Controller
    {
        private readonly UserManager<TaiKhoan> _userManager;
        private readonly QLQAContext _context;

        public ProfileController(UserManager<TaiKhoan> userManager, QLQAContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Admin/Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            // Truy vấn thông tin Admin
            var userInfo = await _context.Users
                .Include(t => t.NhanVien)
                .FirstOrDefaultAsync(t => t.Id == user.Id);

            if (userInfo == null || userInfo.NhanVien == null)
            {
                return NotFound("Không tìm thấy thông tin quản trị viên.");
            }

            var model = new UserProfileViewModel
            {
                TenTK = userInfo.UserName,
                NgayTao = userInfo.NgayTao,
                VaiTroHienThi = "Quản Trị Viên", // Cố định vì đây là Area Admin

                HoTen = userInfo.NhanVien.HoTen,
                SDT = userInfo.NhanVien.SDT,
                Email = userInfo.NhanVien.Email,
                ChucVu = userInfo.NhanVien.ChucVu,
                GioiTinhHienThi = userInfo.NhanVien.GioiTinh ? "Nam" : "Nữ",
                NgaySinh = userInfo.NhanVien.NgaySinh,
                DiaChi = userInfo.NhanVien.DiaChi
            };

            return View(model);
        }
    }
}