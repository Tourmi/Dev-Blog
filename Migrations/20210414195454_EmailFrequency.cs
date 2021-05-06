using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dev_Blog.Migrations
{
    public partial class EmailFrequency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastEmailDate",
                table: "Subscribers",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaximumEmailFrequency",
                table: "Subscribers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEmailDate",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "MaximumEmailFrequency",
                table: "Subscribers");
        }
    }
}
