using System.ComponentModel.DataAnnotations;

namespace QLQA_TH.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        [Display(Name = "Tên tài khoản")]
        public string TenTK { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò đăng nhập")]
        [Display(Name = "Đăng nhập với quyền")]
        public int VaiTroId { get; set; } // 0, 1, 2, 3

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}