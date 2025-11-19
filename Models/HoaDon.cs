using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLQA_TH.Models
{
    [Table("HOADON")]
    public class HoaDon
    {
        [Key]
        public int MaHD { get; set; }

        public DateTime ThoiGianVao { get; set; }
        public DateTime? ThoiGianRa { get; set; }

        // 0: Chưa thanh toán, 1: Đã thanh toán
        public int TrangThaiHoaDon { get; set; }

        public int TongTien { get; set; }
        public int GiamGia { get; set; }
        public int ThanhTien { get; set; }

        [StringLength(50)]
        public string? PhuongThucThanhToan { get; set; }

        public int? MaNV { get; set; }
        [ForeignKey("MaNV")]
        public virtual NhanVien? NhanVien { get; set; }

        public int? MaBan { get; set; }
        [ForeignKey("MaBan")]
        public virtual Ban? Ban { get; set; }

        public int? MaKM { get; set; }
        [ForeignKey("MaKM")]
        public virtual KhuyenMai? KhuyenMai { get; set; }

        // Danh sách các phiếu đặt (lượt gọi món) của hóa đơn này
        public virtual ICollection<PhieuDat> PhieuDats { get; set; } = new List<PhieuDat>();
    }
}