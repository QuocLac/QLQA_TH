using System.ComponentModel.DataAnnotations;

namespace QLQA_TH.Areas.Admin.Models
{
    public class UserUpsertViewModel
    {
        // --- PHẦN 1: THÔNG TIN NHÂN VIÊN ---
        public int MaNV { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SDT { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Chức vụ")]
        public string? ChucVu { get; set; }

        public bool GioiTinh { get; set; } = true;

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        public string? DiaChi { get; set; }
        public string? HinhAnh { get; set; }

        [Display(Name = "Trạng thái làm việc")]
        public bool TrangThaiNV { get; set; } = true;


        // --- PHẦN 2: THÔNG TIN TÀI KHOẢN (Nếu có) ---
        public bool HasAccount { get; set; }

        public int? Id_TK { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string? TenTK { get; set; }

        [Display(Name = "Phân quyền")]
        public int? VaiTro { get; set; }

        // --- ĐÃ SỬA: Đổi từ bool? sang bool ---
        [Display(Name = "Trạng thái tài khoản")]
        public bool TrangThaiTK { get; set; } = true; // Mặc định True (Hoạt động)

        [Display(Name = "Đặt lại mật khẩu (Bỏ trống nếu không đổi)")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
        public string? NewPassword { get; set; }
    }
}