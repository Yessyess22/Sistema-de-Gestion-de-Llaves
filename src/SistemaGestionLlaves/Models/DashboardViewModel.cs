using System;
using System.Collections.Generic;

namespace SistemaGestionLlaves.Models
{
    public class DashboardViewModel
    {
        // KPIs
        public int TotalLlaves { get; set; }
        public int PrestamosActivos { get; set; }
        public int ReservasHoy { get; set; }
        public int PersonasRegistradas { get; set; }

        // Actividad Reciente y Alertas
        public List<ActividadRecienteDto> ActividadReciente { get; set; } = new List<ActividadRecienteDto>();
        public List<AlertaCriticaDto> AlertasCriticas { get; set; } = new List<AlertaCriticaDto>();

        // Datos para Gráficos
        public List<UsoTipoAmbienteDto> UsoPorTipo { get; set; } = new List<UsoTipoAmbienteDto>();
        public List<HistoricoChartDto> HistoricoPrestamos { get; set; } = new List<HistoricoChartDto>();
    }

    public class ActividadRecienteDto
    {
        public string PersonaNombre { get; set; } = string.Empty;
        public string PersonaTipo { get; set; } = string.Empty;
        public string LlaveCodigo { get; set; } = string.Empty;
        public string AmbienteNombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string TipoAccion { get; set; } = string.Empty; // "Prestamo" o "Devolucion"
    }

    public class UsoTipoAmbienteDto
    {
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class HistoricoChartDto
    {
        public string Mes { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class AlertaCriticaDto
    {
        public int IdPrestamo { get; set; }
        public string PersonaNombre { get; set; } = string.Empty;
        public string LlaveCodigo { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public int HorasRetraso { get; set; }
    }
}
