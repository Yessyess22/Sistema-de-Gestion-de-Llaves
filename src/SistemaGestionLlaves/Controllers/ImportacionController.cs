using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers
{
    [Authorize]
    public class ImportacionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImportacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new ImportResult { Success = true });
        }

        [HttpGet]
        public IActionResult DescargarPlantilla(string tipo)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Plantilla");

            switch (tipo.ToLower())
            {
                case "personas":
                    worksheet.Cell(1, 1).Value = "CI";
                    worksheet.Cell(1, 2).Value = "Nombres";
                    worksheet.Cell(1, 3).Value = "Apellidos";
                    worksheet.Cell(1, 4).Value = "Celular";
                    worksheet.Cell(1, 5).Value = "Correo";
                    break;

                case "ambientes":
                    worksheet.Cell(1, 1).Value = "Nombre";
                    worksheet.Cell(1, 2).Value = "Codigo";
                    worksheet.Cell(1, 3).Value = "TipoAmbiente"; // Ejemplo: Aula, Laboratorio, Oficina
                    worksheet.Cell(1, 4).Value = "Ubicacion";
                    break;

                case "llaves":
                    worksheet.Cell(1, 1).Value = "Codigo";
                    worksheet.Cell(1, 2).Value = "NombreAmbiente";
                    worksheet.Cell(1, 3).Value = "NumCopias";
                    worksheet.Cell(1, 4).Value = "EsMaestra"; // SI / NO
                    worksheet.Cell(1, 5).Value = "Observaciones";
                    break;

                default:
                    return BadRequest("Tipo de plantilla no válido.");
            }

            // Estilo de encabezado
            var rngHeader = worksheet.Range(1, 1, 1, worksheet.ColumnsUsed().Count());
            rngHeader.Style.Font.Bold = true;
            rngHeader.Style.Fill.BackgroundColor = XLColor.FromHtml("#0369a1");
            rngHeader.Style.Font.FontColor = XLColor.White;
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Plantilla_{tipo}.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Importar(IFormFile archivo, string tipo)
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Por favor selecciona un archivo de Excel.";
                return RedirectToAction(nameof(Index));
            }

            var result = new ImportResult { Tipo = tipo };

            try
            {
                using var stream = archivo.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1); // Saltar encabezado

                switch (tipo.ToLower())
                {
                    case "personas":
                        await ProcesarPersonas(rows, result);
                        break;
                    case "ambientes":
                        await ProcesarAmbientes(rows, result);
                        break;
                    case "llaves":
                        await ProcesarLlaves(rows, result);
                        break;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Log.Add($"Error crítico al procesar el archivo: {ex.Message}");
            }

            return View("Index", result);
        }

        private async Task ProcesarPersonas(IEnumerable<IXLRow> rows, ImportResult result)
        {
            foreach (var row in rows)
            {
                try
                {
                    var ci = row.Cell(1).GetValue<string>()?.Trim();
                    if (string.IsNullOrEmpty(ci)) continue;

                    // Validaciones de caracteres
                    if (!System.Text.RegularExpressions.Regex.IsMatch(ci, @"^[0-9]*$"))
                    {
                        result.RegistrosFallidos++;
                        result.Log.Add($"Fila {row.RowNumber()}: El CI '{ci}' contiene caracteres no permitidos (solo números).");
                        continue;
                    }

                    var nombres = row.Cell(2).GetValue<string>()?.Trim() ?? "";
                    var apellidos = row.Cell(3).GetValue<string>()?.Trim() ?? "";
                    var regexAlpha = @"^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ\s]*$";

                    if (!System.Text.RegularExpressions.Regex.IsMatch(nombres, regexAlpha) || 
                        !System.Text.RegularExpressions.Regex.IsMatch(apellidos, regexAlpha))
                    {
                        result.RegistrosFallidos++;
                        result.Log.Add($"Fila {row.RowNumber()}: El nombre o apellido contiene caracteres especiales no permitidos.");
                        continue;
                    }

                    var celular = row.Cell(4).GetValue<string>()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(celular) && !System.Text.RegularExpressions.Regex.IsMatch(celular, @"^[0-9]*$"))
                    {
                        result.RegistrosFallidos++;
                        result.Log.Add($"Fila {row.RowNumber()}: El celular '{celular}' contiene caracteres no permitidos (solo números).");
                        continue;
                    }

                    if (await _context.Personas.AnyAsync(p => p.Ci == ci))
                    {
                        result.RegistrosSaltados++;
                        result.Log.Add($"Fila {row.RowNumber()}: La persona con CI {ci} ya existe.");
                        continue;
                    }

                    var persona = new Persona
                    {
                        Ci = ci,
                        Nombres = string.IsNullOrEmpty(nombres) ? "Sin Nombre" : nombres,
                        Apellidos = string.IsNullOrEmpty(apellidos) ? "Sin Apellido" : apellidos,
                        Celular = celular,
                        Correo = row.Cell(5).GetValue<string>()?.Trim(),
                        Estado = "A"
                    };

                    _context.Personas.Add(persona);
                    result.RegistrosExitosos++;
                }
                catch (Exception ex)
                {
                    result.RegistrosFallidos++;
                    result.Log.Add($"Error en fila {row.RowNumber()}: {ex.Message}");
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task ProcesarAmbientes(IEnumerable<IXLRow> rows, ImportResult result)
        {
            var tiposExistentes = await _context.TiposAmbiente.ToDictionaryAsync(t => t.NombreTipo.ToLower(), t => t);

            foreach (var row in rows)
            {
                try
                {
                    var nombre = row.Cell(1).GetValue<string>()?.Trim();
                    var codigo = row.Cell(2).GetValue<string>()?.Trim();
                    if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(codigo)) continue;

                    // Validaciones de caracteres
                    var regexAlpha = @"^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ\s]*$";
                    var regexCodigoAmb = @"^[a-zA-Z0-9\s-]*$";

                    if (!System.Text.RegularExpressions.Regex.IsMatch(nombre, regexAlpha))
                    {
                        result.RegistrosFallidos++;
                        result.Log.Add($"Fila {row.RowNumber()}: El nombre del ambiente '{nombre}' contiene caracteres especiales no permitidos.");
                        continue;
                    }

                    if (!System.Text.RegularExpressions.Regex.IsMatch(codigo, regexCodigoAmb))
                    {
                        result.RegistrosFallidos++;
                        result.Log.Add($"Fila {row.RowNumber()}: El código de ambiente '{codigo}' solo puede contener letras, números, espacios y guiones.");
                        continue;
                    }

                    var ubicacion = row.Cell(4).GetValue<string>()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(ubicacion) && !System.Text.RegularExpressions.Regex.IsMatch(ubicacion, @"^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ\s,.-]*$"))
                    {
                        result.RegistrosFallidos++;
                        result.Log.Add($"Fila {row.RowNumber()}: La ubicación contiene caracteres no permitidos.");
                        continue;
                    }

                    if (await _context.Ambientes.AnyAsync(a => a.Nombre == nombre || a.Codigo == codigo))
                    {
                        result.RegistrosSaltados++;
                        result.Log.Add($"Fila {row.RowNumber()}: El ambiente '{nombre}' (o código '{codigo}') ya existe.");
                        continue;
                    }

                    var nombreTipo = row.Cell(3).GetValue<string>()?.Trim() ?? "General";
                    if (!tiposExistentes.TryGetValue(nombreTipo.ToLower(), out var tipo))
                    {
                        tipo = new TipoAmbiente { NombreTipo = nombreTipo };
                        _context.TiposAmbiente.Add(tipo);
                        await _context.SaveChangesAsync();
                        tiposExistentes.Add(nombreTipo.ToLower(), tipo);
                    }

                    var ambiente = new Ambiente
                    {
                        Nombre = nombre,
                        Codigo = codigo,
                        IdTipo = tipo.IdTipo,
                        Ubicacion = ubicacion,
                        Estado = "A"
                    };

                    _context.Ambientes.Add(ambiente);
                    result.RegistrosExitosos++;
                }
                catch (Exception ex)
                {
                    result.RegistrosFallidos++;
                    result.Log.Add($"Error en fila {row.RowNumber()}: {ex.Message}");
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task ProcesarLlaves(IEnumerable<IXLRow> rows, ImportResult result)
        {
            foreach (var row in rows)
            {
                try
                {
                    var codigo = row.Cell(1).GetValue<string>()?.Trim();
                    if (string.IsNullOrEmpty(codigo)) continue;

                    // Validación de caracteres para código de llave
                    if (!System.Text.RegularExpressions.Regex.IsMatch(codigo, @"^[a-zA-Z0-9-]*$"))
                    {
                        result.RegistrosFallidos++;
                        result.Log.Add($"Fila {row.RowNumber()}: El código de llave '{codigo}' solo puede contener letras, números y guiones.");
                        continue;
                    }

                    if (await _context.Llaves.AnyAsync(l => l.Codigo == codigo))
                    {
                        result.RegistrosSaltados++;
                        result.Log.Add($"Fila {row.RowNumber()}: La llave '{codigo}' ya existe.");
                        continue;
                    }

                    var nombreAmbiente = row.Cell(2).GetValue<string>()?.Trim();
                    var ambiente = await _context.Ambientes.FirstOrDefaultAsync(a => a.Nombre == nombreAmbiente);
                    
                    if (ambiente == null)
                    {
                        result.RegistrosFallidos++;
                        result.Log.Add($"Error en fila {row.RowNumber()}: No se encontró el ambiente '{nombreAmbiente}'.");
                        continue;
                    }

                    var llave = new Llave
                    {
                        Codigo = codigo,
                        IdAmbiente = ambiente.IdAmbiente,
                        NumCopias = row.Cell(3).GetValue<int>(),
                        EsMaestra = row.Cell(4).GetValue<string>()?.ToUpper() == "SI",
                        Observaciones = row.Cell(5).GetValue<string>()?.Trim(),
                        Estado = "D"
                    };

                    _context.Llaves.Add(llave);
                    result.RegistrosExitosos++;
                }
                catch (Exception ex)
                {
                    result.RegistrosFallidos++;
                    result.Log.Add($"Error en fila {row.RowNumber()}: {ex.Message}");
                }
            }
            await _context.SaveChangesAsync();
        }
    }

    public class ImportResult
    {
        public bool Success { get; set; } = true;
        public string Tipo { get; set; } = string.Empty;
        public int RegistrosExitosos { get; set; }
        public int RegistrosFallidos { get; set; }
        public int RegistrosSaltados { get; set; }
        public List<string> Log { get; set; } = new List<string>();
    }
}
