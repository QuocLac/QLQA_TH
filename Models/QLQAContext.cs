using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLQA_TH.Models;

namespace QLQA_TH.Data
{
    public class QLQAContext : IdentityDbContext<TaiKhoan, IdentityRole<int>, int>
    {
        public QLQAContext(DbContextOptions<QLQAContext> options) : base(options) { }

        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<LoaiMon> LoaiMons { get; set; }
        public DbSet<MonAn> MonAns { get; set; }
        public DbSet<Ban> Bans { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<PhieuDat> PhieuDats { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<DatBan> DatBans { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- MAPPING IDENTITY ---
            modelBuilder.Entity<TaiKhoan>(entity => {
                entity.ToTable("TAIKHOAN");
                entity.Property(e => e.Id).HasColumnName("Id_TK");
                entity.Property(e => e.UserName).HasColumnName("TenTK");
                entity.Property(e => e.PasswordHash).HasColumnName("MatKhauHash");
            });

            // --- QUAN HỆ CŨ ---
            modelBuilder.Entity<NhanVien>()
                .HasOne(nv => nv.TaiKhoan).WithOne(tk => tk.NhanVien)
                .HasForeignKey<TaiKhoan>(tk => tk.MaNV).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TaiKhoan>().HasIndex(tk => tk.MaNV).IsUnique();

            // --- CẤU HÌNH QUAN HỆ PHIẾU ĐẶT & TRACKING NHÂN VIÊN ---
            // 1. Người đặt (Không xóa phiếu khi xóa nhân viên -> Restrict)
            modelBuilder.Entity<PhieuDat>()
                .HasOne(p => p.NhanVienDat)
                .WithMany()
                .HasForeignKey(p => p.MaNV_Dat)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Người bếp
            modelBuilder.Entity<PhieuDat>()
                .HasOne(p => p.NhanVienBep)
                .WithMany()
                .HasForeignKey(p => p.MaNV_Bep)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Người phục vụ
            modelBuilder.Entity<PhieuDat>()
                .HasOne(p => p.NhanVienPhucVu)
                .WithMany()
                .HasForeignKey(p => p.MaNV_PhucVu)
                .OnDelete(DeleteBehavior.Restrict);

            // --- CẤU HÌNH CÁC BẢNG KHÁC ---
            modelBuilder.Entity<DonHang>().HasKey(dh => dh.MaChiTiet);

            modelBuilder.Entity<PhieuDat>()
                .HasOne(p => p.HoaDon).WithMany(h => h.PhieuDats)
                .HasForeignKey(p => p.MaHD).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.PhieuDat).WithMany(p => p.DonHangs)
                .HasForeignKey(d => d.MaPhieu).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MonAn>().Property(m => m.TrangThai)
                .HasComputedColumnSql("CASE WHEN SoLuong > 0 THEN N'Còn hàng' ELSE N'Hết hàng' END");
            modelBuilder.Entity<DonHang>().Property(dh => dh.ThanhTien)
                .HasComputedColumnSql("[SoLuong] * [Gia]");
        }
    }
}