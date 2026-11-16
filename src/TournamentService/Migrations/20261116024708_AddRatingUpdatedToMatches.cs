using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentService.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingUpdatedToMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RatingUpdated",
                table: "Matches",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingUpdated",
                table: "Matches");
        }
    }
}
