using System.Collections.Generic;

namespace QLQA_TH.Models.ViewModels
{
    public class StaffDashboardViewModel
    {
        // Thống kê trong ngày
        public int CompletedOrdersToday { get; set; }  // Số đơn hoàn thành
        public int CancelledOrdersToday { get; set; }  // Số đơn hủy
        public int TotalDishesSoldToday { get; set; }  // Tổng số lượng món ăn bán ra

        // Dữ liệu cho biểu đồ tròn (Completed vs Cancelled)
        public int[] OrderStatusData { get; set; } // [Hoàn thành, Hủy]

        // Top 5 món bán chạy (Có thể là trong ngày hoặc toàn thời gian, ở đây ta làm toàn thời gian để list phong phú hơn)
        public List<TopSellingItem> TopSellingItems { get; set; } = new List<TopSellingItem>();
    }

    public class TopSellingItem
    {
        public string TenMon { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuongBan { get; set; }
        public int Gia { get; set; }
    }
}