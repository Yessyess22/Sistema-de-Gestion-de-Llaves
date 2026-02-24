-- ============================================
-- SCRIPT SQL PARA CREAR TABLA PERSONAS
-- Base de Datos: PostgreSQL
-- ============================================

-- Crear tabla personas si no existe
CREATE TABLE IF NOT EXISTS personas (
    id_persona SERIAL PRIMARY KEY,
    nombres VARCHAR(150) NOT NULL,
    apellido_paterno VARCHAR(100) NOT NULL,
    apellido_materno VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    telefono VARCHAR(20) NOT NULL,
    fecha_nac DATE NOT NULL,
    tipo VARCHAR(20) NOT NULL,  -- Valores: 'Documento' o 'Empresa'
    codigo VARCHAR(50) NOT NULL,
    ci VARCHAR(30) NOT NULL UNIQUE,
    
    -- Campos de auditoría (OPCIONALES)
    -- created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    -- updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    -- created_by VARCHAR(100),
    -- updated_by VARCHAR(100)
);

-- ============================================
-- CREAR ÍNDICES PARA MEJORAR RENDIMIENTO
-- ============================================

-- Índice en email (única, para búsquedas rápidas)
CREATE UNIQUE INDEX IF NOT EXISTS idx_personas_email ON personas(email);

-- Índice en CI (única, para búsquedas rápidas)
CREATE UNIQUE INDEX IF NOT EXISTS idx_personas_ci ON personas(ci);

-- Índice en código (para búsquedas)
CREATE INDEX IF NOT EXISTS idx_personas_codigo ON personas(codigo);

-- Índice en nombres (para búsquedas por nombre)
CREATE INDEX IF NOT EXISTS idx_personas_nombres ON personas(nombres);

-- Índice en tipo (para filtrar por tipo)
CREATE INDEX IF NOT EXISTS idx_personas_tipo ON personas(tipo);

-- ============================================
-- DATOS DE PRUEBA (OPCIONAL)
-- ============================================

-- Insertar datos de prueba (descomente si desea)
/*
INSERT INTO personas (nombres, apellido_paterno, apellido_materno, email, telefono, fecha_nac, tipo, codigo, ci)
VALUES 
    ('Juan Carlos', 'García', 'López', 'juan.garcia@ejemplo.com', '+591 76543210', '1990-05-15', 'Documento', 'PERS001', '1234567'),
    ('María Elena', 'Rodríguez', 'Martínez', 'maria.rodriguez@ejemplo.com', '+591 71234567', '1992-08-22', 'Documento', 'PERS002', '1234568'),
    ('Tech Solutions S.A.', 'Empresa', 'Empresa', 'info@techsolutions.com', '+591 76666666', '2010-01-01', 'Empresa', 'EMP001', 'NIT001');
*/

-- ============================================
-- VERIFICACIÓN DE ESTRUCTURA
-- ============================================

-- Ver estructura de la tabla
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'personas'
ORDER BY ordinal_position;

-- Contar registros
SELECT COUNT(*) as total_personas FROM personas;

-- ============================================
-- COMANDOS ÚTILES PARA ADMINISTRACIÓN
-- ============================================

-- Eliminar todos los registros (CUIDADO - irreversible)
-- DELETE FROM personas;

-- Eliminar la tabla completa (CUIDADO - irreversible)
-- DROP TABLE IF EXISTS personas;

-- Truncar tabla (resetear secuencia)
-- TRUNCATE TABLE personas RESTART IDENTITY;
