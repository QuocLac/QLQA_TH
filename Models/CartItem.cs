namespace QLQA_TH.Models
{
    public class CartItem
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; }
        public string HinhAnh { get; set; }
        public int Gia { get; set; }
        public int SoLuong { get; set; }
        public int ThanhTien => Gia * SoLuong;
    }
}