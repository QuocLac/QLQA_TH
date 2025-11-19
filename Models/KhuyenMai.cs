using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QLQA_TH.Models
{
    [Table("KHUYENMAI")]
    [Index(nameof(MaVoucher), IsUnique = true)] // Ràng buộc Unique DB
    public class KhuyenMai
    {
        [Key]
        public int MaKM { get; set; }

        [Required(ErrorMessage = "Tên chương trình không được để trống")]
        [StringLength(100)]
        public string TenKM { get; set; } = null!;

        [Required(ErrorMessage = "Mã Voucher là bắt buộc")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Mã phải từ 3-20 ký tự")]
        public string MaVoucher { get; set; } = null!;

        [Range(0, 100, ErrorMessage = "% Giảm phải từ 0 đến 100")]
        public int? PhanTramGiam { get; set; }

        [Range(0, 100000000, ErrorMessage = "Số tiền giảm không hợp lệ")]
        public int? SoTienGiam { get; set; }

        [Range(0, 100000000, ErrorMessage = "Điều kiện áp dụng không hợp lệ")]
        public int? DieuKienApDung { get; set; } // Hóa đơn tối thiểu

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime? NgayBatDau { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime? NgayKetThuc { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, 10000, ErrorMessage = "Số lượng ít nhất là 1")]
        public int? SoLuong { get; set; }

        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }
}