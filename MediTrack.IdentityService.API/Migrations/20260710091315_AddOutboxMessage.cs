using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediTrack.IdentityService.API.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Este servicio nunca tuvo migraciones de EF Core antes (el esquema se creaba
    /// con <c>EnsureCreatedAsync</c>). Como esta es la primera migración generada,
    /// EF Core la produjo inicialmente con un snapshot completo del modelo actual
    /// (incluyendo `users`, que ya existe en producción con filas reales). Se
    /// recortó a mano para que sea puramente aditiva: solo crea `outbox_message`
    /// y su índice. No toca `users` ni ninguna otra tabla existente.
    /// </remarks>
    public partial class AddOutboxMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_message",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "binary(16)", nullable: false),
                    EventType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "json", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_message", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_ProcessedAtUtc",
                table: "outbox_message",
                column: "ProcessedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_message");
        }
    }
}
