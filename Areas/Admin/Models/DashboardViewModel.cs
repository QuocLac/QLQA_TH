using System;
using System.Collections.Generic;

namespace QLQA_TH.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        // Thống kê tổng quan
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public int ProductCount { get; set; }     // Số món ăn
        public int UserCount { get; set; }        // Số nhân viên
        public int OrderCount { get; set; }       // Số hóa đơn đã thanh toán

        // Danh sách món bán chạy
        public List<TopProductModel> TopSellingProducts { get; set; } = new List<TopProductModel>();

        // --- DỮ LIỆU BIỂU ĐỒ (Mới thêm) ---
        public string[] ChartLabels { get; set; } // Nhãn (VD: "18/11", "19/11")
        public decimal[] ChartValues { get; set; } // Giá trị (VD: 1000000, 2500000)
    }

    public class TopProductModel
    {
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}