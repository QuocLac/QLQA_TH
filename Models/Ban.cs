using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QLQA_TH.Models
{
    [Table("BAN")]
    [Index(nameof(TenBan), IsUnique = true)] // Ràng buộc: Tên bàn không được trùng
    public class Ban
    {
        [Key]
        public int MaBan { get; set; }

        [Required(ErrorMessage = "Tên bàn không được để trống")]
        [StringLength(50, ErrorMessage = "Tên bàn không được quá 50 ký tự")]
        public string TenBan { get; set; } = null!; // Không được null

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public int TrangThai { get; set; }

        [Required(ErrorMessage = "Số ghế là bắt buộc")]
        [Range(1, 100, ErrorMessage = "Số ghế phải từ 1 đến 100")] // Validate số ghế hợp lý
        public int SoGhe { get; set; } // Chuyển từ int? sang int (Không được null)

        // Các thuộc tính quan hệ (Navigation Properties) giữ nguyên
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
        public virtual ICollection<DatBan> DatBans { get; set; } = new List<DatBan>();
    }
}