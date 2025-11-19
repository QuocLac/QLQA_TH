using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLQA_TH.Models
{
    [Table("DONHANG")]
    public class DonHang
    {
        [Key]
        public int MaChiTiet { get; set; } // Khóa chính tự tăng

        // Liên kết với Phiếu Đặt
        public int MaPhieu { get; set; }

        [ForeignKey("MaPhieu")]
        public virtual PhieuDat PhieuDat { get; set; }

        public int MaMon { get; set; }

        [ForeignKey("MaMon")]
        public virtual MonAn MonAn { get; set; }

        public int SoLuong { get; set; }
        public int Gia { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int ThanhTien { get; private set; }
    }
}