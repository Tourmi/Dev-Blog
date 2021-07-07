using Microsoft.EntityFrameworkCore.Migrations;

namespace Dev_Blog.Migrations
{
    public partial class postfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogFile_Posts_PostID",
                table: "BlogFile");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_AuthorID",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AuthorID",
                table: "Comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogFile",
                table: "BlogFile");

            migrationBuilder.DropColumn(
                name: "AuthorID",
                table: "Comments");

            migrationBuilder.RenameTable(
                name: "BlogFile",
                newName: "PostFiles");

            migrationBuilder.RenameIndex(
                name: "IX_BlogFile_PostID",
                table: "PostFiles",
                newName: "IX_PostFiles_PostID");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "Comments",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostFiles",
                table: "PostFiles",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserID",
                table: "Comments",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_UserID",
                table: "Comments",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PostFiles_Posts_PostID",
                table: "PostFiles",
                column: "PostID",
                principalTable: "Posts",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_AspNetUsers_UserID",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostFiles_Posts_PostID",
                table: "PostFiles");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserID",
                table: "Comments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostFiles",
                table: "PostFiles");

            migrationBuilder.RenameTable(
                name: "PostFiles",
                newName: "BlogFile");

            migrationBuilder.RenameIndex(
                name: "IX_PostFiles_PostID",
                table: "BlogFile",
                newName: "IX_BlogFile_PostID");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "Comments",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorID",
                table: "Comments",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogFile",
                table: "BlogFile",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorID",
                table: "Comments",
                column: "AuthorID");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogFile_Posts_PostID",
                table: "BlogFile",
                column: "PostID",
                principalTable: "Posts",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_AspNetUsers_AuthorID",
                table: "Comments",
                column: "AuthorID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
