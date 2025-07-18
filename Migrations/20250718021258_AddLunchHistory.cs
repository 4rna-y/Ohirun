using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ohirun.Migrations
{
    /// <inheritdoc />
    public partial class AddLunchHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Stores");

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Stores",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LunchHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StoreId = table.Column<int>(type: "INTEGER", nullable: false),
                    MealId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SuggestedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LunchHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LunchHistories_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LunchHistories_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LunchHistories_MealId",
                table: "LunchHistories",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_LunchHistories_StoreId",
                table: "LunchHistories",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_LunchHistory_UserId_SuggestedAt",
                table: "LunchHistories",
                columns: new[] { "UserId", "SuggestedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LunchHistories");

            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Stores");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Stores",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
