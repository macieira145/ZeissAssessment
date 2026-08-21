using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZeissAssessment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductIdSequenceRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RestartSequence(
                name: "ProductIdSequence",
                schema: "dbo",
                startValue: 100000L);

            migrationBuilder.AlterSequence(
                name: "ProductIdSequence",
                schema: "dbo",
                minValue: 100000L,
                maxValue: 999999L);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_Id_Range",
                table: "Products",
                sql: "[Id] BETWEEN 100000 AND 999999");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_Id_Range",
                table: "Products");

            migrationBuilder.AlterSequence(
                name: "ProductIdSequence",
                schema: "dbo",
                oldMinValue: 100000L,
                oldMaxValue: 999999L);

            migrationBuilder.RestartSequence(
                name: "ProductIdSequence",
                schema: "dbo",
                startValue: 1L);
        }
    }
}
