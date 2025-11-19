using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QLQA_TH.Models
{
    [Table("MONAN")]
    [Index(nameof(TenMon), IsUnique = true)] // Ràng buộc: Tên món là duy nhất
    public class MonAn
    {
        [Key]
        public int MaMon { get; set; }

        [Required(ErrorMessage = "Tên món không được để trống")]
        [StringLength(100, ErrorMessage = "Tên món không được quá 100 ký tự")]
        public string TenMon { get; set; } = null!;

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, 10000, ErrorMessage = "Số lượng phải từ 0 đến 10.000")]
        public int SoLuong { get; set; }

        [StringLength(500)]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Giá bán là bắt buộc")]
        [Range(1000, 10000000, ErrorMessage = "Giá bán phải từ 1.000đ trở lên")]
        public int Gia { get; set; }

        [StringLength(255)]
        public string? HinhAnh { get; set; }

        // Cột tính toán (Computed Column) - Logic database tự xử lý
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? TrangThai { get; private set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục món")]
        public int? MaLoai { get; set; }

        [ForeignKey("MaLoai")]
        public virtual LoaiMon? LoaiMon { get; set; }

        public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    }
}