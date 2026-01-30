using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolyo.Migrations
{
    public partial class AddAboutInfoTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AboutMe2Table_AboutMeTable",
                table: "AboutMe2Table");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectsTable_CategoryTable_CategoryID",
                table: "ProjectsTable");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Experinces Table",
                table: "Services Table");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessagesTable",
                table: "MessagesTable");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "AboutMeTable");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AboutMeTable");

            migrationBuilder.DropColumn(
                name: "Interests",
                table: "AboutMeTable");

            migrationBuilder.RenameTable(
                name: "Services Table",
                newName: "ServicesTable");

            migrationBuilder.RenameTable(
                name: "MessagesTable",
                newName: "MessageTable");

            migrationBuilder.RenameColumn(
                name: "TestimonialID",
                table: "TestimonialTable",
                newName: "TestimonialId");

            migrationBuilder.RenameColumn(
                name: "SkilID",
                table: "SkillTable",
                newName: "SkilId");

            migrationBuilder.RenameColumn(
                name: "CategoryID",
                table: "ProjectsTable",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "ProjectID",
                table: "ProjectsTable",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectsTable_CategoryID",
                table: "ProjectsTable",
                newName: "IX_ProjectsTable_CategoryId");

            migrationBuilder.RenameColumn(
                name: "homeID",
                table: "HomePage",
                newName: "HomeId");

            migrationBuilder.RenameColumn(
                name: "CategoryID",
                table: "CategoryTable",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "AboutID",
                table: "AboutMeTable",
                newName: "AboutId");

            migrationBuilder.RenameColumn(
                name: "AboutID",
                table: "AboutMe2Table",
                newName: "AboutId");

            migrationBuilder.RenameColumn(
                name: "DetailID",
                table: "AboutMe2Table",
                newName: "DetailId");

            migrationBuilder.RenameColumn(
                name: "ExperinceID",
                table: "ServicesTable",
                newName: "ExperinceId");

            migrationBuilder.AlterColumn<int>(
                name: "TestimonialId",
                table: "TestimonialTable",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "HomePage",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(10)",
                oldFixedLength: true,
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameSurname",
                table: "HomePage",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(10)",
                oldFixedLength: true,
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "HomePage",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nchar(10)",
                oldFixedLength: true,
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AboutMeTable",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServicesTable",
                table: "ServicesTable",
                column: "ExperinceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageTable",
                table: "MessageTable",
                column: "MessageId");

            migrationBuilder.CreateTable(
                name: "AboutInfoTable",
                columns: table => new
                {
                    AboutInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LongDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Interests = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutInfoTable", x => x.AboutInfoId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AboutMe2Table_AboutMeTable_DetailId",
                table: "AboutMe2Table",
                column: "DetailId",
                principalTable: "AboutMeTable",
                principalColumn: "AboutId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectsTable_CategoryTable_CategoryId",
                table: "ProjectsTable",
                column: "CategoryId",
                principalTable: "CategoryTable",
                principalColumn: "CategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AboutMe2Table_AboutMeTable_DetailId",
                table: "AboutMe2Table");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectsTable_CategoryTable_CategoryId",
                table: "ProjectsTable");

            migrationBuilder.DropTable(
                name: "AboutInfoTable");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServicesTable",
                table: "ServicesTable");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageTable",
                table: "MessageTable");

            migrationBuilder.RenameTable(
                name: "ServicesTable",
                newName: "Services Table");

            migrationBuilder.RenameTable(
                name: "MessageTable",
                newName: "MessagesTable");

            migrationBuilder.RenameColumn(
                name: "TestimonialId",
                table: "TestimonialTable",
                newName: "TestimonialID");

            migrationBuilder.RenameColumn(
                name: "SkilId",
                table: "SkillTable",
                newName: "SkilID");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "ProjectsTable",
                newName: "CategoryID");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "ProjectsTable",
                newName: "ProjectID");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectsTable_CategoryId",
                table: "ProjectsTable",
                newName: "IX_ProjectsTable_CategoryID");

            migrationBuilder.RenameColumn(
                name: "HomeId",
                table: "HomePage",
                newName: "homeID");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "CategoryTable",
                newName: "CategoryID");

            migrationBuilder.RenameColumn(
                name: "AboutId",
                table: "AboutMeTable",
                newName: "AboutID");

            migrationBuilder.RenameColumn(
                name: "AboutId",
                table: "AboutMe2Table",
                newName: "AboutID");

            migrationBuilder.RenameColumn(
                name: "DetailId",
                table: "AboutMe2Table",
                newName: "DetailID");

            migrationBuilder.RenameColumn(
                name: "ExperinceId",
                table: "Services Table",
                newName: "ExperinceID");

            migrationBuilder.AlterColumn<int>(
                name: "TestimonialID",
                table: "TestimonialTable",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "HomePage",
                type: "nchar(10)",
                fixedLength: true,
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NameSurname",
                table: "HomePage",
                type: "nchar(10)",
                fixedLength: true,
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "HomePage",
                type: "nchar(10)",
                fixedLength: true,
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AboutMeTable",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "AboutMeTable",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AboutMeTable",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Interests",
                table: "AboutMeTable",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Experinces Table",
                table: "Services Table",
                column: "ExperinceID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessagesTable",
                table: "MessagesTable",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_AboutMe2Table_AboutMeTable",
                table: "AboutMe2Table",
                column: "DetailID",
                principalTable: "AboutMeTable",
                principalColumn: "AboutID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectsTable_CategoryTable_CategoryID",
                table: "ProjectsTable",
                column: "CategoryID",
                principalTable: "CategoryTable",
                principalColumn: "CategoryID");
        }
    }
}
