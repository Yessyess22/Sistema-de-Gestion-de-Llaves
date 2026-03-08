using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaGestionLlaves.Data;

namespace SistemaGestionLlaves.Controllers;

[Authorize]
public class ReportesController : Controller
{
    private readonly ApplicationDbContext _context;

    private sealed class ReservaReporteItem
    {
        public int IdReserva { get; set; }
        public string Persona { get; set; } = string.Empty;
        public string Llave { get; set; } = string.Empty;
        public string Ambiente { get; set; } = "-";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    private sealed class PrestamoReporteItem
    {
        public int IdPrestamo { get; set; }
        public string Persona { get; set; } = string.Empty;
        public string Llave { get; set; } = string.Empty;
        public string Ambiente { get; set; } = "-";
        public DateTime FechaHoraPrestamo { get; set; }
        public DateTime? FechaHoraDevolucionReal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    private sealed class TopPersonaReporteItem
    {
        public int IdPersona { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Prestamos { get; set; }
        public int Reservas { get; set; }
        public int Total { get; set; }
    }

    private sealed class InventarioLlaveItem
    {
        public int IdLlave { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Ambiente { get; set; } = "-";
        public bool EsMaestra { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int TotalPrestamos { get; set; }
        public string UltimaPersona { get; set; } = "-";
        public DateTime? UltimaFecha { get; set; }
    }

    private sealed class PrestamoVencidoItem
    {
        public int IdPrestamo { get; set; }
        public string Persona { get; set; } = string.Empty;
        public string Ci { get; set; } = string.Empty;
        public string Celular { get; set; } = "-";
        public string Llave { get; set; } = string.Empty;
        public string Ambiente { get; set; } = "-";
        public DateTime FechaHoraPrestamo { get; set; }
        public DateTime FechaEsperada { get; set; }
        public int DiasVencido { get; set; }
    }

    private sealed class ActividadAmbienteItem
    {
        public int IdAmbiente { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Ambiente { get; set; } = string.Empty;
        public int TotalPrestamos { get; set; }
        public int TotalReservas { get; set; }
        public int TotalActividad { get; set; }
        public double PromedioHorasPrestamo { get; set; }
    }

    private sealed class HistorialPersonaItem
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty; // Prestamo o Reserva
        public string Llave { get; set; } = string.Empty;
        public string Ambiente { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    private sealed class DisponibilidadTipoItem
    {
        public string TipoAmbiente { get; set; } = string.Empty;
        public int TotalLlaves { get; set; }
        public int Disponibles { get; set; }
        public int Prestadas { get; set; }
        public int Reservadas { get; set; }
        public double PorcentajeDisponibilidad { get; set; }
    }

    private sealed class AnalisisTemporalItem
    {
        public string Etiqueta { get; set; } = string.Empty; // Mes o Día
        public int Cantidad { get; set; }
    }

    public ReportesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Reservas()
    {
        await CargarFiltrosAsync();
        return View();
    }

    public async Task<IActionResult> Prestamos()
    {
        await CargarFiltrosAsync();
        return View();
    }

    public IActionResult TopPersonas()
    {
        return View();
    }

    public async Task<IActionResult> InventarioLlaves()
    {
        await CargarFiltrosAsync();
        return View();
    }

    public async Task<IActionResult> PrestamosVencidos()
    {
        await CargarFiltrosAsync();
        return View();
    }

    public IActionResult ActividadAmbiente()
    {
        return View();
    }

    public async Task<IActionResult> HistorialPersona()
    {
        await CargarFiltrosAsync();
        return View();
    }

    public async Task<IActionResult> DisponibilidadTipo()
    {
        await CargarFiltrosAsync();
        return View();
    }

    public IActionResult AnalisisTemporal()
    {
        return View();
    }

    private async Task CargarFiltrosAsync()
    {
        ViewData["Ambientes"] = new SelectList(
            await _context.Ambientes
                .Where(a => a.Estado == "A")
                .OrderBy(a => a.Nombre)
                .Select(a => new { a.IdAmbiente, Nombre = a.Codigo + " — " + a.Nombre })
                .ToListAsync(),
            "IdAmbiente", "Nombre");

        ViewData["Llaves"] = new SelectList(
            await _context.Llaves
                .Where(l => l.Estado != "I")
                .OrderBy(l => l.Codigo)
                .Select(l => new { l.IdLlave, Nombre = l.Codigo })
                .ToListAsync(),
            "IdLlave", "Nombre");

        ViewData["Personas"] = new SelectList(
            await _context.Personas
                .OrderBy(p => p.Apellidos)
                .Select(p => new { p.IdPersona, Nombre = p.Apellidos + ", " + p.Nombres + " (" + p.Ci + ")" })
                .ToListAsync(),
            "IdPersona", "Nombre");

        ViewData["TiposAmbiente"] = new SelectList(
            await _context.TiposAmbiente
                .OrderBy(t => t.NombreTipo)
                .ToListAsync(),
            "IdTipo", "NombreTipo");
    }

    [HttpGet]
    public async Task<IActionResult> ReservasData(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente)
    {
        var data = await ObtenerReservasAsync(fechaDesde, fechaHasta, idAmbiente);

        return Ok(new
        {
            total = data.Count,
            data
        });
    }

    [HttpGet]
    public async Task<IActionResult> PrestamosData(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente, string? estado)
    {
        var data = await ObtenerPrestamosAsync(fechaDesde, fechaHasta, idAmbiente, estado);

        return Ok(new
        {
            total = data.Count,
            activos = data.Count(x => x.Estado == "A"),
            devueltos = data.Count(x => x.Estado == "D"),
            vencidos = data.Count(x => x.Estado == "V"),
            cancelados = data.Count(x => x.Estado == "C"),
            data
        });
    }

    [HttpGet]
    public async Task<IActionResult> TopPersonasData(DateTime? fechaDesde, DateTime? fechaHasta, int top = 10)
    {
        var ranking = await ObtenerTopPersonasAsync(fechaDesde, fechaHasta, top);

        return Ok(new
        {
            totalSolicitudes = ranking.Sum(x => x.Total),
            data = ranking
        });
    }

    [HttpGet]
    public async Task<IActionResult> InventarioLlavesData(int? idAmbiente, string? estado)
    {
        var data = await ObtenerInventarioLlavesAsync(idAmbiente, estado);
        return Ok(new { total = data.Count, data });
    }

    [HttpGet]
    public async Task<IActionResult> PrestamosVencidosData(int? idAmbiente)
    {
        var data = await ObtenerPrestamosVencidosAsync(idAmbiente);
        return Ok(new { total = data.Count, data });
    }

    [HttpGet]
    public async Task<IActionResult> ActividadAmbienteData(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var data = await ObtenerActividadAmbienteAsync(fechaDesde, fechaHasta);
        return Ok(new { totalActividad = data.Sum(x => x.TotalActividad), data });
    }

    [HttpGet]
    public async Task<IActionResult> HistorialPersonaData(int? idPersona, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        if (!idPersona.HasValue) return BadRequest("Debe seleccionar una persona.");
        
        if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde > fechaHasta)
            return BadRequest("La fecha inicial no puede ser posterior a la fecha final.");

        var existe = await _context.Personas.AnyAsync(p => p.IdPersona == idPersona.Value);
        if (!existe) return NotFound("La persona seleccionada no existe.");

        var data = await ObtenerHistorialPersonaAsync(idPersona.Value, fechaDesde, fechaHasta);
        return Ok(new { total = data.Count, data });
    }

    [HttpGet]
    public async Task<IActionResult> DisponibilidadTipoData(int? idTipoAmbiente)
    {
        if (idTipoAmbiente.HasValue)
        {
            var existe = await _context.TiposAmbiente.AnyAsync(t => t.IdTipo == idTipoAmbiente.Value);
            if (!existe) return NotFound("El tipo de ambiente seleccionado no existe.");
        }

        var data = await ObtenerDisponibilidadTipoAsync(idTipoAmbiente);
        return Ok(new { total = data.Count, data });
    }

    [HttpGet]
    public async Task<IActionResult> AnalisisTemporalData(string basePeriodo = "mensual")
    {
        var data = await ObtenerAnalisisTemporalAsync(basePeriodo);
        return Ok(new { data });
    }

    [HttpGet]
    public async Task<IActionResult> ExportarReservasPdf(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente)
    {
        var data = await ObtenerReservasAsync(fechaDesde, fechaHasta, idAmbiente);
        var pdf = BaseReportPdf("Reporte de Reservas", container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(0.9f);
                    columns.RelativeColumn(2.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.8f);
                    columns.RelativeColumn(1.6f);
                    columns.RelativeColumn(1.6f);
                    columns.RelativeColumn(1.1f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("#");
                    header.Cell().Element(CellHeader).Text("Persona");
                    header.Cell().Element(CellHeader).Text("Llave");
                    header.Cell().Element(CellHeader).Text("Ambiente");
                    header.Cell().Element(CellHeader).Text("Inicio");
                    header.Cell().Element(CellHeader).Text("Fin");
                    header.Cell().Element(CellHeader).Text("Estado");
                });

                foreach (var item in data)
                {
                    table.Cell().Element(CellBody).Text($"#{item.IdReserva}");
                    table.Cell().Element(CellBody).Text(item.Persona);
                    table.Cell().Element(CellBody).Text(item.Llave);
                    table.Cell().Element(CellBody).Text(item.Ambiente);
                    table.Cell().Element(CellBody).Text(item.FechaInicio.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(CellBody).Text(item.FechaFin.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(CellBody).Text(TraducirEstadoReserva(item.Estado));
                }

                if (!data.Any())
                    table.Cell().ColumnSpan(7).Element(CellBody).AlignCenter().Text("Sin datos para el filtro seleccionado.");
            });
        }, $"Total: {data.Count} reserva(s)");

        return File(pdf, "application/pdf", $"reporte_reservas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPrestamosPdf(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente, string? estado)
    {
        var data = await ObtenerPrestamosAsync(fechaDesde, fechaHasta, idAmbiente, estado);
        var pdf = BaseReportPdf("Reporte de Préstamos", container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(0.9f);
                    columns.RelativeColumn(2.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.8f);
                    columns.RelativeColumn(1.7f);
                    columns.RelativeColumn(1.7f);
                    columns.RelativeColumn(1.1f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("#");
                    header.Cell().Element(CellHeader).Text("Persona");
                    header.Cell().Element(CellHeader).Text("Llave");
                    header.Cell().Element(CellHeader).Text("Ambiente");
                    header.Cell().Element(CellHeader).Text("Préstamo");
                    header.Cell().Element(CellHeader).Text("Devolución");
                    header.Cell().Element(CellHeader).Text("Estado");
                });

                foreach (var item in data)
                {
                    table.Cell().Element(CellBody).Text($"#{item.IdPrestamo}");
                    table.Cell().Element(CellBody).Text(item.Persona);
                    table.Cell().Element(CellBody).Text(item.Llave);
                    table.Cell().Element(CellBody).Text(item.Ambiente);
                    table.Cell().Element(CellBody).Text(item.FechaHoraPrestamo.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(CellBody).Text(item.FechaHoraDevolucionReal.HasValue ? item.FechaHoraDevolucionReal.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : "-");
                    table.Cell().Element(CellBody).Text(TraducirEstadoPrestamo(item.Estado));
                }

                if (!data.Any())
                    table.Cell().ColumnSpan(7).Element(CellBody).AlignCenter().Text("Sin datos para el filtro seleccionado.");
            });
        }, $"Total: {data.Count} préstamo(s)");

        return File(pdf, "application/pdf", $"reporte_prestamos_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarTopPersonasPdf(DateTime? fechaDesde, DateTime? fechaHasta, int top = 10)
    {
        var data = await ObtenerTopPersonasAsync(fechaDesde, fechaHasta, top);
        var pdf = BaseReportPdf("Top Personas Solicitantes", container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(0.8f);
                    columns.RelativeColumn(2.8f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("#");
                    header.Cell().Element(CellHeader).Text("Persona");
                    header.Cell().Element(CellHeader).Text("Préstamos");
                    header.Cell().Element(CellHeader).Text("Reservas");
                    header.Cell().Element(CellHeader).Text("Total");
                });

                for (var i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    table.Cell().Element(CellBody).Text((i + 1).ToString());
                    table.Cell().Element(CellBody).Text(item.Nombre);
                    table.Cell().Element(CellBody).Text(item.Prestamos.ToString());
                    table.Cell().Element(CellBody).Text(item.Reservas.ToString());
                    table.Cell().Element(CellBody).Text(item.Total.ToString());
                }

                if (!data.Any())
                    table.Cell().ColumnSpan(5).Element(CellBody).AlignCenter().Text("Sin datos para el filtro seleccionado.");
            });
        }, $"Total solicitudes: {data.Sum(x => x.Total)}");

        return File(pdf, "application/pdf", $"reporte_top_personas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarInventarioLlavesPdf(int? idAmbiente, string? estado)
    {
        var data = await ObtenerInventarioLlavesAsync(idAmbiente, estado);
        var pdf = BaseReportPdf("Inventario de Llaves", container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1f);
                    columns.RelativeColumn(2f);
                    columns.RelativeColumn(0.9f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1f);
                    columns.RelativeColumn(2.2f);
                    columns.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Código");
                    header.Cell().Element(CellHeader).Text("Ambiente");
                    header.Cell().Element(CellHeader).Text("Maestra");
                    header.Cell().Element(CellHeader).Text("Estado");
                    header.Cell().Element(CellHeader).Text("Préstamos");
                    header.Cell().Element(CellHeader).Text("Último Solicitante");
                    header.Cell().Element(CellHeader).Text("Última Fecha");
                });

                foreach (var item in data)
                {
                    table.Cell().Element(CellBody).Text(item.Codigo);
                    table.Cell().Element(CellBody).Text(item.Ambiente);
                    table.Cell().Element(CellBody).Text(item.EsMaestra ? "Sí" : "No");
                    table.Cell().Element(CellBody).Text(TraducirEstadoLlave(item.Estado));
                    table.Cell().Element(CellBody).Text(item.TotalPrestamos.ToString());
                    table.Cell().Element(CellBody).Text(item.UltimaPersona);
                    table.Cell().Element(CellBody).Text(item.UltimaFecha.HasValue ? item.UltimaFecha.Value.ToLocalTime().ToString("dd/MM/yyyy") : "-");
                }

                if (!data.Any())
                    table.Cell().ColumnSpan(7).Element(CellBody).AlignCenter().Text("Sin datos para el filtro seleccionado.");
            });
        }, $"Total: {data.Count} llave(s)");

        return File(pdf, "application/pdf", $"reporte_inventario_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPrestamosVencidosPdf(int? idAmbiente)
    {
        var data = await ObtenerPrestamosVencidosAsync(idAmbiente);
        var pdf = BaseReportPdf("Préstamos Vencidos / No Devueltos", container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(0.8f);
                    columns.RelativeColumn(2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(1f);
                    columns.RelativeColumn(1.8f);
                    columns.RelativeColumn(1.2f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("#");
                    header.Cell().Element(CellHeader).Text("Persona");
                    header.Cell().Element(CellHeader).Text("CI");
                    header.Cell().Element(CellHeader).Text("Celular");
                    header.Cell().Element(CellHeader).Text("Llave");
                    header.Cell().Element(CellHeader).Text("Vto. Esperado");
                    header.Cell().Element(CellHeader).Text("Días vencido");
                });

                foreach (var item in data)
                {
                    table.Cell().Element(CellBody).Text($"#{item.IdPrestamo}");
                    table.Cell().Element(CellBody).Text(item.Persona);
                    table.Cell().Element(CellBody).Text(item.Ci);
                    table.Cell().Element(CellBody).Text(item.Celular);
                    table.Cell().Element(CellBody).Text(item.Llave);
                    table.Cell().Element(CellBody).Text(item.FechaEsperada.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(CellBody).Text(item.DiasVencido == 0 ? "< 1 día" : $"{item.DiasVencido} día(s)");
                }

                if (!data.Any())
                    table.Cell().ColumnSpan(7).Element(CellBody).AlignCenter().Text("Sin préstamos vencidos.");
            });
        }, $"Total: {data.Count} préstamo(s) vencido(s)");

        return File(pdf, "application/pdf", $"reporte_vencidos_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarActividadAmbientePdf(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var data = await ObtenerActividadAmbienteAsync(fechaDesde, fechaHasta);
        var pdf = BaseReportPdf("Actividad por Ambiente", container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.2f);
                    columns.RelativeColumn(2.5f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(1.3f);
                    columns.RelativeColumn(2f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("Código");
                    header.Cell().Element(CellHeader).Text("Ambiente");
                    header.Cell().Element(CellHeader).Text("Préstamos");
                    header.Cell().Element(CellHeader).Text("Reservas");
                    header.Cell().Element(CellHeader).Text("Total");
                    header.Cell().Element(CellHeader).Text("Prom. Horas Préstamo");
                });

                foreach (var item in data)
                {
                    table.Cell().Element(CellBody).Text(item.Codigo);
                    table.Cell().Element(CellBody).Text(item.Ambiente);
                    table.Cell().Element(CellBody).Text(item.TotalPrestamos.ToString());
                    table.Cell().Element(CellBody).Text(item.TotalReservas.ToString());
                    table.Cell().Element(CellBody).Text(item.TotalActividad.ToString());
                    table.Cell().Element(CellBody).Text(item.PromedioHorasPrestamo > 0 ? $"{item.PromedioHorasPrestamo} h" : "—");
                }

                if (!data.Any())
                    table.Cell().ColumnSpan(6).Element(CellBody).AlignCenter().Text("Sin actividad para el periodo seleccionado.");
            });
        }, $"Total actividad: {data.Sum(x => x.TotalActividad)} | Ambientes: {data.Count}");

        return File(pdf, "application/pdf", $"reporte_actividad_ambiente_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarHistorialPersonaPdf(int idPersona, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        if (fechaDesde.HasValue && fechaHasta.HasValue && fechaDesde > fechaHasta)
            return View("Error", new { mensaje = "Rango de fechas inválido." });

        var persona = await _context.Personas.FindAsync(idPersona);
        if (persona == null) return NotFound();

        var nombrePersona = $"{persona.Apellidos}, {persona.Nombres}";
        var data = await ObtenerHistorialPersonaAsync(idPersona, fechaDesde, fechaHasta);

        var pdf = BaseReportPdf("Historial de Actividad por Persona", container =>
        {
            container.Column(col =>
            {
                col.Item().Text($"Persona: {nombrePersona}").FontSize(12).FontColor(Colors.Grey.Darken3);
                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.8f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1.1f);
                        columns.RelativeColumn(1.8f);
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeader).Text("Fecha");
                        header.Cell().Element(CellHeader).Text("Tipo");
                        header.Cell().Element(CellHeader).Text("Llave");
                        header.Cell().Element(CellHeader).Text("Ambiente");
                        header.Cell().Element(CellHeader).Text("Detalle");
                        header.Cell().Element(CellHeader).Text("Estado");
                    });

                    foreach (var item in data)
                    {
                        table.Cell().Element(CellBody).Text(item.Fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                        table.Cell().Element(CellBody).Text(item.Tipo);
                        table.Cell().Element(CellBody).Text(item.Llave);
                        table.Cell().Element(CellBody).Text(item.Ambiente);
                        table.Cell().Element(CellBody).Text(item.Detalle);
                        table.Cell().Element(CellBody).Text(item.Tipo == "Préstamo" ? TraducirEstadoPrestamo(item.Estado) : TraducirEstadoReserva(item.Estado));
                    }

                    if (!data.Any())
                        table.Cell().ColumnSpan(6).Element(CellBody).AlignCenter().Text("Sin actividad registrada.");
                });
            });
        }, $"Total registros: {data.Count}");

        return File(pdf, "application/pdf", $"historial_{idPersona}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarDisponibilidadTipoPdf(int? idTipoAmbiente)
    {
        var tipoFiltro = "General";
        if (idTipoAmbiente.HasValue)
        {
            var t = await _context.TiposAmbiente.FindAsync(idTipoAmbiente.Value);
            if (t == null) return NotFound();
            tipoFiltro = t.NombreTipo;
        }
        
        var data = await ObtenerDisponibilidadTipoAsync(idTipoAmbiente);
        var pdf = BaseReportPdf("Reporte de Disponibilidad de Llaves", container =>
        {
            container.Column(col =>
            {
                col.Item().Text($"Filtro: {tipoFiltro}").FontSize(12).FontColor(Colors.Grey.Darken3);
                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeader).Text("Tipo Ambiente");
                        header.Cell().Element(CellHeader).Text("Total");
                        header.Cell().Element(CellHeader).Text("Disp.");
                        header.Cell().Element(CellHeader).Text("Prest.");
                        header.Cell().Element(CellHeader).Text("Reser.");
                        header.Cell().Element(CellHeader).Text("% Disp.");
                    });

                    foreach (var item in data)
                    {
                        table.Cell().Element(CellBody).Text(item.TipoAmbiente);
                        table.Cell().Element(CellBody).AlignCenter().Text(item.TotalLlaves.ToString());
                        table.Cell().Element(CellBody).AlignCenter().Text(item.Disponibles.ToString());
                        table.Cell().Element(CellBody).AlignCenter().Text(item.Prestadas.ToString());
                        table.Cell().Element(CellBody).AlignCenter().Text(item.Reservadas.ToString());
                        table.Cell().Element(CellBody).AlignCenter().Text($"{item.PorcentajeDisponibilidad}%");
                    }

                    if (!data.Any())
                        table.Cell().ColumnSpan(6).Element(CellBody).AlignCenter().Text("Sin datos disponibles.");
                });
            });
        }, $"Disponibilidad al {DateTime.Now:dd/MM/yyyy}");

        return File(pdf, "application/pdf", $"disponibilidad_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private async Task<List<ReservaReporteItem>> ObtenerReservasAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente)
    {
        var query = _context.Reservas
            .Include(r => r.Persona)
            .Include(r => r.Llave)
                .ThenInclude(l => l.Ambiente)
            .AsQueryable();

        if (fechaDesde.HasValue)
        {
            var desde = DateTime.SpecifyKind(fechaDesde.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.FechaInicio >= desde);
        }

        if (fechaHasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(fechaHasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(r => r.FechaInicio <= hasta);
        }

        if (idAmbiente.HasValue)
            query = query.Where(r => r.Llave.IdAmbiente == idAmbiente.Value);

        return await query
            .OrderByDescending(r => r.FechaInicio)
            .Select(r => new ReservaReporteItem
            {
                IdReserva = r.IdReserva,
                Persona = r.Persona.Nombres + " " + r.Persona.Apellidos,
                FechaInicio = r.FechaInicio,
                FechaFin = r.FechaFin,
                Estado = r.Estado,
                Llave = r.Llave.Codigo,
                Ambiente = r.Llave.Ambiente != null ? r.Llave.Ambiente.Nombre : "-"
            })
            .ToListAsync();
    }

    private async Task<List<PrestamoReporteItem>> ObtenerPrestamosAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente, string? estado)
    {
        var query = _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Llave)
                .ThenInclude(l => l.Ambiente)
            .AsQueryable();

        if (fechaDesde.HasValue)
        {
            var desde = DateTime.SpecifyKind(fechaDesde.Value.Date, DateTimeKind.Utc);
            query = query.Where(p => p.FechaHoraPrestamo >= desde);
        }

        if (fechaHasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(fechaHasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(p => p.FechaHoraPrestamo <= hasta);
        }

        if (idAmbiente.HasValue)
            query = query.Where(p => p.Llave.IdAmbiente == idAmbiente.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.Estado == estado);

        return await query
            .OrderByDescending(p => p.FechaHoraPrestamo)
            .Select(p => new PrestamoReporteItem
            {
                IdPrestamo = p.IdPrestamo,
                Persona = p.Persona.Nombres + " " + p.Persona.Apellidos,
                FechaHoraPrestamo = p.FechaHoraPrestamo,
                FechaHoraDevolucionReal = p.FechaHoraDevolucionReal,
                Estado = p.Estado,
                Llave = p.Llave.Codigo,
                Ambiente = p.Llave.Ambiente != null ? p.Llave.Ambiente.Nombre : "-"
            })
            .ToListAsync();
    }

    private async Task<List<TopPersonaReporteItem>> ObtenerTopPersonasAsync(DateTime? fechaDesde, DateTime? fechaHasta, int top)
    {
        var prestamos = _context.Prestamos.AsQueryable();
        var reservas = _context.Reservas.AsQueryable();

        if (fechaDesde.HasValue)
        {
            var desde = DateTime.SpecifyKind(fechaDesde.Value.Date, DateTimeKind.Utc);
            prestamos = prestamos.Where(p => p.FechaHoraPrestamo >= desde);
            reservas = reservas.Where(r => r.FechaInicio >= desde);
        }

        if (fechaHasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(fechaHasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            prestamos = prestamos.Where(p => p.FechaHoraPrestamo <= hasta);
            reservas = reservas.Where(r => r.FechaInicio <= hasta);
        }

        var topPrestamos = await prestamos
            .GroupBy(p => p.IdPersona)
            .Select(g => new { IdPersona = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        var topReservas = await reservas
            .GroupBy(r => r.IdPersona)
            .Select(g => new { IdPersona = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        var personas = await _context.Personas
            .Select(p => new { p.IdPersona, Nombre = p.Nombres + " " + p.Apellidos })
            .ToDictionaryAsync(p => p.IdPersona, p => p.Nombre);

        return topPrestamos
            .Select(p => new TopPersonaReporteItem
            {
                IdPersona = p.IdPersona,
                Prestamos = p.Cantidad,
                Reservas = topReservas.FirstOrDefault(r => r.IdPersona == p.IdPersona)?.Cantidad ?? 0
            })
            .Concat(
                topReservas
                    .Where(r => !topPrestamos.Any(p => p.IdPersona == r.IdPersona))
                    .Select(r => new TopPersonaReporteItem
                    {
                        IdPersona = r.IdPersona,
                        Prestamos = 0,
                        Reservas = r.Cantidad
                    })
            )
            .Select(x => new TopPersonaReporteItem
            {
                IdPersona = x.IdPersona,
                Nombre = personas.ContainsKey(x.IdPersona) ? personas[x.IdPersona] : "Persona no encontrada",
                Prestamos = x.Prestamos,
                Reservas = x.Reservas,
                Total = x.Prestamos + x.Reservas
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Nombre)
            .Take(top)
            .ToList();
    }

    private async Task<List<InventarioLlaveItem>> ObtenerInventarioLlavesAsync(int? idAmbiente, string? estado)
    {
        var query = _context.Llaves
            .Include(l => l.Ambiente)
            .AsQueryable();

        if (idAmbiente.HasValue)
            query = query.Where(l => l.IdAmbiente == idAmbiente.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(l => l.Estado == estado);
        else
            query = query.Where(l => l.Estado != "I");

        var llaves = await query.OrderBy(l => l.Codigo).ToListAsync();
        var idLlaves = llaves.Select(l => l.IdLlave).ToList();

        var conteoPrestamos = await _context.Prestamos
            .Where(p => idLlaves.Contains(p.IdLlave))
            .GroupBy(p => p.IdLlave)
            .Select(g => new { IdLlave = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.IdLlave, x => x.Total);

        var ultimosPrestamos = await _context.Prestamos
            .Include(p => p.Persona)
            .Where(p => idLlaves.Contains(p.IdLlave))
            .OrderByDescending(p => p.FechaHoraPrestamo)
            .ToListAsync();

        var ultimosPorLlave = ultimosPrestamos
            .GroupBy(p => p.IdLlave)
            .ToDictionary(g => g.Key, g => g.First());

        return llaves.Select(l =>
        {
            ultimosPorLlave.TryGetValue(l.IdLlave, out var ultimo);
            return new InventarioLlaveItem
            {
                IdLlave = l.IdLlave,
                Codigo = l.Codigo,
                Ambiente = l.Ambiente?.Nombre ?? "-",
                EsMaestra = l.EsMaestra,
                Estado = l.Estado,
                TotalPrestamos = conteoPrestamos.GetValueOrDefault(l.IdLlave, 0),
                UltimaPersona = ultimo != null ? ultimo.Persona.Nombres + " " + ultimo.Persona.Apellidos : "-",
                UltimaFecha = ultimo?.FechaHoraPrestamo
            };
        }).ToList();
    }

    private async Task<List<PrestamoVencidoItem>> ObtenerPrestamosVencidosAsync(int? idAmbiente)
    {
        var ahora = DateTime.UtcNow;

        var query = _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Llave)
                .ThenInclude(l => l.Ambiente)
            .Where(p => p.Estado == "A"
                     && p.FechaHoraDevolucionEsperada.HasValue
                     && p.FechaHoraDevolucionEsperada.Value < ahora)
            .AsQueryable();

        if (idAmbiente.HasValue)
            query = query.Where(p => p.Llave.IdAmbiente == idAmbiente.Value);

        var prestamos = await query.OrderBy(p => p.FechaHoraDevolucionEsperada).ToListAsync();

        return prestamos.Select(p => new PrestamoVencidoItem
        {
            IdPrestamo = p.IdPrestamo,
            Persona = p.Persona.Nombres + " " + p.Persona.Apellidos,
            Ci = p.Persona.Ci,
            Celular = p.Persona.Celular ?? "-",
            Llave = p.Llave.Codigo,
            Ambiente = p.Llave.Ambiente?.Nombre ?? "-",
            FechaHoraPrestamo = p.FechaHoraPrestamo,
            FechaEsperada = p.FechaHoraDevolucionEsperada!.Value,
            DiasVencido = (int)(ahora - p.FechaHoraDevolucionEsperada!.Value).TotalDays
        }).ToList();
    }

    private async Task<List<ActividadAmbienteItem>> ObtenerActividadAmbienteAsync(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var prestamosQuery = _context.Prestamos.Include(p => p.Llave).AsQueryable();
        var reservasQuery = _context.Reservas.Include(r => r.Llave).AsQueryable();

        if (fechaDesde.HasValue)
        {
            var desde = DateTime.SpecifyKind(fechaDesde.Value.Date, DateTimeKind.Utc);
            prestamosQuery = prestamosQuery.Where(p => p.FechaHoraPrestamo >= desde);
            reservasQuery = reservasQuery.Where(r => r.FechaInicio >= desde);
        }

        if (fechaHasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(fechaHasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            prestamosQuery = prestamosQuery.Where(p => p.FechaHoraPrestamo <= hasta);
            reservasQuery = reservasQuery.Where(r => r.FechaInicio <= hasta);
        }

        var prestamos = await prestamosQuery.ToListAsync();
        var reservas = await reservasQuery.ToListAsync();

        var ambientes = await _context.Ambientes
            .Where(a => a.Estado == "A")
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        return ambientes.Select(a =>
        {
            var pres = prestamos.Where(p => p.Llave.IdAmbiente == a.IdAmbiente).ToList();
            var res = reservas.Where(r => r.Llave.IdAmbiente == a.IdAmbiente).ToList();
            var conDev = pres.Where(p => p.FechaHoraDevolucionReal.HasValue).ToList();
            var horasTotal = conDev.Sum(p => (p.FechaHoraDevolucionReal!.Value - p.FechaHoraPrestamo).TotalHours);

            return new ActividadAmbienteItem
            {
                IdAmbiente = a.IdAmbiente,
                Codigo = a.Codigo,
                Ambiente = a.Nombre,
                TotalPrestamos = pres.Count,
                TotalReservas = res.Count,
                TotalActividad = pres.Count + res.Count,
                PromedioHorasPrestamo = conDev.Count > 0 ? Math.Round(horasTotal / conDev.Count, 1) : 0
            };
        })
        .Where(x => x.TotalActividad > 0)
        .OrderByDescending(x => x.TotalActividad)
        .ToList();
    }

    private byte[] BaseReportPdf(string title, Action<IContainer> contentAction, string footerText)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text(title).SemiBold().FontSize(16);
                    col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(12).Element(contentAction);

                page.Footer().AlignRight().Text(footerText).FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();
    }

    private static IContainer CellHeader(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .PaddingVertical(6)
            .PaddingHorizontal(4)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten1)
            .DefaultTextStyle(x => x.SemiBold());
    }

    private static IContainer CellBody(IContainer container)
    {
        return container
            .PaddingVertical(5)
            .PaddingHorizontal(4)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3);
    }

    private async Task<List<HistorialPersonaItem>> ObtenerHistorialPersonaAsync(int idPersona, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var prestamos = _context.Prestamos
            .Include(p => p.Llave).ThenInclude(l => l.Ambiente)
            .Where(p => p.IdPersona == idPersona);

        var reservas = _context.Reservas
            .Include(r => r.Llave).ThenInclude(l => l.Ambiente)
            .Where(r => r.IdPersona == idPersona);

        if (fechaDesde.HasValue)
        {
            var desde = DateTime.SpecifyKind(fechaDesde.Value.Date, DateTimeKind.Utc);
            prestamos = prestamos.Where(p => p.FechaHoraPrestamo >= desde);
            reservas = reservas.Where(r => r.FechaInicio >= desde);
        }

        if (fechaHasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(fechaHasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            prestamos = prestamos.Where(p => p.FechaHoraPrestamo <= hasta);
            reservas = reservas.Where(r => r.FechaInicio <= hasta);
        }

        var pList = await prestamos.Select(p => new HistorialPersonaItem
        {
            Fecha = p.FechaHoraPrestamo,
            Tipo = "Préstamo",
            Llave = p.Llave.Codigo,
            Ambiente = p.Llave.Ambiente != null ? p.Llave.Ambiente.Nombre : "-",
            Estado = p.Estado,
            Detalle = p.FechaHoraDevolucionReal.HasValue ? $"Devuelto: {p.FechaHoraDevolucionReal.Value:dd/MM HH:mm}" : "Activo/Sin devolución"
        }).ToListAsync();

        var rList = await reservas.Select(r => new HistorialPersonaItem
        {
            Fecha = r.FechaInicio,
            Tipo = "Reserva",
            Llave = r.Llave.Codigo,
            Ambiente = r.Llave.Ambiente != null ? r.Llave.Ambiente.Nombre : "-",
            Estado = r.Estado,
            Detalle = $"Hasta: {r.FechaFin:dd/MM HH:mm}"
        }).ToListAsync();

        return pList.Concat(rList).OrderByDescending(x => x.Fecha).ToList();
    }

    private async Task<List<DisponibilidadTipoItem>> ObtenerDisponibilidadTipoAsync(int? idTipoAmbiente)
    {
        var query = _context.Ambientes.AsQueryable();
        if (idTipoAmbiente.HasValue)
            query = query.Where(a => a.IdTipo == idTipoAmbiente.Value);

        var ambientes = await query.Include(a => a.TipoAmbiente).ToListAsync();
        var idAmbientes = ambientes.Select(a => a.IdAmbiente).ToList();

        var llaves = await _context.Llaves
            .Where(l => idAmbientes.Contains(l.IdAmbiente) && l.Estado != "I")
            .ToListAsync();

        return ambientes.Select(a =>
        {
            var llavesAmbiente = llaves.Where(l => l.IdAmbiente == a.IdAmbiente).ToList();
            int total = llavesAmbiente.Count;
            int disp = llavesAmbiente.Count(l => l.Estado == "D");
            int pres = llavesAmbiente.Count(l => l.Estado == "P");
            int res = llavesAmbiente.Count(l => l.Estado == "R");

            return new DisponibilidadTipoItem
            {
                TipoAmbiente = a.TipoAmbiente?.NombreTipo ?? "Sin Tipo",
                TotalLlaves = total,
                Disponibles = disp,
                Prestadas = pres,
                Reservadas = res,
                PorcentajeDisponibilidad = total > 0 ? Math.Round((double)disp / total * 100, 1) : 0
            };
        })
        .Where(x => x.TotalLlaves > 0)
        .OrderByDescending(x => x.PorcentajeDisponibilidad)
        .ToList();
    }

    private async Task<List<AnalisisTemporalItem>> ObtenerAnalisisTemporalAsync(string basePeriodo)
    {
        if (basePeriodo == "dia")
        {
            var prestamos = await _context.Prestamos
                .Select(p => p.FechaHoraPrestamo)
                .ToListAsync();

            var dias = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            return prestamos
                .GroupBy(p => p.ToLocalTime().DayOfWeek)
                .Select(g => new AnalisisTemporalItem
                {
                    Etiqueta = dias[(int)g.Key],
                    Cantidad = g.Count()
                })
                .OrderBy(x => Array.IndexOf(dias, x.Etiqueta))
                .ToList();
        }
        else
        {
            // Mensual (últimos 12 meses)
            var haceUnAño = DateTime.UtcNow.AddMonths(-11);
            haceUnAño = new DateTime(haceUnAño.Year, haceUnAño.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var prestamos = await _context.Prestamos
                .Where(p => p.FechaHoraPrestamo >= haceUnAño)
                .Select(p => p.FechaHoraPrestamo)
                .ToListAsync();

            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            
            var resultado = new List<AnalisisTemporalItem>();
            for (int i = 0; i < 12; i++)
            {
                var fecha = haceUnAño.AddMonths(i);
                var etiqueta = $"{meses[fecha.Month - 1]} {fecha.Year % 100}";
                var count = prestamos.Count(p => p.Month == fecha.Month && p.Year == fecha.Year);
                resultado.Add(new AnalisisTemporalItem { Etiqueta = etiqueta, Cantidad = count });
            }
            return resultado;
        }
    }

    private static string TraducirEstadoReserva(string estado)
    {
        return estado switch
        {
            "P" => "Pendiente",
            "C" => "Confirmada",
            "U" => "Utilizada",
            "X" => "Cancelada",
            _ => estado
        };
    }

    private static string TraducirEstadoPrestamo(string estado)
    {
        return estado switch
        {
            "A" => "Activo",
            "D" => "Devuelto",
            "V" => "Vencido",
            "C" => "Cancelado",
            _ => estado
        };
    }

    private static string TraducirEstadoLlave(string estado)
    {
        return estado switch
        {
            "D" => "Disponible",
            "P" => "Prestada",
            "R" => "Reservada",
            "I" => "Inactiva",
            _ => estado
        };
    }
}
