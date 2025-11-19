using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;
using QLQA_TH.Models.ViewModels;
using System.Diagnostics;

namespace QLQA_TH.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QLQAContext _context;

        public HomeController(ILogger<HomeController> logger, QLQAContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now.Date;

            var ordersToday = await _context.HoaDons
                .Where(h => h.ThoiGianVao >= today)
                .ToListAsync();

            // SỬA: Đếm đơn hủy là trạng thái 3
            int completedCount = ordersToday.Count(h => h.TrangThaiHoaDon == 1);
            int cancelledCount = ordersToday.Count(h => h.TrangThaiHoaDon == 3);

            int totalDishes = await _context.DonHangs
                .Include(d => d.PhieuDat).ThenInclude(p => p.HoaDon)
                .Where(d => d.PhieuDat.HoaDon.ThoiGianVao >= today
                            && d.PhieuDat.HoaDon.TrangThaiHoaDon == 1)
                .SumAsync(d => d.SoLuong);

            var topItems = await _context.DonHangs
                .Include(d => d.PhieuDat).ThenInclude(p => p.HoaDon)
                .Where(d => d.PhieuDat.HoaDon.TrangThaiHoaDon == 1)
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
                    (stat, mon) => new TopSellingItem
                    {
                        TenMon = mon.TenMon,
                        HinhAnh = mon.HinhAnh,
                        Gia = mon.Gia,
                        SoLuongBan = stat.TotalSold
                    })
                .ToListAsync();

            var model = new StaffDashboardViewModel
            {
                CompletedOrdersToday = completedCount,
                CancelledOrdersToday = cancelledCount,
                TotalDishesSoldToday = totalDishes,
                OrderStatusData = new int[] { completedCount, cancelledCount },
                TopSellingItems = topItems
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}