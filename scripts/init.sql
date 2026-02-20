-- ============================================================
-- Sistema de Gestión de Llaves
-- Universidad Privada Domingo Savio (UPDS)
-- Script SQL de referencia (3FN - Tercera Forma Normal)
-- Generado como referencia del Sprint 1
-- ============================================================

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'GestionLlaves')
BEGIN
    CREATE DATABASE GestionLlaves
    COLLATE Modern_Spanish_CI_AI;
END
GO

USE GestionLlaves;
GO

-- ============================================================
-- CATÁLOGOS / ENTIDADES MAESTRAS
-- ============================================================

-- Tipo de Ambiente (Oficina, Laboratorio, Depósito, etc.)
CREATE TABLE TipoAmbiente (
    id_tipo        INT IDENTITY(1,1) NOT NULL,
    nombre_tipo    NVARCHAR(80)      NOT NULL,
    CONSTRAINT PK_TipoAmbiente PRIMARY KEY (id_tipo),
    CONSTRAINT UQ_TipoAmbiente_Nombre UNIQUE (nombre_tipo)
);
GO

-- Rol de usuario
CREATE TABLE Rol (
    id_rol       INT IDENTITY(1,1) NOT NULL,
    nombre_rol   NVARCHAR(80)      NOT NULL,
    descripcion  NVARCHAR(250)     NULL,
    estado       CHAR(1)           NOT NULL DEFAULT 'A',   -- A=Activo, I=Inactivo
    CONSTRAINT PK_Rol PRIMARY KEY (id_rol),
    CONSTRAINT UQ_Rol_Nombre UNIQUE (nombre_rol),
    CONSTRAINT CK_Rol_Estado CHECK (estado IN ('A','I'))
);
GO

-- Permisos del sistema
CREATE TABLE Permisos (
    id_permiso      INT IDENTITY(1,1) NOT NULL,
    nombre_permiso  NVARCHAR(100)     NOT NULL,
    descripcion     NVARCHAR(250)     NULL,
    CONSTRAINT PK_Permiso PRIMARY KEY (id_permiso),
    CONSTRAINT UQ_Permiso_Nombre UNIQUE (nombre_permiso)
);
GO

-- Tabla intermedia Rol-Permiso (muchos a muchos)
CREATE TABLE RolPermisos (
    id_rol      INT NOT NULL,
    id_permiso  INT NOT NULL,
    CONSTRAINT PK_RolPermisos PRIMARY KEY (id_rol, id_permiso),
    CONSTRAINT FK_RolPermisos_Rol FOREIGN KEY (id_rol)
        REFERENCES Rol(id_rol) ON DELETE CASCADE,
    CONSTRAINT FK_RolPermisos_Permiso FOREIGN KEY (id_permiso)
        REFERENCES Permisos(id_permiso) ON DELETE CASCADE
);
GO

-- ============================================================
-- PERSONAS Y USUARIOS
-- ============================================================

-- Persona (docente, estudiante, administrativo, etc.)
CREATE TABLE Persona (
    id_persona       INT IDENTITY(1,1) NOT NULL,
    nombres          NVARCHAR(100)     NOT NULL,
    apellidos        NVARCHAR(100)     NOT NULL,
    ci               NVARCHAR(20)      NOT NULL,
    fecha_nacimiento DATE              NULL,
    genero           CHAR(1)           NULL,                -- M, F, O
    correo           NVARCHAR(150)     NULL,
    celular          NVARCHAR(20)      NULL,
    estado           CHAR(1)           NOT NULL DEFAULT 'A', -- A=Activo, I=Inactivo
    CONSTRAINT PK_Persona PRIMARY KEY (id_persona),
    CONSTRAINT UQ_Persona_CI UNIQUE (ci),
    CONSTRAINT CK_Persona_Genero CHECK (genero IN ('M','F','O') OR genero IS NULL),
    CONSTRAINT CK_Persona_Estado CHECK (estado IN ('A','I'))
);
GO
CREATE INDEX IX_Persona_Correo ON Persona(correo);
GO

-- Usuario del sistema (login)
CREATE TABLE Usuario (
    id_usuario     INT IDENTITY(1,1) NOT NULL,
    id_persona     INT               NOT NULL,
    id_rol         INT               NOT NULL,
    nombre_usuario NVARCHAR(80)      NOT NULL,
    password_hash  NVARCHAR(200)     NOT NULL,  -- BCrypt hash, NUNCA texto plano
    fecha_inicio   DATETIME2         NULL,
    fecha_fin      DATETIME2         NULL,
    estado         CHAR(1)           NOT NULL DEFAULT 'A', -- A=Activo, I=Inactivo, B=Bloqueado
    CONSTRAINT PK_Usuario PRIMARY KEY (id_usuario),
    CONSTRAINT UQ_Usuario_NombreUsuario UNIQUE (nombre_usuario),
    CONSTRAINT FK_Usuario_Persona FOREIGN KEY (id_persona)
        REFERENCES Persona(id_persona) ON DELETE NO ACTION,
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (id_rol)
        REFERENCES Rol(id_rol) ON DELETE NO ACTION,
    CONSTRAINT CK_Usuario_Estado CHECK (estado IN ('A','I','B'))
);
GO

-- ============================================================
-- AMBIENTES Y LLAVES
-- ============================================================

-- Ambiente físico (aula, laboratorio, oficina)
CREATE TABLE Ambiente (
    id_ambiente  INT IDENTITY(1,1) NOT NULL,
    codigo       NVARCHAR(20)      NOT NULL,
    nombre       NVARCHAR(150)     NOT NULL,
    ubicacion    NVARCHAR(200)     NULL,
    id_tipo      INT               NOT NULL,
    estado       CHAR(1)           NOT NULL DEFAULT 'A', -- A=Activo, I=Inactivo
    CONSTRAINT PK_Ambiente PRIMARY KEY (id_ambiente),
    CONSTRAINT UQ_Ambiente_Codigo UNIQUE (codigo),
    CONSTRAINT FK_Ambiente_Tipo FOREIGN KEY (id_tipo)
        REFERENCES TipoAmbiente(id_tipo) ON DELETE NO ACTION,
    CONSTRAINT CK_Ambiente_Estado CHECK (estado IN ('A','I'))
);
GO

-- Llave física del ambiente
CREATE TABLE Llave (
    id_llave      INT IDENTITY(1,1) NOT NULL,
    codigo        NVARCHAR(30)      NOT NULL,
    num_copias    INT               NOT NULL DEFAULT 1,
    id_ambiente   INT               NOT NULL,
    es_maestra    BIT               NOT NULL DEFAULT 0,
    estado        CHAR(1)           NOT NULL DEFAULT 'D', -- D=Disponible, P=Prestada, R=Reservada, I=Inactiva
    observaciones NVARCHAR(300)     NULL,
    CONSTRAINT PK_Llave PRIMARY KEY (id_llave),
    CONSTRAINT UQ_Llave_Codigo UNIQUE (codigo),
    CONSTRAINT FK_Llave_Ambiente FOREIGN KEY (id_ambiente)
        REFERENCES Ambiente(id_ambiente) ON DELETE NO ACTION,
    CONSTRAINT CK_Llave_Estado CHECK (estado IN ('D','P','R','I')),
    CONSTRAINT CK_Llave_NumCopias CHECK (num_copias >= 1)
);
GO

-- Personas autorizadas a solicitar una llave específica
CREATE TABLE Persona_Autorizada (
    id          INT IDENTITY(1,1) NOT NULL,
    id_persona  INT               NOT NULL,
    id_llave    INT               NOT NULL,
    CONSTRAINT PK_PersonaAutorizada PRIMARY KEY (id),
    CONSTRAINT UQ_PersonaAutorizada_PersonaLlave UNIQUE (id_persona, id_llave),
    CONSTRAINT FK_PersonaAutorizada_Persona FOREIGN KEY (id_persona)
        REFERENCES Persona(id_persona) ON DELETE CASCADE,
    CONSTRAINT FK_PersonaAutorizada_Llave FOREIGN KEY (id_llave)
        REFERENCES Llave(id_llave) ON DELETE CASCADE
);
GO

-- ============================================================
-- PRÉSTAMOS Y RESERVAS
-- ============================================================

-- Préstamo de llave
CREATE TABLE Prestamo (
    id_prestamo                   INT IDENTITY(1,1) NOT NULL,
    id_llave                      INT               NOT NULL,
    id_persona                    INT               NOT NULL,
    id_usuario                    INT               NOT NULL,
    fecha_hora_prestamo           DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    fecha_hora_devolucion_esperada DATETIME2        NULL,
    fecha_hora_devolucion_real    DATETIME2         NULL,
    estado                        CHAR(1)           NOT NULL DEFAULT 'A', -- A=Activo, D=Devuelto, V=Vencido, C=Cancelado
    observaciones                 NVARCHAR(300)     NULL,
    CONSTRAINT PK_Prestamo PRIMARY KEY (id_prestamo),
    CONSTRAINT FK_Prestamo_Llave FOREIGN KEY (id_llave)
        REFERENCES Llave(id_llave) ON DELETE NO ACTION,
    CONSTRAINT FK_Prestamo_Persona FOREIGN KEY (id_persona)
        REFERENCES Persona(id_persona) ON DELETE NO ACTION,
    CONSTRAINT FK_Prestamo_Usuario FOREIGN KEY (id_usuario)
        REFERENCES Usuario(id_usuario) ON DELETE NO ACTION,
    CONSTRAINT CK_Prestamo_Estado CHECK (estado IN ('A','D','V','C'))
);
GO
CREATE INDEX IX_Prestamo_Estado ON Prestamo(estado);
CREATE INDEX IX_Prestamo_Fecha ON Prestamo(fecha_hora_prestamo);
GO

-- Reserva anticipada de llave
CREATE TABLE Reserva (
    id_reserva   INT IDENTITY(1,1) NOT NULL,
    id_llave     INT               NOT NULL,
    id_persona   INT               NOT NULL,
    id_usuario   INT               NOT NULL,
    fecha_inicio DATETIME2         NOT NULL,
    fecha_fin    DATETIME2         NOT NULL,
    estado       CHAR(1)           NOT NULL DEFAULT 'P', -- P=Pendiente, C=Confirmada, U=Utilizada, X=Cancelada
    CONSTRAINT PK_Reserva PRIMARY KEY (id_reserva),
    CONSTRAINT FK_Reserva_Llave FOREIGN KEY (id_llave)
        REFERENCES Llave(id_llave) ON DELETE NO ACTION,
    CONSTRAINT FK_Reserva_Persona FOREIGN KEY (id_persona)
        REFERENCES Persona(id_persona) ON DELETE NO ACTION,
    CONSTRAINT FK_Reserva_Usuario FOREIGN KEY (id_usuario)
        REFERENCES Usuario(id_usuario) ON DELETE NO ACTION,
    CONSTRAINT CK_Reserva_Estado CHECK (estado IN ('P','C','U','X')),
    CONSTRAINT CK_Reserva_Fechas CHECK (fecha_fin > fecha_inicio)
);
GO
CREATE INDEX IX_Reserva_Estado ON Reserva(estado);
GO

-- ============================================================
-- AUDITORÍA Y SEGURIDAD
-- ============================================================

-- Registro de auditoría de todas las operaciones
CREATE TABLE Auditoria (
    id_auditoria    INT IDENTITY(1,1)  NOT NULL,
    tabla_afectada  NVARCHAR(100)      NOT NULL,
    operacion       NVARCHAR(20)       NOT NULL,  -- INSERT, UPDATE, DELETE, LOGIN
    id_registro     INT                NULL,
    id_usuario      INT                NULL,
    fecha_hora      DATETIME2          NOT NULL DEFAULT GETUTCDATE(),
    datos_anteriores NVARCHAR(MAX)     NULL,      -- JSON antes del cambio
    datos_nuevos    NVARCHAR(MAX)      NULL,       -- JSON después del cambio
    CONSTRAINT PK_Auditoria PRIMARY KEY (id_auditoria),
    CONSTRAINT FK_Auditoria_Usuario FOREIGN KEY (id_usuario)
        REFERENCES Usuario(id_usuario) ON DELETE SET NULL
);
GO
CREATE INDEX IX_Auditoria_Fecha ON Auditoria(fecha_hora);
CREATE INDEX IX_Auditoria_Tabla ON Auditoria(tabla_afectada);
GO

-- Intentos de acceso al sistema
CREATE TABLE IntentoAcceso (
    id_intento     INT IDENTITY(1,1) NOT NULL,
    nombre_usuario NVARCHAR(80)      NOT NULL,
    fecha_hora     DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    ip             NVARCHAR(50)      NULL,
    exitoso        BIT               NOT NULL DEFAULT 0,
    CONSTRAINT PK_IntentoAcceso PRIMARY KEY (id_intento)
);
GO
CREATE INDEX IX_IntentoAcceso_Fecha ON IntentoAcceso(fecha_hora);
GO

-- Alertas y notificaciones del sistema
CREATE TABLE AlertaNotificacion (
    id_alerta      INT IDENTITY(1,1) NOT NULL,
    tipo_alerta    NVARCHAR(50)      NOT NULL,  -- VENCIMIENTO, PERDIDA, DEVOLUCION, RESERVA
    id_prestamo    INT               NULL,
    id_llave       INT               NULL,
    mensaje        NVARCHAR(500)     NOT NULL,
    fecha_generada DATETIME2         NOT NULL DEFAULT GETUTCDATE(),
    leida          BIT               NOT NULL DEFAULT 0,
    CONSTRAINT PK_AlertaNotificacion PRIMARY KEY (id_alerta),
    CONSTRAINT FK_Alerta_Prestamo FOREIGN KEY (id_prestamo)
        REFERENCES Prestamo(id_prestamo) ON DELETE SET NULL,
    CONSTRAINT FK_Alerta_Llave FOREIGN KEY (id_llave)
        REFERENCES Llave(id_llave) ON DELETE SET NULL
);
GO
CREATE INDEX IX_Alerta_Leida ON AlertaNotificacion(leida);
GO

-- ============================================================
-- DATOS SEMILLA (SEED DATA)
-- ============================================================

-- Tipos de ambiente
INSERT INTO TipoAmbiente (nombre_tipo) VALUES
('Oficina'),
('Laboratorio'),
('Depósito'),
('Área común'),
('Otro');
GO

-- Roles
INSERT INTO Rol (nombre_rol, descripcion, estado) VALUES
('Administrador', 'Acceso total al sistema. Gestiona usuarios, llaves y configuración.', 'A'),
('Operador',      'Registra préstamos, devoluciones y reservas de llaves.', 'A'),
('Consultor',     'Solo puede consultar el estado de las llaves y reportes.', 'A');
GO

-- Permisos
INSERT INTO Permisos (nombre_permiso, descripcion) VALUES
('ver_dashboard',       'Ver tablero principal'),
('gestionar_llaves',    'Crear, editar llaves'),
('gestionar_ambientes', 'Crear, editar ambientes'),
('gestionar_personas',  'Crear, editar personas'),
('gestionar_usuarios',  'Crear, editar usuarios del sistema'),
('registrar_prestamo',  'Registrar préstamo de llave'),
('registrar_devolucion','Registrar devolución de llave'),
('gestionar_reservas',  'Crear, cancelar reservas'),
('ver_auditoria',       'Ver registros de auditoría'),
('ver_reportes',        'Acceder a reportes');
GO

-- Asignar todos los permisos al Administrador
INSERT INTO RolPermisos (id_rol, id_permiso)
SELECT r.id_rol, p.id_permiso
FROM Rol r CROSS JOIN Permisos p
WHERE r.nombre_rol = 'Administrador';
GO

-- Permisos del Operador
INSERT INTO RolPermisos (id_rol, id_permiso)
SELECT r.id_rol, p.id_permiso
FROM Rol r JOIN Permisos p ON p.nombre_permiso IN (
    'ver_dashboard','registrar_prestamo','registrar_devolucion',
    'gestionar_reservas','ver_reportes'
)
WHERE r.nombre_rol = 'Operador';
GO

-- Permisos del Consultor
INSERT INTO RolPermisos (id_rol, id_permiso)
SELECT r.id_rol, p.id_permiso
FROM Rol r JOIN Permisos p ON p.nombre_permiso IN ('ver_dashboard','ver_reportes')
WHERE r.nombre_rol = 'Consultor';
GO

-- Persona administradora
INSERT INTO Persona (nombres, apellidos, ci, correo, estado)
VALUES ('Administrador', 'Sistema', '00000000', 'admin@upds.edu.bo', 'A');
GO

-- Usuario admin (password: Admin@1234 - hash BCrypt generado por la app)
-- NOTA: En producción la app aplica el hash automáticamente via SeedData.cs
INSERT INTO Usuario (id_persona, id_rol, nombre_usuario, password_hash, fecha_inicio, estado)
SELECT
    p.id_persona,
    r.id_rol,
    'admin',
    '$2a$11$placeholder_hash_generado_por_la_app',
    GETUTCDATE(),
    'A'
FROM Persona p CROSS JOIN Rol r
WHERE p.ci = '00000000' AND r.nombre_rol = 'Administrador';
GO

PRINT 'Base de datos GestionLlaves creada e inicializada correctamente.';
GO
