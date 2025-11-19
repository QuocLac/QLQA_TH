using System;
using System.ComponentModel.DataAnnotations;

namespace QLQA_TH.Models.ViewModels
{
    public class UserProfileViewModel
    {
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SDT { get; set; }

        [Display(Name = "Email liên hệ")]
        public string? Email { get; set; }

        [Display(Name = "Chức vụ")]
        public string? ChucVu { get; set; }

        [Display(Name = "Giới tính")]
        public string GioiTinhHienThi { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Tên tài khoản")]
        public string TenTK { get; set; }

        [Display(Name = "Vai trò hệ thống")]
        public string VaiTroHienThi { get; set; }

        [Display(Name = "Ngày tham gia")]
        public DateTime NgayTao { get; set; }

        // --- SỬ DỤNG HINHANH ĐỂ KHỚP VỚI MODEL ---
        [Display(Name = "Ảnh đại diện")]
        public string? HinhAnh { get; set; }
    }
}