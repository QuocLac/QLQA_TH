using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLQA_TH.Models
{
    [Table("DATBAN")]
    public class DatBan
    {
        [Key]
        public int MaDatBan { get; set; }

        [StringLength(50)]
        public string? TenKhachDat { get; set; }

        [StringLength(15)]
        public string? SDTKhachDat { get; set; }

        public int? SoNguoi { get; set; }

        public DateTime ThoiGianDat { get; set; }

        [StringLength(200)]
        public string? GhiChu { get; set; }

        public int TrangThaiDat { get; set; }

        public int? MaBan { get; set; }
        [ForeignKey("MaBan")]
        public virtual Ban? Ban { get; set; }
    }
}