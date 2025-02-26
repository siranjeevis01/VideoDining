using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoDiningApp.Migrations
{
    /// <inheritdoc />
    public partial class video2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "VideoCallRequests",
                type: "time(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "VideoCallRequests");
        }
    }
}
