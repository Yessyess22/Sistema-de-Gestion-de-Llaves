using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaGestionLlaves.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntentoAcceso",
                columns: table => new
                {
                    id_intento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_usuario = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    exitoso = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
                    nombre_permiso = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.id_permiso);
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                columns: table => new
                {
                    id_persona = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ci = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_nacimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    genero = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: true),
                    correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    celular = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    estado = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "A")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.id_persona);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_rol = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    estado = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "A")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "TipoAmbiente",
                columns: table => new
                {
                    id_tipo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_tipo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoAmbiente", x => x.id_tipo);
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

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_persona = table.Column<int>(type: "integer", nullable: false),
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    nombre_usuario = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "A")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_Usuario_Persona_id_persona",
                        column: x => x.id_persona,
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol_id_rol",
                        column: x => x.id_rol,
                        principalTable: "Rol",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ambiente",
                columns: table => new
                {
                    id_ambiente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ubicacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    id_tipo = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "A")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ambiente", x => x.id_ambiente);
                    table.ForeignKey(
                        name: "FK_Ambiente_TipoAmbiente_id_tipo",
                        column: x => x.id_tipo,
                        principalTable: "TipoAmbiente",
                        principalColumn: "id_tipo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    id_auditoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tabla_afectada = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    operacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_registro = table.Column<int>(type: "integer", nullable: true),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    fecha_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    datos_anteriores = table.Column<string>(type: "text", nullable: true),
                    datos_nuevos = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.id_auditoria);
                    table.ForeignKey(
                        name: "FK_Auditoria_Usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Llave",
                columns: table => new
                {
                    id_llave = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    num_copias = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    id_ambiente = table.Column<int>(type: "integer", nullable: false),
                    es_maestra = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    estado = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "D"),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Llave", x => x.id_llave);
                    table.ForeignKey(
                        name: "FK_Llave_Ambiente_id_ambiente",
                        column: x => x.id_ambiente,
                        principalTable: "Ambiente",
                        principalColumn: "id_ambiente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Persona_Autorizada",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_persona = table.Column<int>(type: "integer", nullable: false),
                    id_llave = table.Column<int>(type: "integer", nullable: false)
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
                name: "Prestamo",
                columns: table => new
                {
                    id_prestamo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_llave = table.Column<int>(type: "integer", nullable: false),
                    id_persona = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    fecha_hora_prestamo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    fecha_hora_devolucion_esperada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_hora_devolucion_real = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "A"),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestamo", x => x.id_prestamo);
                    table.ForeignKey(
                        name: "FK_Prestamo_Llave_id_llave",
                        column: x => x.id_llave,
                        principalTable: "Llave",
                        principalColumn: "id_llave",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prestamo_Persona_id_persona",
                        column: x => x.id_persona,
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prestamo_Usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reserva",
                columns: table => new
                {
                    id_reserva = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_llave = table.Column<int>(type: "integer", nullable: false),
                    id_persona = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false, defaultValue: "P")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reserva", x => x.id_reserva);
                    table.ForeignKey(
                        name: "FK_Reserva_Llave_id_llave",
                        column: x => x.id_llave,
                        principalTable: "Llave",
                        principalColumn: "id_llave",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reserva_Persona_id_persona",
                        column: x => x.id_persona,
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reserva_Usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlertaNotificacion",
                columns: table => new
                {
                    id_alerta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo_alerta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    id_prestamo = table.Column<int>(type: "integer", nullable: true),
                    id_llave = table.Column<int>(type: "integer", nullable: true),
                    mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    fecha_generada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    leida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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
                name: "IX_Ambiente_id_tipo",
                table: "Ambiente",
                column: "id_tipo");

            migrationBuilder.CreateIndex(
                name: "UQ_Ambiente_Codigo",
                table: "Ambiente",
                column: "codigo",
                unique: true);

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
                name: "IX_IntentoAcceso_Fecha",
                table: "IntentoAcceso",
                column: "fecha_hora");

            migrationBuilder.CreateIndex(
                name: "IX_Llave_id_ambiente",
                table: "Llave",
                column: "id_ambiente");

            migrationBuilder.CreateIndex(
                name: "UQ_Llave_Codigo",
                table: "Llave",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Permiso_Nombre",
                table: "Permisos",
                column: "nombre_permiso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persona_Correo",
                table: "Persona",
                column: "correo");

            migrationBuilder.CreateIndex(
                name: "UQ_Persona_Ci",
                table: "Persona",
                column: "ci",
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
                name: "IX_Prestamo_Estado",
                table: "Prestamo",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_Fecha",
                table: "Prestamo",
                column: "fecha_hora_prestamo");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_id_llave",
                table: "Prestamo",
                column: "id_llave");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_id_persona",
                table: "Prestamo",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_id_usuario",
                table: "Prestamo",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_Estado",
                table: "Reserva",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_id_llave",
                table: "Reserva",
                column: "id_llave");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_id_persona",
                table: "Reserva",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_id_usuario",
                table: "Reserva",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "UQ_Rol_Nombre",
                table: "Rol",
                column: "nombre_rol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_id_permiso",
                table: "RolPermisos",
                column: "id_permiso");

            migrationBuilder.CreateIndex(
                name: "UQ_TipoAmbiente_Nombre",
                table: "TipoAmbiente",
                column: "nombre_tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_id_persona",
                table: "Usuario",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_id_rol",
                table: "Usuario",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "UQ_Usuario_NombreUsuario",
                table: "Usuario",
                column: "nombre_usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertaNotificacion");

            migrationBuilder.DropTable(
                name: "Auditoria");

            migrationBuilder.DropTable(
                name: "IntentoAcceso");

            migrationBuilder.DropTable(
                name: "Persona_Autorizada");

            migrationBuilder.DropTable(
                name: "Reserva");

            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DropTable(
                name: "Prestamo");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Llave");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Ambiente");

            migrationBuilder.DropTable(
                name: "Persona");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "TipoAmbiente");
        }
    }
}
