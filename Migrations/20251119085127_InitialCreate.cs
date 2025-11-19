using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLQA_TH.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BAN",
                columns: table => new
                {
                    MaBan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenBan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    SoGhe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAN", x => x.MaBan);
                });

            migrationBuilder.CreateTable(
                name: "KHUYENMAI",
                columns: table => new
                {
                    MaKM = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKM = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaVoucher = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhanTramGiam = table.Column<int>(type: "int", nullable: true),
                    SoTienGiam = table.Column<int>(type: "int", nullable: true),
                    DieuKienApDung = table.Column<int>(type: "int", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHUYENMAI", x => x.MaKM);
                });

            migrationBuilder.CreateTable(
                name: "LOAIMON",
                columns: table => new
                {
                    MaLoai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOAIMON", x => x.MaLoai);
                });

            migrationBuilder.CreateTable(
                name: "NHANVIEN",
                columns: table => new
                {
                    MaNV = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SDT = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GioiTinh = table.Column<bool>(type: "bit", nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHANVIEN", x => x.MaNV);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DATBAN",
                columns: table => new
                {
                    MaDatBan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhachDat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SDTKhachDat = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SoNguoi = table.Column<int>(type: "int", nullable: true),
                    ThoiGianDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThaiDat = table.Column<int>(type: "int", nullable: false),
                    MaBan = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DATBAN", x => x.MaDatBan);
                    table.ForeignKey(
                        name: "FK_DATBAN_BAN_MaBan",
                        column: x => x.MaBan,
                        principalTable: "BAN",
                        principalColumn: "MaBan");
                });

            migrationBuilder.CreateTable(
                name: "MONAN",
                columns: table => new
                {
                    MaMon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Gia = table.Column<int>(type: "int", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true, computedColumnSql: "CASE WHEN SoLuong > 0 THEN N'Còn hàng' ELSE N'Hết hàng' END"),
                    MaLoai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MONAN", x => x.MaMon);
                    table.ForeignKey(
                        name: "FK_MONAN_LOAIMON_MaLoai",
                        column: x => x.MaLoai,
                        principalTable: "LOAIMON",
                        principalColumn: "MaLoai",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HOADON",
                columns: table => new
                {
                    MaHD = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThoiGianVao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianRa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThaiHoaDon = table.Column<int>(type: "int", nullable: false),
                    TongTien = table.Column<int>(type: "int", nullable: false),
                    GiamGia = table.Column<int>(type: "int", nullable: false),
                    ThanhTien = table.Column<int>(type: "int", nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaNV = table.Column<int>(type: "int", nullable: true),
                    MaBan = table.Column<int>(type: "int", nullable: true),
                    MaKM = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HOADON", x => x.MaHD);
                    table.ForeignKey(
                        name: "FK_HOADON_BAN_MaBan",
                        column: x => x.MaBan,
                        principalTable: "BAN",
                        principalColumn: "MaBan");
                    table.ForeignKey(
                        name: "FK_HOADON_KHUYENMAI_MaKM",
                        column: x => x.MaKM,
                        principalTable: "KHUYENMAI",
                        principalColumn: "MaKM");
                    table.ForeignKey(
                        name: "FK_HOADON_NHANVIEN_MaNV",
                        column: x => x.MaNV,
                        principalTable: "NHANVIEN",
                        principalColumn: "MaNV");
                });

            migrationBuilder.CreateTable(
                name: "TAIKHOAN",
                columns: table => new
                {
                    Id_TK = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VaiTro = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaNV = table.Column<int>(type: "int", nullable: false),
                    TenTK = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAIKHOAN", x => x.Id_TK);
                    table.ForeignKey(
                        name: "FK_TAIKHOAN_NHANVIEN_MaNV",
                        column: x => x.MaNV,
                        principalTable: "NHANVIEN",
                        principalColumn: "MaNV",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PHIEUDAT",
                columns: table => new
                {
                    MaPhieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThoiGianDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    MaHD = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHIEUDAT", x => x.MaPhieu);
                    table.ForeignKey(
                        name: "FK_PHIEUDAT_HOADON_MaHD",
                        column: x => x.MaHD,
                        principalTable: "HOADON",
                        principalColumn: "MaHD",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_TAIKHOAN_UserId",
                        column: x => x.UserId,
                        principalTable: "TAIKHOAN",
                        principalColumn: "Id_TK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_TAIKHOAN_UserId",
                        column: x => x.UserId,
                        principalTable: "TAIKHOAN",
                        principalColumn: "Id_TK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_TAIKHOAN_UserId",
                        column: x => x.UserId,
                        principalTable: "TAIKHOAN",
                        principalColumn: "Id_TK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_TAIKHOAN_UserId",
                        column: x => x.UserId,
                        principalTable: "TAIKHOAN",
                        principalColumn: "Id_TK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DONHANG",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieu = table.Column<int>(type: "int", nullable: false),
                    MaMon = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    Gia = table.Column<int>(type: "int", nullable: false),
                    ThanhTien = table.Column<int>(type: "int", nullable: false, computedColumnSql: "[SoLuong] * [Gia]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DONHANG", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_DONHANG_MONAN_MaMon",
                        column: x => x.MaMon,
                        principalTable: "MONAN",
                        principalColumn: "MaMon",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DONHANG_PHIEUDAT_MaPhieu",
                        column: x => x.MaPhieu,
                        principalTable: "PHIEUDAT",
                        principalColumn: "MaPhieu",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BAN_TenBan",
                table: "BAN",
                column: "TenBan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DATBAN_MaBan",
                table: "DATBAN",
                column: "MaBan");

            migrationBuilder.CreateIndex(
                name: "IX_DONHANG_MaMon",
                table: "DONHANG",
                column: "MaMon");

            migrationBuilder.CreateIndex(
                name: "IX_DONHANG_MaPhieu",
                table: "DONHANG",
                column: "MaPhieu");

            migrationBuilder.CreateIndex(
                name: "IX_HOADON_MaBan",
                table: "HOADON",
                column: "MaBan");

            migrationBuilder.CreateIndex(
                name: "IX_HOADON_MaKM",
                table: "HOADON",
                column: "MaKM");

            migrationBuilder.CreateIndex(
                name: "IX_HOADON_MaNV",
                table: "HOADON",
                column: "MaNV");

            migrationBuilder.CreateIndex(
                name: "IX_KHUYENMAI_MaVoucher",
                table: "KHUYENMAI",
                column: "MaVoucher",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MONAN_MaLoai",
                table: "MONAN",
                column: "MaLoai");

            migrationBuilder.CreateIndex(
                name: "IX_MONAN_TenMon",
                table: "MONAN",
                column: "TenMon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUDAT_MaHD",
                table: "PHIEUDAT",
                column: "MaHD");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "TAIKHOAN",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_TAIKHOAN_MaNV",
                table: "TAIKHOAN",
                column: "MaNV",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "TAIKHOAN",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DATBAN");

            migrationBuilder.DropTable(
                name: "DONHANG");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "TAIKHOAN");

            migrationBuilder.DropTable(
                name: "MONAN");

            migrationBuilder.DropTable(
                name: "PHIEUDAT");

            migrationBuilder.DropTable(
                name: "LOAIMON");

            migrationBuilder.DropTable(
                name: "HOADON");

            migrationBuilder.DropTable(
                name: "BAN");

            migrationBuilder.DropTable(
                name: "KHUYENMAI");

            migrationBuilder.DropTable(
                name: "NHANVIEN");
        }
    }
}
