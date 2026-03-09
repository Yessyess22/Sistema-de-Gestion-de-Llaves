using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

    // ─── DTOs internos ───────────────────────────────────────

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

    // ─── Constructor ─────────────────────────────────────────

    public ReportesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ─── Vistas MVC ──────────────────────────────────────────

    public IActionResult Index() => View();

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

    public IActionResult TopPersonas() => View();

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

    public IActionResult ActividadAmbiente() => View();

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
    }

    // ─── Endpoints de datos (JSON) ───────────────────────────

    [HttpGet]
    public async Task<IActionResult> ReservasData(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente)
    {
        var data = await ObtenerReservasAsync(fechaDesde, fechaHasta, idAmbiente);
        return Ok(new { total = data.Count, data });
    }

    [HttpGet]
    public async Task<IActionResult> PrestamosData(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente, string? estado)
    {
        var data = await ObtenerPrestamosAsync(fechaDesde, fechaHasta, idAmbiente, estado);
        return Ok(new
        {
            total = data.Count,
            activos   = data.Count(x => x.Estado == "A"),
            devueltos = data.Count(x => x.Estado == "D"),
            vencidos  = data.Count(x => x.Estado == "V"),
            cancelados = data.Count(x => x.Estado == "C"),
            data
        });
    }

    [HttpGet]
    public async Task<IActionResult> TopPersonasData(DateTime? fechaDesde, DateTime? fechaHasta, int top = 10)
    {
        var ranking = await ObtenerTopPersonasAsync(fechaDesde, fechaHasta, top);
        return Ok(new { totalSolicitudes = ranking.Sum(x => x.Total), data = ranking });
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

    // ─── Exportadores PDF ────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> ExportarReservasPdf(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente)
    {
        var data    = await ObtenerReservasAsync(fechaDesde, fechaHasta, idAmbiente);
        var usuario = ObtenerUsuarioActual();
        var codigo  = GenerarCodigoSeguridad("Reservas", usuario);
        var logoPath = ObtenerRutaLogo();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => BuildHeader(c, logoPath,
                    "Reporte de Reservas",
                    "Reservas de llaves registradas en el sistema",
                    usuario, codigo, fechaDesde, fechaHasta));

                page.Content().PaddingTop(14).Table(table =>
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

                page.Footer().Element(c => BuildFooter(c, data.Count, "reserva(s)"));
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"reporte_reservas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPrestamosPdf(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente, string? estado)
    {
        var data    = await ObtenerPrestamosAsync(fechaDesde, fechaHasta, idAmbiente, estado);
        var usuario = ObtenerUsuarioActual();
        var codigo  = GenerarCodigoSeguridad("Prestamos", usuario);
        var logoPath = ObtenerRutaLogo();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => BuildHeader(c, logoPath,
                    "Reporte de Préstamos",
                    "Historial de préstamos de llaves",
                    usuario, codigo, fechaDesde, fechaHasta));

                page.Content().PaddingTop(14).Column(col =>
                {
                    // Resumen rápido
                    col.Item().PaddingBottom(8).Row(row =>
                    {
                        row.RelativeItem().Element(c => SummaryBox(c, "Total",     data.Count.ToString(),                        Colors.BlueGrey.Lighten4, Colors.BlueGrey.Darken2));
                        row.ConstantItem(6);
                        row.RelativeItem().Element(c => SummaryBox(c, "Activos",   data.Count(x => x.Estado == "A").ToString(), "#c8e6c9", "#2e7d32"));
                        row.ConstantItem(6);
                        row.RelativeItem().Element(c => SummaryBox(c, "Devueltos", data.Count(x => x.Estado == "D").ToString(), "#e3f2fd", "#1565c0"));
                        row.ConstantItem(6);
                        row.RelativeItem().Element(c => SummaryBox(c, "Vencidos",  data.Count(x => x.Estado == "V").ToString(), "#ffebee", "#c62828"));
                    });

                    col.Item().Table(table =>
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
                            table.Cell().Element(CellBody).Text(item.FechaHoraDevolucionReal.HasValue
                                ? item.FechaHoraDevolucionReal.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : "-");
                            table.Cell().Element(CellBody).Text(TraducirEstadoPrestamo(item.Estado));
                        }

                        if (!data.Any())
                            table.Cell().ColumnSpan(7).Element(CellBody).AlignCenter().Text("Sin datos para el filtro seleccionado.");
                    });
                });

                page.Footer().Element(c => BuildFooter(c, data.Count, "préstamo(s)"));
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"reporte_prestamos_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarTopPersonasPdf(DateTime? fechaDesde, DateTime? fechaHasta, int top = 10)
    {
        var data    = await ObtenerTopPersonasAsync(fechaDesde, fechaHasta, top);
        var usuario = ObtenerUsuarioActual();
        var codigo  = GenerarCodigoSeguridad("TopPersonas", usuario);
        var logoPath = ObtenerRutaLogo();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => BuildHeader(c, logoPath,
                    "Top Personas Solicitantes",
                    $"Ranking de las {top} personas con más actividad",
                    usuario, codigo, fechaDesde, fechaHasta));

                page.Content().PaddingTop(14).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(0.7f);
                        columns.RelativeColumn(3f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellHeader).Text("Pos.");
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

                page.Footer().Element(c => BuildFooter(c, data.Sum(x => x.Total), "solicitud(es) en total"));
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"reporte_top_personas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarInventarioLlavesPdf(int? idAmbiente, string? estado)
    {
        var data    = await ObtenerInventarioLlavesAsync(idAmbiente, estado);
        var usuario = ObtenerUsuarioActual();
        var codigo  = GenerarCodigoSeguridad("Inventario", usuario);
        var logoPath = ObtenerRutaLogo();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => BuildHeader(c, logoPath,
                    "Inventario de Llaves",
                    "Estado actual del inventario de llaves del sistema",
                    usuario, codigo, null, null));

                page.Content().PaddingTop(14).Column(col =>
                {
                    // Resumen por estado
                    col.Item().PaddingBottom(8).Row(row =>
                    {
                        row.RelativeItem().Element(c => SummaryBox(c, "Total",       data.Count.ToString(),                               Colors.BlueGrey.Lighten4, Colors.BlueGrey.Darken2));
                        row.ConstantItem(6);
                        row.RelativeItem().Element(c => SummaryBox(c, "Disponibles", data.Count(x => x.Estado == "D").ToString(), "#c8e6c9", "#2e7d32"));
                        row.ConstantItem(6);
                        row.RelativeItem().Element(c => SummaryBox(c, "Prestadas",   data.Count(x => x.Estado == "P").ToString(), "#fff9c4", "#f57f17"));
                        row.ConstantItem(6);
                        row.RelativeItem().Element(c => SummaryBox(c, "Maestras",    data.Count(x => x.EsMaestra).ToString(),              "#e8eaf6", "#283593"));
                    });

                    col.Item().Table(table =>
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
                            table.Cell().Element(CellBody).Text(item.UltimaFecha.HasValue
                                ? item.UltimaFecha.Value.ToLocalTime().ToString("dd/MM/yyyy") : "-");
                        }

                        if (!data.Any())
                            table.Cell().ColumnSpan(7).Element(CellBody).AlignCenter().Text("Sin datos para el filtro seleccionado.");
                    });
                });

                page.Footer().Element(c => BuildFooter(c, data.Count, "llave(s)"));
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"reporte_inventario_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPrestamosVencidosPdf(int? idAmbiente)
    {
        var data    = await ObtenerPrestamosVencidosAsync(idAmbiente);
        var usuario = ObtenerUsuarioActual();
        var codigo  = GenerarCodigoSeguridad("Vencidos", usuario);
        var logoPath = ObtenerRutaLogo();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => BuildHeader(c, logoPath,
                    "Préstamos Vencidos / No Devueltos",
                    "Llaves prestadas con plazo de devolución excedido",
                    usuario, codigo, null, null));

                page.Content().PaddingTop(14).Table(table =>
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
                        var diasText = item.DiasVencido == 0 ? "< 1 día" : $"{item.DiasVencido} día(s)";
                        table.Cell().Element(CellBody).Text($"#{item.IdPrestamo}");
                        table.Cell().Element(CellBody).Text(item.Persona);
                        table.Cell().Element(CellBody).Text(item.Ci);
                        table.Cell().Element(CellBody).Text(item.Celular);
                        table.Cell().Element(CellBody).Text(item.Llave);
                        table.Cell().Element(CellBody).Text(item.FechaEsperada.ToLocalTime().ToString("dd/MM/yyyy HH:mm"));
                        // Resaltar días vencidos en rojo si > 0
                        table.Cell().Element(CellBody)
                            .Text(diasText)
                            .FontColor(item.DiasVencido > 0 ? "#c62828" : Colors.Black);
                    }

                    if (!data.Any())
                        table.Cell().ColumnSpan(7).Element(CellBody).AlignCenter().Text("Sin préstamos vencidos. ¡Todo al día!");
                });

                page.Footer().Element(c => BuildFooter(c, data.Count, "préstamo(s) vencido(s)"));
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"reporte_vencidos_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarActividadAmbientePdf(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var data    = await ObtenerActividadAmbienteAsync(fechaDesde, fechaHasta);
        var usuario = ObtenerUsuarioActual();
        var codigo  = GenerarCodigoSeguridad("Actividad", usuario);
        var logoPath = ObtenerRutaLogo();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => BuildHeader(c, logoPath,
                    "Actividad por Ambiente",
                    "Préstamos y reservas agrupados por ambiente",
                    usuario, codigo, fechaDesde, fechaHasta));

                page.Content().PaddingTop(14).Column(col =>
                {
                    // Resumen
                    col.Item().PaddingBottom(8).Row(row =>
                    {
                        row.RelativeItem().Element(c => SummaryBox(c, "Ambientes",  data.Count.ToString(),                         Colors.BlueGrey.Lighten4, Colors.BlueGrey.Darken2));
                        row.ConstantItem(6);
                        row.RelativeItem().Element(c => SummaryBox(c, "Préstamos",  data.Sum(x => x.TotalPrestamos).ToString(),  "#c8e6c9", "#2e7d32"));
                        row.ConstantItem(6);
                        row.RelativeItem().Element(c => SummaryBox(c, "Reservas",   data.Sum(x => x.TotalReservas).ToString(),   "#e3f2fd", "#1565c0"));
                        row.ConstantItem(6);
                        row.RelativeItem().Element(c => SummaryBox(c, "Actividad Total", data.Sum(x => x.TotalActividad).ToString(), "#f3e5f5", "#6a1b9a"));
                    });

                    col.Item().Table(table =>
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
                            table.Cell().Element(CellBody).Text(item.PromedioHorasPrestamo > 0
                                ? $"{item.PromedioHorasPrestamo} h" : "—");
                        }

                        if (!data.Any())
                            table.Cell().ColumnSpan(6).Element(CellBody).AlignCenter().Text("Sin actividad para el periodo seleccionado.");
                    });
                });

                page.Footer().Element(c => BuildFooter(c, data.Sum(x => x.TotalActividad), "actividades registradas"));
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"reporte_actividad_ambiente_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    // ─── Helpers de diseño PDF ───────────────────────────────

    /// <summary>Encabezado institucional con logo, título, filtros, usuario y código de seguridad.</summary>
    private static void BuildHeader(
        IContainer container,
        string? logoPath,
        string titulo,
        string subtitulo,
        string usuario,
        string codigoSeguridad,
        DateTime? desde,
        DateTime? hasta)
    {
        container.Column(col =>
        {
            // Franja azul institucional
            col.Item()
               .Background("#0d3b7c")
               .Padding(12)
               .Row(row =>
               {
                   // Logo
                   if (logoPath != null && System.IO.File.Exists(logoPath))
                   {
                       row.ConstantItem(56).Image(logoPath).FitArea();
                       row.ConstantItem(12);
                   }

                   // Título e institución
                   row.RelativeItem().Column(inner =>
                   {
                       inner.Item().Text("KeyFlow Inc.")
                                   .FontSize(8).FontColor(Colors.White).Italic();
                       inner.Item().Text("Sistema de Gestión de Llaves")
                                   .FontSize(8).FontColor("#90caf9").Italic();
                       inner.Item().PaddingTop(4).Text(titulo)
                                   .FontSize(14).SemiBold().FontColor(Colors.White);
                       inner.Item().Text(subtitulo)
                                   .FontSize(8).FontColor("#b0c4de");
                   });

                   // Bloque derecho: fecha y datos del reporte
                   row.ConstantItem(130).AlignRight().Column(inner =>
                   {
                       inner.Item().AlignRight().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}")
                                   .FontSize(8).FontColor(Colors.White);
                       inner.Item().AlignRight().Text($"Hora:  {DateTime.Now:HH:mm}")
                                   .FontSize(8).FontColor(Colors.White);
                       inner.Item().AlignRight().Text($"Imprime: {usuario}")
                                   .FontSize(8).FontColor("#b0c4de");
                       if (desde.HasValue || hasta.HasValue)
                       {
                           var rango = $"{(desde.HasValue ? desde.Value.ToString("dd/MM/yyyy") : "—")} → {(hasta.HasValue ? hasta.Value.ToString("dd/MM/yyyy") : "—")}";
                           inner.Item().AlignRight().Text($"Período: {rango}")
                                       .FontSize(7).FontColor("#90caf9");
                       }
                   });
               });

            // Línea de código de seguridad
            col.Item()
               .Background("#1a4f9c")
               .PaddingHorizontal(12)
               .PaddingVertical(3)
               .Row(row =>
               {
                   row.RelativeItem().Text($"Cód. Seguridad: {codigoSeguridad}")
                                     .FontSize(7).FontColor("#b0c4de").Italic();
                   row.AutoItem().Text("Documento generado automáticamente — No válido sin firma electrónica")
                                 .FontSize(7).FontColor("#90caf9").Italic();
               });
        });
    }

    /// <summary>Pie de página con número de página, total de registros y marca de agua.</summary>
    private static void BuildFooter(IContainer container, int total, string unidad)
    {
        container
            .BorderTop(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingTop(6)
            .Row(row =>
            {
                row.RelativeItem().Text(x =>
                {
                    x.Span("KeyFlow — Sistema de Gestión  ").FontColor(Colors.Grey.Darken1).FontSize(8);
                });

                row.ConstantItem(100).AlignCenter()
                   .Text($"Total: {total} {unidad}")
                   .FontSize(8).FontColor(Colors.Grey.Darken2).SemiBold();

                row.RelativeItem().AlignRight().Text(x =>
                {
                    x.Span("Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken2).SemiBold();
                    x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken2).SemiBold();
                });
            });
    }

    /// <summary>Caja de resumen coloreada para métricas rápidas.</summary>
    private static void SummaryBox(IContainer container, string label, string value, string bgColor, string textColor)
    {
        container
            .Background(bgColor)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(6)
            .Column(col =>
            {
                col.Item().AlignCenter().Text(value).FontSize(14).SemiBold().FontColor(textColor);
                col.Item().AlignCenter().Text(label).FontSize(8).FontColor(textColor).Italic();
            });
    }

    // ─── Estilos de celda ────────────────────────────────────

    private static IContainer CellHeader(IContainer container) =>
        container
            .Background("#e8edf4")
            .PaddingVertical(6)
            .PaddingHorizontal(4)
            .BorderBottom(1)
            .BorderColor("#c5cdd9")
            .DefaultTextStyle(x => x.SemiBold().FontColor("#0d3b7c"));

    private static IContainer CellBody(IContainer container) =>
        container
            .PaddingVertical(5)
            .PaddingHorizontal(4)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3);

    // ─── Utilidades ──────────────────────────────────────────

    private string ObtenerUsuarioActual() =>
        User.FindFirstValue(ClaimTypes.Name) ?? "Sistema";

    private string ObtenerRutaLogo()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logo.png");
        return System.IO.File.Exists(path) ? path : string.Empty;
    }

    /// <summary>
    /// Genera un código de seguridad HMAC-SHA256 basado en el nombre del reporte,
    /// la fecha actual y el usuario que imprime. Permite detectar alteraciones.
    /// </summary>
    private static string GenerarCodigoSeguridad(string nombreReporte, string usuario)
    {
        const string secretKey = "KeyFlow-2024-SecretKey";
        var payload = $"{nombreReporte}|{usuario}|{DateTime.UtcNow:yyyyMMddHH}";
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);

        // Formato legible: 4 grupos de 4 caracteres hex mayúsculas
        var hex = Convert.ToHexString(hash)[..16];
        return $"{hex[..4]}-{hex[4..8]}-{hex[8..12]}-{hex[12..16]}";
    }

    // ─── Queries de datos ────────────────────────────────────

    private async Task<List<ReservaReporteItem>> ObtenerReservasAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente)
    {
        var query = _context.Reservas
            .Include(r => r.Persona)
            .Include(r => r.Llave).ThenInclude(l => l.Ambiente)
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
                IdReserva   = r.IdReserva,
                Persona     = r.Persona.Nombres + " " + r.Persona.Apellidos,
                FechaInicio = r.FechaInicio,
                FechaFin    = r.FechaFin,
                Estado      = r.Estado,
                Llave       = r.Llave.Codigo,
                Ambiente    = r.Llave.Ambiente != null ? r.Llave.Ambiente.Nombre : "-"
            })
            .ToListAsync();
    }

    private async Task<List<PrestamoReporteItem>> ObtenerPrestamosAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? idAmbiente, string? estado)
    {
        var query = _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Llave).ThenInclude(l => l.Ambiente)
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
                IdPrestamo            = p.IdPrestamo,
                Persona               = p.Persona.Nombres + " " + p.Persona.Apellidos,
                FechaHoraPrestamo     = p.FechaHoraPrestamo,
                FechaHoraDevolucionReal = p.FechaHoraDevolucionReal,
                Estado                = p.Estado,
                Llave                 = p.Llave.Codigo,
                Ambiente              = p.Llave.Ambiente != null ? p.Llave.Ambiente.Nombre : "-"
            })
            .ToListAsync();
    }

    private async Task<List<TopPersonaReporteItem>> ObtenerTopPersonasAsync(DateTime? fechaDesde, DateTime? fechaHasta, int top)
    {
        var prestamos = _context.Prestamos.AsQueryable();
        var reservas  = _context.Reservas.AsQueryable();

        if (fechaDesde.HasValue)
        {
            var desde = DateTime.SpecifyKind(fechaDesde.Value.Date, DateTimeKind.Utc);
            prestamos = prestamos.Where(p => p.FechaHoraPrestamo >= desde);
            reservas  = reservas.Where(r => r.FechaInicio >= desde);
        }

        if (fechaHasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(fechaHasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            prestamos = prestamos.Where(p => p.FechaHoraPrestamo <= hasta);
            reservas  = reservas.Where(r => r.FechaInicio <= hasta);
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
                Reservas  = topReservas.FirstOrDefault(r => r.IdPersona == p.IdPersona)?.Cantidad ?? 0
            })
            .Concat(
                topReservas
                    .Where(r => !topPrestamos.Any(p => p.IdPersona == r.IdPersona))
                    .Select(r => new TopPersonaReporteItem
                    {
                        IdPersona = r.IdPersona,
                        Prestamos = 0,
                        Reservas  = r.Cantidad
                    })
            )
            .Select(x => new TopPersonaReporteItem
            {
                IdPersona = x.IdPersona,
                Nombre    = personas.ContainsKey(x.IdPersona) ? personas[x.IdPersona] : "Persona no encontrada",
                Prestamos = x.Prestamos,
                Reservas  = x.Reservas,
                Total     = x.Prestamos + x.Reservas
            })
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Nombre)
            .Take(top)
            .ToList();
    }

    private async Task<List<InventarioLlaveItem>> ObtenerInventarioLlavesAsync(int? idAmbiente, string? estado)
    {
        var query = _context.Llaves.Include(l => l.Ambiente).AsQueryable();

        if (idAmbiente.HasValue)
            query = query.Where(l => l.IdAmbiente == idAmbiente.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(l => l.Estado == estado);
        else
            query = query.Where(l => l.Estado != "I");

        var llaves   = await query.OrderBy(l => l.Codigo).ToListAsync();
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
                IdLlave        = l.IdLlave,
                Codigo         = l.Codigo,
                Ambiente       = l.Ambiente?.Nombre ?? "-",
                EsMaestra      = l.EsMaestra,
                Estado         = l.Estado,
                TotalPrestamos = conteoPrestamos.GetValueOrDefault(l.IdLlave, 0),
                UltimaPersona  = ultimo != null ? ultimo.Persona.Nombres + " " + ultimo.Persona.Apellidos : "-",
                UltimaFecha    = ultimo?.FechaHoraPrestamo
            };
        }).ToList();
    }

    private async Task<List<PrestamoVencidoItem>> ObtenerPrestamosVencidosAsync(int? idAmbiente)
    {
        var ahora = DateTime.UtcNow;

        var query = _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Llave).ThenInclude(l => l.Ambiente)
            .Where(p => p.Estado == "A"
                     && p.FechaHoraDevolucionEsperada.HasValue
                     && p.FechaHoraDevolucionEsperada.Value < ahora)
            .AsQueryable();

        if (idAmbiente.HasValue)
            query = query.Where(p => p.Llave.IdAmbiente == idAmbiente.Value);

        var prestamos = await query.OrderBy(p => p.FechaHoraDevolucionEsperada).ToListAsync();

        return prestamos.Select(p => new PrestamoVencidoItem
        {
            IdPrestamo      = p.IdPrestamo,
            Persona         = p.Persona.Nombres + " " + p.Persona.Apellidos,
            Ci              = p.Persona.Ci,
            Celular         = p.Persona.Celular ?? "-",
            Llave           = p.Llave.Codigo,
            Ambiente        = p.Llave.Ambiente?.Nombre ?? "-",
            FechaHoraPrestamo = p.FechaHoraPrestamo,
            FechaEsperada   = p.FechaHoraDevolucionEsperada!.Value,
            DiasVencido     = (int)(ahora - p.FechaHoraDevolucionEsperada!.Value).TotalDays
        }).ToList();
    }

    private async Task<List<ActividadAmbienteItem>> ObtenerActividadAmbienteAsync(DateTime? fechaDesde, DateTime? fechaHasta)
    {
        var prestamosQuery = _context.Prestamos.Include(p => p.Llave).AsQueryable();
        var reservasQuery  = _context.Reservas.Include(r => r.Llave).AsQueryable();

        if (fechaDesde.HasValue)
        {
            var desde = DateTime.SpecifyKind(fechaDesde.Value.Date, DateTimeKind.Utc);
            prestamosQuery = prestamosQuery.Where(p => p.FechaHoraPrestamo >= desde);
            reservasQuery  = reservasQuery.Where(r => r.FechaInicio >= desde);
        }

        if (fechaHasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(fechaHasta.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            prestamosQuery = prestamosQuery.Where(p => p.FechaHoraPrestamo <= hasta);
            reservasQuery  = reservasQuery.Where(r => r.FechaInicio <= hasta);
        }

        var prestamos = await prestamosQuery.ToListAsync();
        var reservas  = await reservasQuery.ToListAsync();

        var ambientes = await _context.Ambientes
            .Where(a => a.Estado == "A")
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        return ambientes.Select(a =>
        {
            var pres   = prestamos.Where(p => p.Llave.IdAmbiente == a.IdAmbiente).ToList();
            var res    = reservas.Where(r => r.Llave.IdAmbiente == a.IdAmbiente).ToList();
            var conDev = pres.Where(p => p.FechaHoraDevolucionReal.HasValue).ToList();
            var horasTotal = conDev.Sum(p => (p.FechaHoraDevolucionReal!.Value - p.FechaHoraPrestamo).TotalHours);

            return new ActividadAmbienteItem
            {
                IdAmbiente            = a.IdAmbiente,
                Codigo                = a.Codigo,
                Ambiente              = a.Nombre,
                TotalPrestamos        = pres.Count,
                TotalReservas         = res.Count,
                TotalActividad        = pres.Count + res.Count,
                PromedioHorasPrestamo = conDev.Count > 0 ? Math.Round(horasTotal / conDev.Count, 1) : 0
            };
        })
        .Where(x => x.TotalActividad > 0)
        .OrderByDescending(x => x.TotalActividad)
        .ToList();
    }

    // ─── Traductores de estado ───────────────────────────────

    private static string TraducirEstadoReserva(string estado) => estado switch
    {
        "P" => "Pendiente",
        "C" => "Confirmada",
        "U" => "Utilizada",
        "X" => "Cancelada",
        _   => estado
    };

    private static string TraducirEstadoPrestamo(string estado) => estado switch
    {
        "A" => "Activo",
        "D" => "Devuelto",
        "V" => "Vencido",
        "C" => "Cancelado",
        _   => estado
    };

    private static string TraducirEstadoLlave(string estado) => estado switch
    {
        "D" => "Disponible",
        "P" => "Prestada",
        "R" => "Reservada",
        "I" => "Inactiva",
        _   => estado
    };
}
