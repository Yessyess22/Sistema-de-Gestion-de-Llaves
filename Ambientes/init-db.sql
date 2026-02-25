-- Script de inicialización de PostgreSQL
-- Este archivo se ejecuta automáticamente al crear el contenedor

-- Crear esquema si no existe
CREATE SCHEMA IF NOT EXISTS public;

-- Crear tabla de ambientes con comentarios
CREATE TABLE IF NOT EXISTS public.ambientes (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) NOT NULL UNIQUE,
    nombre VARCHAR(100) NOT NULL,
    tipo_ambiente VARCHAR(50) NOT NULL,
    ubicacion VARCHAR(100) NOT NULL,
    estado VARCHAR(30) NOT NULL,
    fecha_creacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_actualizacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Crear índices para mejorar el rendimiento
CREATE INDEX IF NOT EXISTS idx_ambiente_codigo_unique ON public.ambientes(codigo);
CREATE INDEX IF NOT EXISTS idx_ambiente_estado ON public.ambientes(estado);
CREATE INDEX IF NOT EXISTS idx_ambiente_nombre ON public.ambientes(nombre);

-- Crear tabla de auditoría (opcional, para futuras extensiones)
CREATE TABLE IF NOT EXISTS public.ambientes_auditoria (
    id SERIAL PRIMARY KEY,
    ambiente_id INT NOT NULL,
    accion VARCHAR(20) NOT NULL,
    datos_anteriores JSONB,
    datos_nuevos JSONB,
    fecha_cambio TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    usuario VARCHAR(100)
);

-- Crear función para actualizar fecha_actualizacion automáticamente
CREATE OR REPLACE FUNCTION public.actualizar_fecha_actualizacion()
RETURNS TRIGGER AS $$
BEGIN
    NEW.fecha_actualizacion = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Crear trigger para actualizar fecha_actualizacion
DROP TRIGGER IF EXISTS trigger_actualizar_fecha ON public.ambientes;
CREATE TRIGGER trigger_actualizar_fecha
    BEFORE UPDATE ON public.ambientes
    FOR EACH ROW
    EXECUTE FUNCTION public.actualizar_fecha_actualizacion();

-- Insertar datos de prueba (opcional)
INSERT INTO public.ambientes (codigo, nombre, tipo_ambiente, ubicacion, estado)
VALUES 
    ('LAB-001', 'Laboratorio de Informática 1', 'Laboratorio', 'Edificio A, Piso 2', 'Disponible'),
    ('AULA-101', 'Aula de Clases 101', 'Aula', 'Edificio A, Piso 1', 'Disponible'),
    ('CONF-001', 'Sala de Conferencias Principal', 'Sala de Conferencias', 'Edificio B, Piso 3', 'Ocupado'),
    ('LAB-002', 'Laboratorio de Electrónica', 'Laboratorio', 'Edificio A, Piso 3', 'Mantenimiento')
ON CONFLICT (codigo) DO NOTHING;

-- Comentarios en la tabla
COMMENT ON TABLE public.ambientes IS 'Tabla que almacena la información de los ambientes del sistema';
COMMENT ON COLUMN public.ambientes.id IS 'Identificador único del ambiente';
COMMENT ON COLUMN public.ambientes.codigo IS 'Código único que identifica al ambiente (ej: LAB-001, AULA-101)';
COMMENT ON COLUMN public.ambientes.nombre IS 'Nombre descriptivo del ambiente';
COMMENT ON COLUMN public.ambientes.tipo_ambiente IS 'Tipo de ambiente (Laboratorio, Aula, Sala de Conferencias, etc)';
COMMENT ON COLUMN public.ambientes.ubicacion IS 'Ubicación física del ambiente (Edificio, Piso, etc)';
COMMENT ON COLUMN public.ambientes.estado IS 'Estado actual (Disponible, Ocupado, Mantenimiento)';
COMMENT ON COLUMN public.ambientes.fecha_creacion IS 'Fecha y hora de creación del registro';
COMMENT ON COLUMN public.ambientes.fecha_actualizacion IS 'Fecha y hora de última actualización del registro';
