using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Models;

namespace QLQA_TH.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            // Lấy các dịch vụ cần thiết từ DI container
            var userManager = serviceProvider.GetRequiredService<UserManager<TaiKhoan>>();
            var context = serviceProvider.GetRequiredService<QLQAContext>();

            // Đảm bảo database đã được tạo
            await context.Database.EnsureCreatedAsync();

            // 1. Kiểm tra xem tài khoản Admin đã tồn tại chưa
            var adminUser = await userManager.FindByNameAsync("admin");

            if (adminUser == null)
            {
                // 2. Nếu chưa có, PHẢI tạo Nhân viên trước (vì ràng buộc khóa ngoại MaNV)
                var adminNhanVien = new NhanVien
                {
                    HoTen = "Quản Trị Viên Hệ Thống",
                    SDT = "0999999999", // SĐT giả định cho admin
                    Email = "admin@system.com",
                    ChucVu = "Quản lý",
                    GioiTinh = true, // Nam
                    NgaySinh = new DateTime(2000, 1, 1),
                    DiaChi = "Trụ sở chính",
                    TrangThai = true
                };

                // Kiểm tra xem nhân viên này đã có chưa để tránh trùng SDT (Unique Index)
                var existingNhanVien = await context.NhanViens
                    .FirstOrDefaultAsync(nv => nv.SDT == "0999999999");

                if (existingNhanVien == null)
                {
                    context.NhanViens.Add(adminNhanVien);
                    await context.SaveChangesAsync(); // Lưu để lấy MaNV
                }
                else
                {
                    adminNhanVien = existingNhanVien;
                }

                // 3. Tạo Tài khoản Admin gắn với Nhân viên vừa tạo
                var newAdmin = new TaiKhoan
                {
                    UserName = "admin",
                    Email = "admin@system.com",
                    VaiTro = 0, // 0: Quản lý (Admin)
                    TrangThai = true,
                    NgayTao = DateTime.Now,
                    MaNV = adminNhanVien.MaNV, // Gắn khóa ngoại
                    EmailConfirmed = true
                };

                // 4. Tạo User và Băm mật khẩu
                var result = await userManager.CreateAsync(newAdmin, "Admin123@");

                if (!result.Succeeded)
                {
                    // Nếu lỗi (ví dụ mật khẩu không đủ mạnh), in ra console để debug
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Lỗi tạo Admin: {error.Description}");
                    }
                }
                else
                {
                    Console.WriteLine("Đã tạo tài khoản Admin thành công!");
                }
            }
        }
    }
}