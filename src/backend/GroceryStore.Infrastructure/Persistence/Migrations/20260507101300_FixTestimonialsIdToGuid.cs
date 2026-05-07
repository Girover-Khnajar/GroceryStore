using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GroceryStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixTestimonialsIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_testimonials",
                table: "testimonials");

            migrationBuilder.AddColumn<Guid>(
                name: "Id_tmp",
                table: "testimonials",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.Sql("UPDATE [testimonials] SET [Id_tmp] = NEWID() WHERE [Id_tmp] = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "testimonials");

            migrationBuilder.RenameColumn(
                name: "Id_tmp",
                table: "testimonials",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_testimonials",
                table: "testimonials",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_testimonials",
                table: "testimonials");

            migrationBuilder.AddColumn<int>(
                name: "Id_tmp",
                table: "testimonials",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "testimonials");

            migrationBuilder.RenameColumn(
                name: "Id_tmp",
                table: "testimonials",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_testimonials",
                table: "testimonials",
                column: "Id");
        }
    }
}
