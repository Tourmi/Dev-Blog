using Microsoft.EntityFrameworkCore.Migrations;

namespace Dev_Blog.Migrations
{
    public partial class subscribers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscribers",
                columns: table => new
                {
                    Email = table.Column<string>(nullable: false),
                    ValidationToken = table.Column<string>(nullable: false),
                    ValidatedEmail = table.Column<bool>(nullable: false),
                    SubscribedToAll = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribers", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "SubscriberTags",
                columns: table => new
                {
                    SubscriberID = table.Column<string>(nullable: false),
                    TagID = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberTags", x => new { x.SubscriberID, x.TagID });
                    table.ForeignKey(
                        name: "FK_SubscriberTags_Subscribers_SubscriberID",
                        column: x => x.SubscriberID,
                        principalTable: "Subscribers",
                        principalColumn: "Email",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriberTags_Tags_TagID",
                        column: x => x.TagID,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DisplayName",
                table: "AspNetUsers",
                column: "DisplayName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_ValidationToken",
                table: "Subscribers",
                column: "ValidationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberTags_TagID",
                table: "SubscriberTags",
                column: "TagID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberTags");

            migrationBuilder.DropTable(
                name: "Subscribers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DisplayName",
                table: "AspNetUsers");
        }
    }
}
