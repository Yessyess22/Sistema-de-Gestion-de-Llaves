using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaWeb.Models
{
    /// <summary>
    /// Modelo de Persona - Entidad que representa una persona o empresa en el sistema.
    /// Mapea los campos de la tabla 'personas' en PostgreSQL.
    /// </summary>
    [Table("personas")] // Nombre de la tabla en PostgreSQL
    public class Persona
    {
        /// <summary>
        /// Identificador único de la persona (Clave Primaria)
        /// </summary>
        [Key]
        [Column("id_persona")]
        public int IdPersona { get; set; }

        /// <summary>
        /// Nombres de la persona
        /// </summary>
        [Required(ErrorMessage = "Los nombres son requeridos")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 150 caracteres")]
        [Column("nombres")]
        public string Nombres { get; set; } = string.Empty;

        /// <summary>
        /// Apellido paterno
        /// </summary>
        [Required(ErrorMessage = "El apellido paterno es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido paterno debe tener entre 2 y 100 caracteres")]
        [Column("apellido_paterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        /// <summary>
        /// Apellido materno
        /// </summary>
        [Required(ErrorMessage = "El apellido materno es requerido")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido materno debe tener entre 2 y 100 caracteres")]
        [Column("apellido_materno")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico de la persona
        /// </summary>
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Debe proporcionar un correo electrónico válido")]
        [StringLength(150)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Número de teléfono
        /// </summary>
        [Required(ErrorMessage = "El teléfono es requerido")]
        [StringLength(20, MinimumLength = 7, ErrorMessage = "El teléfono debe tener entre 7 y 20 caracteres")]
        [RegularExpression(@"^[0-9\-\+\(\)\ ]+$", ErrorMessage = "El teléfono solo puede contener números, guiones, paréntesis y espacios")]
        [Column("telefono")]
        public string Telefono { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de nacimiento
        /// </summary>
        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        [DataType(DataType.Date)]
        [Column("fecha_nac")]
        public DateTime FechaNac { get; set; }

        /// <summary>
        /// Tipo de persona: "Documento" o "Empresa"
        /// </summary>
        [Required(ErrorMessage = "El tipo de persona es requerido")]
        [StringLength(20)]
        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Código de identificación de la persona o empresa
        /// </summary>
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(50)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Cédula de Identidad (CI)
        /// </summary>
        [Required(ErrorMessage = "La Cédula de Identidad es requerida")]
        [StringLength(30)]
        [Column("ci")]
        public string CI { get; set; } = string.Empty;

        /// <summary>
        /// Propiedad de lectura para obtener el nombre completo
        /// </summary>
        [NotMapped]
        public string NombreCompleto => $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}".Trim();

        /// <summary>
        /// Propiedad de lectura para validar si es mayor de edad
        /// </summary>
        [NotMapped]
        public int Edad
        {
            get
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - FechaNac.Year;

                if (FechaNac.Date > hoy.AddYears(-edad))
                {
                    edad--;
                }

                return edad;
            }
        }
    }
}
