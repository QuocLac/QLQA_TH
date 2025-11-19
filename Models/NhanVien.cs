using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLQA_TH.Models
{
    [Table("NHANVIEN")]
    public class NhanVien
    {
        [Key]
        public int MaNV { get; set; }

        [StringLength(50)]
        public string HoTen { get; set; } = null!;

        [StringLength(15)]
        public string? SDT { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? ChucVu { get; set; }

        public bool GioiTinh { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(100)]
        public string? DiaChi { get; set; }

        // --- THUỘC TÍNH MỚI ---
        [StringLength(255)]
        [Display(Name = "Hình ảnh")]
        public string? HinhAnh { get; set; } // Đường dẫn ảnh (VD: /images/nhanvien/nv1.jpg)

        public bool TrangThai { get; set; }

        // Navigation Properties
        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }
}