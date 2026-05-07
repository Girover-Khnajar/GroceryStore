using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTestimonialsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "testimonials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    client_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    client_title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    client_company = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    client_image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    rating = table.Column<byte>(type: "tinyint", nullable: true),
                    testimonial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_testimonials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_testimonials_is_active",
                table: "testimonials",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_testimonials_sort_order",
                table: "testimonials",
                column: "sort_order");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "testimonials");
        }
    }
}
