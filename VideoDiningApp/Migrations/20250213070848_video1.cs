using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoDiningApp.Migrations
{
    /// <inheritdoc />
    public partial class video1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndedAt",
                table: "VideoCallRequests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "VideoCallRequests",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndedAt",
                table: "VideoCallRequests");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "VideoCallRequests");
        }
    }
}
