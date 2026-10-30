
public partial class AddRequestComments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RequestComments",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RequestId = table.Column<int>(nullable: false),
                AuthorId = table.Column<int>(nullable: false),
                Text = table.Column<string>(maxLength: 4000, nullable: false),
                CreatedAt = table.Column<DateTime>(nullable: false,
                    defaultValueSql: "SYSUTCDATETIME()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RequestComments", x => x.Id);

                table.ForeignKey(
                    name: "FK_RequestComments_Requests_RequestId",
                    column: x => x.RequestId,
                    principalTable: "Requests",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);

               
                table.ForeignKey(
                    name: "FK_RequestComments_Users_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);

                
                table.CheckConstraint(
                    "CK_RequestComments_Text_NotEmpty",
                    "LEN(LTRIM(RTRIM([Text]))) > 0");
            });

        migrationBuilder.CreateIndex(
            name: "IX_RequestComments_RequestId_CreatedAt",
            table: "RequestComments",
            columns: new[] { "RequestId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_RequestComments_AuthorId",
            table: "RequestComments",
            column: "AuthorId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropTable(name: "RequestComments");
}
