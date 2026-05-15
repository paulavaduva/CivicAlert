using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicAlert.Migrations
{
    /// <inheritdoc />
    public partial class addIssueNewAttr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AiConfidenceScore",
                table: "Issues",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiValidationReason",
                table: "Issues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "Issues",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiConfidenceScore",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "AiValidationReason",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "Issues");
        }
    }
}
