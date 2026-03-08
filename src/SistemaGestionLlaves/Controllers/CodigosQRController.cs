using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace SistemaGestionLlaves.Controllers
{
    [Authorize]
    public class CodigosQRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CodigosQRController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "LlavesView");
        }

        [HttpPost]
        public async Task<IActionResult> GenerarEtiquetas(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                TempData["Error"] = "Por favor selecciona al menos una llave.";
                return RedirectToAction("Index", "LlavesView");
            }

            var llaves = await _context.Llaves
                .Include(l => l.Ambiente)
                .Where(l => ids.Contains(l.IdLlave))
                .ToListAsync();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    page.Content().PaddingVertical(10).Grid(grid =>
                    {
                        grid.VerticalSpacing(15);
                        grid.HorizontalSpacing(15);
                        grid.Columns(3); // 3 etiquetas por fila

                        foreach (var llave in llaves)
                        {
                            grid.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(col =>
                            {
                                col.Item().AlignCenter().Text(llave.Ambiente.Nombre).Bold().FontSize(12);
                                col.Item().AlignCenter().Text($"Cód: {llave.Codigo}").FontSize(9).FontColor(Colors.Grey.Medium);
                                
                                // Generar QR
                                var qrImage = GenerarImagenQR(llave.Codigo);
                                col.Item().AlignCenter().Width(80).Image(qrImage);

                                col.Item().AlignCenter().Text("S.G.LL. / Alex").FontSize(7).Italic();
                            });
                        }
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"Etiquetas_QR_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public IActionResult Escanear()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ProcesarEscaneo(string codigo)
        {
            if (string.IsNullOrEmpty(codigo)) return BadRequest("Código vacío");

            var llave = await _context.Llaves.FirstOrDefaultAsync(l => l.Codigo == codigo);
            if (llave == null)
            {
                return Json(new { success = false, message = "Llave no encontrada en el sistema." });
            }

            // Redirigir según el estado
            string url = "";
            if (llave.Estado == "D") // Disponible
            {
                url = Url.Action("Crear", "Prestamos", new { idLlave = llave.IdLlave }) ?? "";
            }
            else if (llave.Estado == "P") // Prestada
            {
                // Buscar el préstamo activo para esta llave
                var prestamo = await _context.Prestamos
                    .FirstOrDefaultAsync(p => p.IdLlave == llave.IdLlave && p.Estado == "A");
                
                if (prestamo != null)
                {
                    // Redirigimos al Index de préstamos y allí manejaremos la apertura del modal via JS
                    url = Url.Action("Index", "Prestamos", new { idPrestamo = prestamo.IdPrestamo }) ?? "";
                }
                else
                {
                    return Json(new { success = false, message = "La llave aparece como prestada pero no hay registro activo." });
                }
            }
            else if (llave.Estado == "M") // Mantenimiento
            {
                return Json(new { success = false, message = "Esta llave está bloqueada por mantenimiento." });
            }

            return Json(new { success = true, redirectUrl = url });
        }

        private byte[] GenerarImagenQR(string texto)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}
