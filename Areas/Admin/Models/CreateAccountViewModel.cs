using System.ComponentModel.DataAnnotations;

namespace QLQA_TH.Areas.Admin.Models
{
    public class CreateAccountViewModel
    {
        public int MaNV { get; set; } // Khóa ngoại để biết tạo cho ai

        public string HoTenNhanVien { get; set; } // Để hiển thị tên cho chắc chắn

        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        [Display(Name = "Tên đăng nhập")]
        public string TenTK { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public int VaiTro { get; set; } // 0: Admin, 1: PV, 2: TN, 3: Bếp
    }
}