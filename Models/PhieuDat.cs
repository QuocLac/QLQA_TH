using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLQA_TH.Models
{
    [Table("PHIEUDAT")]
    public class PhieuDat
    {
        [Key]
        public int MaPhieu { get; set; }

        public DateTime ThoiGianDat { get; set; } = DateTime.Now;

        // 0: Chờ xác nhận, 1: Đang làm, 2: Hoàn tất, 3: Hủy, 4: Đã phục vụ
        public int TrangThai { get; set; }

        public int MaHD { get; set; }
        [ForeignKey("MaHD")]
        public virtual HoaDon HoaDon { get; set; }

        // --- TRACKING NHÂN VIÊN (Minh bạch hóa quy trình) ---

        // 1. Người tạo phiếu (Phục vụ/Thu ngân)
        public int? MaNV_Dat { get; set; }
        [ForeignKey("MaNV_Dat")]
        public virtual NhanVien? NhanVienDat { get; set; }

        // 2. Người chế biến (Đầu bếp)
        public int? MaNV_Bep { get; set; }
        [ForeignKey("MaNV_Bep")]
        public virtual NhanVien? NhanVienBep { get; set; }

        // 3. Người bưng món (Phục vụ/Runner)
        public int? MaNV_PhucVu { get; set; }
        [ForeignKey("MaNV_PhucVu")]
        public virtual NhanVien? NhanVienPhucVu { get; set; }

        public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    }
}