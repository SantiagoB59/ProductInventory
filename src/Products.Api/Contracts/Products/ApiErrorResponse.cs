
namespace Products.Api.Contracts.Products;

/// <summary>
/// Estructura estándar de las respuestas de error de la API.
/// </summary>
public sealed class ApiErrorResponse
{
    /// <summary>URI que identifica el tipo de error.</summary>
    public string? Type { get; set; }

    /// <summary>Resumen breve del error.</summary>
    public string? Title { get; set; }

    /// <summary>Código de estado HTTP.</summary>
    public int? Status { get; set; }

    /// <summary>Explicación específica del error.</summary>
    public string? Detail { get; set; }

    /// <summary>URI de la solicitud que produjo el error.</summary>
    public string? Instance { get; set; }

    /// <summary>Errores de validación por campo, cuando corresponda.</summary>
    public Dictionary<string, string[]>? Errors { get; set; }
}
