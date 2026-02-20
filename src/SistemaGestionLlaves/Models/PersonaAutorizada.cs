namespace SistemaGestionLlaves.Models;

/// <summary>
/// Define qué persona está autorizada a solicitar una llave específica.
/// </summary>
public class PersonaAutorizada
{
    public int Id { get; set; }
    public int IdPersona { get; set; }
    public int IdLlave { get; set; }

    // Navegación
    public Persona Persona { get; set; } = null!;
    public Llave Llave { get; set; } = null!;
}
