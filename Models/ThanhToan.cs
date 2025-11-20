using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLQA_TH.Models
{
    [Table("THANHTOAN")]
    public class ThanhToan
    {
        [Key]
        public int MaGiaoDich { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.Now;

        // Số tiền thanh toán trong lần này
        public int SoTien { get; set; }

        // Loại: "Tiền mặt", "VNPay", "MoMo", "Chuyển khoản"
        [StringLength(50)]
        public string PhuongThuc { get; set; }

        // Mã giao dịch từ cổng thanh toán trả về (để đối soát)
        [StringLength(100)]
        public string? MaGiaoDichCong { get; set; }

        // Trạng thái: 0: Đang xử lý, 1: Thành công, 2: Thất bại
        public int TrangThai { get; set; }

        // Ghi chú (VD: "Khách đưa 500k trả lại 20k", hoặc Lỗi từ VNPay)
        [StringLength(255)]
        public string? GhiChu { get; set; }

        // Liên kết Hóa đơn
        public int MaHD { get; set; }
        [ForeignKey("MaHD")]
        public virtual HoaDon HoaDon { get; set; }
    }
}