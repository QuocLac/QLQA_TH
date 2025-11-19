using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Areas.Admin.Models;
using QLQA_TH.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLQA_TH.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "RequireAdminRole")]
    public class HomeController : Controller
    {
        private readonly QLQAContext _context;

        public HomeController(QLQAContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. THỐNG KÊ TỔNG QUAN (Dựa trên bảng HoaDon - Không đổi)
            // Chỉ tính hóa đơn ĐÃ THANH TOÁN (TrangThaiHoaDon = 1)
            var totalRevenue = await _context.HoaDons
                .Where(h => h.TrangThaiHoaDon == 1)
                .SumAsync(h => h.ThanhTien);

            var productCount = await _context.MonAns.CountAsync();
            var userCount = await _context.NhanViens.CountAsync();
            var orderCount = await _context.HoaDons
                .Where(h => h.TrangThaiHoaDon == 1)
                .CountAsync();

            // 2. TOP 5 SẢN PHẨM BÁN CHẠY (CẬP NHẬT LOGIC MỚI)
            // DonHang -> PhieuDat -> HoaDon
            var topProducts = await _context.DonHangs
                .Include(d => d.PhieuDat).ThenInclude(p => p.HoaDon)
                .Where(d => d.PhieuDat.HoaDon.TrangThaiHoaDon == 1) // Chỉ tính đơn đã thanh toán
                .GroupBy(d => d.MaMon)
                .Select(g => new
                {
                    MaMon = g.Key,
                    TotalSold = g.Sum(x => x.SoLuong)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .Join(_context.MonAns,
                    stat => stat.MaMon,
                    mon => mon.MaMon,
                    (stat, mon) => new TopProductModel
                    {
                        ProductName = mon.TenMon,
                        QuantitySold = stat.TotalSold,
                        Price = mon.Gia,
                        Stock = mon.SoLuong,
                        ImageUrl = mon.HinhAnh
                    })
                .ToListAsync();

            // 3. DỮ LIỆU BIỂU ĐỒ (Dựa trên bảng HoaDon - Không đổi)
            var days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Now.Date.AddDays(-6 + i))
                .ToList();

            var revenueData = await _context.HoaDons
                .Where(h => h.TrangThaiHoaDon == 1 && h.ThoiGianVao >= days[0])
                .GroupBy(h => h.ThoiGianVao.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.ThanhTien) })
                .ToListAsync();

            var chartLabels = new List<string>();
            var chartValues = new List<decimal>();

            foreach (var day in days)
            {
                chartLabels.Add(day.ToString("dd/MM"));
                var record = revenueData.FirstOrDefault(r => r.Date == day);
                chartValues.Add(record != null ? record.Revenue : 0);
            }

            var model = new DashboardViewModel
            {
                TotalRevenue = totalRevenue,
                ProductCount = productCount,
                UserCount = userCount,
                OrderCount = orderCount,
                TopSellingProducts = topProducts,
                ChartLabels = chartLabels.ToArray(),
                ChartValues = chartValues.ToArray()
            };

            return View(model);
        }
    }
}