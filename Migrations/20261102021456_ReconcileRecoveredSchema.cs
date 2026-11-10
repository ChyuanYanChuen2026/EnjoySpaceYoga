using Microsoft.EntityFrameworkCore.Migrations;
namespace EnjoySpace.Web.Migrations;
// Guarded recovery: preserves existing data and missing-source history identifiers.
public partial class ReconcileRecoveredSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        using var stream = typeof(ReconcileRecoveredSchema).Assembly.GetManifestResourceStream("EnjoySpace.Recovery.compatible-up.sql")
            ?? throw new InvalidOperationException("Missing frozen recovery SQL resource.");
        using var reader = new StreamReader(stream);
        migrationBuilder.Sql(reader.ReadToEnd());
    }
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        "THROW 51091, N'Recovered objects predate this migration. Automatic downgrade is disabled; use an approved backup or forward correction.', 1;");
}
