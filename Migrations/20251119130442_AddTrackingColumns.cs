using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLQA_TH.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaNV_Bep",
                table: "PHIEUDAT",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaNV_Dat",
                table: "PHIEUDAT",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaNV_PhucVu",
                table: "PHIEUDAT",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUDAT_MaNV_Bep",
                table: "PHIEUDAT",
                column: "MaNV_Bep");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUDAT_MaNV_Dat",
                table: "PHIEUDAT",
                column: "MaNV_Dat");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUDAT_MaNV_PhucVu",
                table: "PHIEUDAT",
                column: "MaNV_PhucVu");

            migrationBuilder.AddForeignKey(
                name: "FK_PHIEUDAT_NHANVIEN_MaNV_Bep",
                table: "PHIEUDAT",
                column: "MaNV_Bep",
                principalTable: "NHANVIEN",
                principalColumn: "MaNV",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PHIEUDAT_NHANVIEN_MaNV_Dat",
                table: "PHIEUDAT",
                column: "MaNV_Dat",
                principalTable: "NHANVIEN",
                principalColumn: "MaNV",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PHIEUDAT_NHANVIEN_MaNV_PhucVu",
                table: "PHIEUDAT",
                column: "MaNV_PhucVu",
                principalTable: "NHANVIEN",
                principalColumn: "MaNV",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PHIEUDAT_NHANVIEN_MaNV_Bep",
                table: "PHIEUDAT");

            migrationBuilder.DropForeignKey(
                name: "FK_PHIEUDAT_NHANVIEN_MaNV_Dat",
                table: "PHIEUDAT");

            migrationBuilder.DropForeignKey(
                name: "FK_PHIEUDAT_NHANVIEN_MaNV_PhucVu",
                table: "PHIEUDAT");

            migrationBuilder.DropIndex(
                name: "IX_PHIEUDAT_MaNV_Bep",
                table: "PHIEUDAT");

            migrationBuilder.DropIndex(
                name: "IX_PHIEUDAT_MaNV_Dat",
                table: "PHIEUDAT");

            migrationBuilder.DropIndex(
                name: "IX_PHIEUDAT_MaNV_PhucVu",
                table: "PHIEUDAT");

            migrationBuilder.DropColumn(
                name: "MaNV_Bep",
                table: "PHIEUDAT");

            migrationBuilder.DropColumn(
                name: "MaNV_Dat",
                table: "PHIEUDAT");

            migrationBuilder.DropColumn(
                name: "MaNV_PhucVu",
                table: "PHIEUDAT");
        }
    }
}
