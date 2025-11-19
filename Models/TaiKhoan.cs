using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLQA_TH.Models
{
    [Table("TAIKHOAN")]
    // Kế thừa IdentityUser<int> để dùng Id kiểu int
    public class TaiKhoan : IdentityUser<int>
    {
        // --- CÁC THUỘC TÍNH CỦA IDENTITY (Đã có sẵn trong class cha, KHÔNG khai báo lại) ---
        // public int Id { get; set; }         <-- Thay thế cho Id_TK
        // public string UserName { get; set; } <-- Thay thế cho TenTK
        // public string PasswordHash { get; set; } <-- Thay thế cho MatKhauHash
        // public string Email { get; set; }    <-- Thay thế cho Email

        // --- CÁC THUỘC TÍNH RIÊNG CỦA BẠN ---

        // 0: Quản lý, 1: Phục vụ, 2: Thu ngân, 3: Bếp
        public int VaiTro { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        // Khóa ngoại trỏ về NhanVien
        public int MaNV { get; set; }

        [ForeignKey("MaNV")]
        public virtual NhanVien NhanVien { get; set; } = null!;
    }
}