using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLQA_TH.Models
{
    [Table("LOAIMON")]
    public class LoaiMon
    {
        [Key]
        public int MaLoai { get; set; }

        [StringLength(100)]
        public string TenLoai { get; set; }

        // Navigation property: Một loại có nhiều món ăn
        public virtual ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
    }
}