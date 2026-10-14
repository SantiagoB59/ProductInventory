
using Microsoft.AspNetCore.Mvc;
using Products.Api.Contracts.Products;
using Products.Application.Products;
using Products.Domain.Entities;
using Products.Application.Contracts.Persistence;
namespace Products.Api.Controllers;

/// <summary>
/// Expone los endpoints para administrar el catálogo de productos y sus existencias.
/// </summary>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
public sealed class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    /// <summary>
    /// Inicializa el controlador de productos.
    /// </summary>
    /// <param name="productService">Servicio para gestionar productos e inventario.</param>
    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Registra un nuevo producto en el catálogo.
    /// </summary>
    /// <param name="request">Información del producto que se va a registrar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El producto creado y la ubicación de su recurso.</returns>
    /// <response code="201">El producto se creó correctamente.</response>
    /// <response code="400">Los datos del producto no son válidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productService.CreateAsync(
                request.Name,
                request.Description,
                request.Price,
                request.Stock,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                ProductResponse.From(product));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "Datos del producto no válidos",
                exception.Message));
        }
    }

    /// <summary>
    /// Consulta un producto por su identificador único.
    /// </summary>
    /// <param name="id">Identificador UUID del producto.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La información del producto solicitado.</returns>
    /// <response code="200">El producto se encontró correctamente.</response>
    /// <response code="404">No existe un producto con el identificador indicado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return NotFound(CreateProblem(
                StatusCodes.Status404NotFound,
                "Producto no encontrado",
                $"No existe un producto con el identificador '{id}'."));
        }

        return Ok(ProductResponse.From(product));
    }

    /// <summary>
    /// Obtiene una lista paginada de productos.
    /// </summary>
    /// <param name="page">Número de página. Debe ser mayor o igual a 1.</param>
    /// <param name="pageSize">Cantidad de productos por página. Rango permitido: de 1 a 100.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Lista de productos con información de paginación.</returns>
    /// <response code="200">La lista de productos se obtuvo correctamente.</response>
    /// <response code="400">Los parámetros de paginación no son válidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "Parámetros de paginación no válidos",
                "La página debe ser como mínimo 1 y el tamaño de página debe estar entre 1 y 100."));
        }

        var result = await _productService.GetPageAsync(
            page,
            pageSize,
            cancellationToken);

        var response = new PagedResponse<ProductResponse>(
            result.Items.Select(ProductResponse.From).ToList(),
            result.Page,
            result.PageSize,
            result.TotalItems,
            (int)Math.Ceiling(result.TotalItems / (double)result.PageSize));

        return Ok(response);
    }

    /// <summary>
    /// Actualiza los datos de un producto existente.
    /// </summary>
    /// <param name="id">Identificador UUID del producto.</param>
    /// <param name="request">Nuevos datos del producto.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El producto actualizado.</returns>
    /// <response code="200">El producto se actualizó correctamente.</response>
    /// <response code="400">Los datos proporcionados no son válidos.</response>
    /// <response code="404">No existe un producto con el identificador indicado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _productService.UpdateAsync(
                id,
                request.Name,
                request.Description,
                request.Price,
                cancellationToken);

            if (!updated)
            {
                return NotFound(CreateProblem(
                    StatusCodes.Status404NotFound,
                    "Producto no encontrado",
                    $"No existe un producto con el identificador '{id}'."));
            }

            var product = await _productService.GetByIdAsync(id, cancellationToken);

            return Ok(ProductResponse.From(product!));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "Datos del producto no válidos",
                exception.Message));
        }
    }

    /// <summary>
    /// Elimina un producto del catálogo.
    /// </summary>
    /// <param name="id">Identificador UUID del producto.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="204">El producto se eliminó correctamente.</response>
    /// <response code="404">No existe un producto con el identificador indicado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(CreateProblem(
                StatusCodes.Status404NotFound,
                "Producto no encontrado",
                $"No existe un producto con el identificador '{id}'."));
        }

        return NoContent();
    }

    /// <summary>
    /// Ajusta las existencias de un producto.
    /// </summary>
    /// <remarks>
    /// Una cantidad positiva aumenta las existencias y una cantidad negativa las disminuye.
    /// No se permite una cantidad igual a cero ni un inventario final negativo.
    /// </remarks>
    /// <param name="id">Identificador UUID del producto.</param>
    /// <param name="request">Cantidad que se sumará o restará a las existencias.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El producto con sus existencias actualizadas.</returns>
    /// <response code="200">Las existencias se actualizaron correctamente.</response>
    /// <response code="400">La cantidad indicada no es válida.</response>
    /// <response code="404">No existe un producto con el identificador indicado.</response>
    /// <response code="409">El ajuste dejaría existencias negativas o superaría el límite permitido.</response>
    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> AdjustStock(
        Guid id,
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.AdjustStockAsync(
            id,
            request.Quantity,
            cancellationToken);

        switch (result)
        {
            case StockAdjustmentResult.InvalidQuantity:
                return BadRequest(CreateProblem(
                    StatusCodes.Status400BadRequest,
                    "Ajuste de inventario no válido",
                    "La cantidad no puede ser cero."));

            case StockAdjustmentResult.NotFound:
                return NotFound(CreateProblem(
                    StatusCodes.Status404NotFound,
                    "Producto no encontrado",
                    $"No existe un producto con el identificador '{id}'."));

            case StockAdjustmentResult.InsufficientStock:
                return Conflict(CreateProblem(
                    StatusCodes.Status409Conflict,
                    "Existencias insuficientes",
                    "El ajuste provocaría que las existencias del producto fueran negativas."));

            case StockAdjustmentResult.StockLimitExceeded:
                return Conflict(CreateProblem(
                    StatusCodes.Status409Conflict,
                    "Límite de inventario excedido",
                    "El ajuste supera el límite máximo de existencias permitido."));

            case StockAdjustmentResult.Updated:
                var product = await _productService.GetByIdAsync(id, cancellationToken);
                return Ok(ProductResponse.From(product!));

            default:
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error inesperado",
                    detail: "No fue posible completar el ajuste de inventario.");
        }
    }

    private ProblemDetails CreateProblem(
        int statusCode,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = HttpContext.Request.Path
        };
    }
}

/// <summary>
/// Representa un producto devuelto por la API.
/// </summary>
/// <param name="Id">Identificador único del producto.</param>
/// <param name="Name">Nombre del producto.</param>
/// <param name="Description">Descripción del producto, si existe.</param>
/// <param name="Price">Precio unitario del producto.</param>
/// <param name="Stock">Existencias actuales del producto.</param>
public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock)
{
    /// <summary>
    /// Convierte una entidad de dominio en la respuesta de la API.
    /// </summary>
    /// <param name="product">Producto de dominio.</param>
    /// <returns>Representación del producto para la API.</returns>
    public static ProductResponse From(Product product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.Stock);
}

/// <summary>
/// Representa una página de resultados.
/// </summary>
/// <typeparam name="T">Tipo de los elementos de la página.</typeparam>
/// <param name="Items">Elementos incluidos en la página actual.</param>
/// <param name="Page">Número de página actual.</param>
/// <param name="PageSize">Cantidad máxima de elementos por página.</param>
/// <param name="TotalItems">Cantidad total de elementos disponibles.</param>
/// <param name="TotalPages">Cantidad total de páginas.</param>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
