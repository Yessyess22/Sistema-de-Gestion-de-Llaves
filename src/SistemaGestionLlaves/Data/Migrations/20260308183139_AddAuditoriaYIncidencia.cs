using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaGestionLlaves.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditoriaYIncidencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auditoria_Usuario_id_usuario",
                table: "Auditoria");

            migrationBuilder.DropTable(
                name: "AlertaNotificacion");

            migrationBuilder.DropTable(
                name: "IntentoAcceso");

            migrationBuilder.DropTable(
                name: "Persona_Autorizada");

            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropIndex(
                name: "IX_Auditoria_Fecha",
                table: "Auditoria");

            migrationBuilder.DropIndex(
                name: "IX_Auditoria_id_usuario",
                table: "Auditoria");

            migrationBuilder.DropIndex(
                name: "IX_Auditoria_Tabla",
                table: "Auditoria");

            migrationBuilder.DropColumn(
                name: "id_registro",
                table: "Auditoria");

            migrationBuilder.DropColumn(
                name: "id_usuario",
                table: "Auditoria");

            migrationBuilder.DropColumn(
                name: "operacion",
                table: "Auditoria");

            migrationBuilder.DropColumn(
                name: "tabla_afectada",
                table: "Auditoria");

            migrationBuilder.RenameColumn(
                name: "datos_nuevos",
                table: "Auditoria",
                newName: "valores_nuevos");

            migrationBuilder.RenameColumn(
                name: "datos_anteriores",
                table: "Auditoria",
                newName: "valores_anteriores");

            migrationBuilder.RenameColumn(
                name: "id_auditoria",
                table: "Auditoria",
                newName: "id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_hora",
                table: "Auditoria",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "columnas_afectadas",
                table: "Auditoria",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "llave_primaria",
                table: "Auditoria",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tabla",
                table: "Auditoria",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_accion",
                table: "Auditoria",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "Auditoria",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Incidencia",
                columns: table => new
                {
                    id_incidencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_llave = table.Column<int>(type: "integer", nullable: false),
                    tipo_incidencia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    fecha_reporte = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    fecha_resolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "A"),
                    notas_resolucion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidencia", x => x.id_incidencia);
                    table.ForeignKey(
                        name: "FK_Incidencia_Llave_id_llave",
                        column: x => x.id_llave,
                        principalTable: "Llave",
                        principalColumn: "id_llave",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidencia_id_llave",
                table: "Incidencia",
                column: "id_llave");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Incidencia");

            migrationBuilder.DropColumn(
                name: "columnas_afectadas",
                table: "Auditoria");

            migrationBuilder.DropColumn(
                name: "llave_primaria",
                table: "Auditoria");

            migrationBuilder.DropColumn(
                name: "tabla",
                table: "Auditoria");

            migrationBuilder.DropColumn(
                name: "tipo_accion",
                table: "Auditoria");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "Auditoria");

            migrationBuilder.RenameColumn(
                name: "valores_nuevos",
                table: "Auditoria",
                newName: "datos_nuevos");

            migrationBuilder.RenameColumn(
                name: "valores_anteriores",
                table: "Auditoria",
                newName: "datos_anteriores");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Auditoria",
                newName: "id_auditoria");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_hora",
                table: "Auditoria",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "id_registro",
                table: "Auditoria",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_usuario",
                table: "Auditoria",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "operacion",
                table: "Auditoria",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tabla_afectada",
                table: "Auditoria",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AlertaNotificacion",
                columns: table => new
                {
                    id_alerta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_llave = table.Column<int>(type: "integer", nullable: true),
                    id_prestamo = table.Column<int>(type: "integer", nullable: true),
                    fecha_generada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    leida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tipo_alerta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertaNotificacion", x => x.id_alerta);
                    table.ForeignKey(
                        name: "FK_AlertaNotificacion_Llave_id_llave",
                        column: x => x.id_llave,
                        principalTable: "Llave",
                        principalColumn: "id_llave",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AlertaNotificacion_Prestamo_id_prestamo",
                        column: x => x.id_prestamo,
                        principalTable: "Prestamo",
                        principalColumn: "id_prestamo",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "IntentoAcceso",
                columns: table => new
                {
                    id_intento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    exitoso = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nombre_usuario = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentoAcceso", x => x.id_intento);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    id_permiso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    nombre_permiso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.id_permiso);
                });

            migrationBuilder.CreateTable(
                name: "Persona_Autorizada",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_llave = table.Column<int>(type: "integer", nullable: false),
                    id_persona = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona_Autorizada", x => x.id);
                    table.ForeignKey(
                        name: "FK_Persona_Autorizada_Llave_id_llave",
                        column: x => x.id_llave,
                        principalTable: "Llave",
                        principalColumn: "id_llave",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Persona_Autorizada_Persona_id_persona",
                        column: x => x.id_persona,
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolPermisos",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    id_permiso = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermisos", x => new { x.id_rol, x.id_permiso });
                    table.ForeignKey(
                        name: "FK_RolPermisos_Permisos_id_permiso",
                        column: x => x.id_permiso,
                        principalTable: "Permisos",
                        principalColumn: "id_permiso",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolPermisos_Rol_id_rol",
                        column: x => x.id_rol,
                        principalTable: "Rol",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Fecha",
                table: "Auditoria",
                column: "fecha_hora");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_id_usuario",
                table: "Auditoria",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Tabla",
                table: "Auditoria",
                column: "tabla_afectada");

            migrationBuilder.CreateIndex(
                name: "IX_Alerta_Leida",
                table: "AlertaNotificacion",
                column: "leida");

            migrationBuilder.CreateIndex(
                name: "IX_AlertaNotificacion_id_llave",
                table: "AlertaNotificacion",
                column: "id_llave");

            migrationBuilder.CreateIndex(
                name: "IX_AlertaNotificacion_id_prestamo",
                table: "AlertaNotificacion",
                column: "id_prestamo");

            migrationBuilder.CreateIndex(
                name: "IX_IntentoAcceso_Fecha",
                table: "IntentoAcceso",
                column: "fecha_hora");

            migrationBuilder.CreateIndex(
                name: "UQ_Permiso_Nombre",
                table: "Permisos",
                column: "nombre_permiso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persona_Autorizada_id_llave",
                table: "Persona_Autorizada",
                column: "id_llave");

            migrationBuilder.CreateIndex(
                name: "UQ_PersonaAutorizada_PersonaLlave",
                table: "Persona_Autorizada",
                columns: new[] { "id_persona", "id_llave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_id_permiso",
                table: "RolPermisos",
                column: "id_permiso");

            migrationBuilder.AddForeignKey(
                name: "FK_Auditoria_Usuario_id_usuario",
                table: "Auditoria",
                column: "id_usuario",
                principalTable: "Usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
