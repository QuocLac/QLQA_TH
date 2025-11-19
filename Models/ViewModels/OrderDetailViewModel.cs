using System.Collections.Generic;
using System.Linq;

namespace QLQA_TH.Models.ViewModels
{
    public class OrderDetailViewModel
    {
        public Ban Ban { get; set; }

        // THÊM LẠI THUỘC TÍNH NÀY
        public HoaDon? HoaDon { get; set; }

        public List<CartItem> CurrentCart { get; set; } = new List<CartItem>();
        public int CartTotal => CurrentCart.Sum(x => x.ThanhTien);

        public List<MonAn> MenuItems { get; set; }
        public List<LoaiMon> Categories { get; set; }

        public string CurrentSearch { get; set; }
        public int? CurrentCategory { get; set; }
    }
}