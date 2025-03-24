using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoDiningApp.Migrations
{
    /// <inheritdoc />
    public partial class u1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$MKNldbYLCV9VjG5CK9ib3eP2Czc3gKbkGaV3w5USRF2t2QLOrXb9.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$k0Xi1W/XFet1Qd6ftGVsp.hkIj66sulwhl8YMYVYq4uInTrtyTtya");
        }
    }
}
