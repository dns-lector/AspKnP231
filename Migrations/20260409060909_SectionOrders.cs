using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspKnP231.Migrations
{
    /// <inheritdoc />
    public partial class SectionOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderInPrice",
                table: "ShopSections",
                type: "int",
                nullable: false,
                defaultValue: 10000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderInPrice",
                table: "ShopSections");
        }
    }
}
