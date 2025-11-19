using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Data;
using QLQA_TH.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddControllersWithViews();

// 2. Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<QLQAContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Cấu hình Identity
builder.Services.AddIdentity<TaiKhoan, IdentityRole<int>>(options =>
{
    // Cấu hình Password
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredUniqueChars = 1;

    // Cấu hình Lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<QLQAContext>()
.AddDefaultTokenProviders();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Giỏ hàng tồn tại 30 phút nếu không thao tác
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. Cấu hình Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(12);
});

// 5. Cấu hình Phân Quyền
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireClaim("VaiTro", "0"));
    options.AddPolicy("RequirePhucVuRole", policy => policy.RequireClaim("VaiTro", "1"));
    options.AddPolicy("RequireThuNganRole", policy => policy.RequireClaim("VaiTro", "2"));
    options.AddPolicy("RequireBepRole", policy => policy.RequireClaim("VaiTro", "3"));

    // 1. Nhóm "Phục Vụ" (Đặt món, Xem món): 
    // Cho phép: Phục vụ (1), Thu ngân (2), Admin (0)
    options.AddPolicy("PhucVuAccess", policy => policy.RequireClaim("VaiTro", "1", "2", "0"));

    // 2. Nhóm "Thu Ngân" (Hóa đơn, Thanh toán): 
    // Cho phép: Thu ngân (2), Admin (0)
    options.AddPolicy("ThuNganAccess", policy => policy.RequireClaim("VaiTro", "2", "0"));

    // 3. Nhóm "Bếp" (Chế biến): 
    // Cho phép: Bếp (3), Admin (0)
    options.AddPolicy("BepAccess", policy => policy.RequireClaim("VaiTro", "3", "0"));
});

var app = builder.Build();

// --- BẮT ĐẦU: KHỞI TẠO DỮ LIỆU (SEED DATA) ---
// Đoạn code này sẽ chạy mỗi khi ứng dụng khởi động
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Gọi hàm SeedData từ class DbInitializer
        await DbInitializer.SeedData(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi khi khởi tạo dữ liệu Admin.");
    }
}
// --- KẾT THÚC KHỞI TẠO DỮ LIỆU ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();