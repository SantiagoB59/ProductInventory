
using Microsoft.AspNetCore.Mvc;
using Products.Api.Contracts.Products;
using Products.Application.Contracts.Persistence;
using Products.Application.Products;
using Products.Domain.Entities;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public sealed class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
                "Invalid product data",
                exception.Message));
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return NotFound(CreateProblem(
                StatusCodes.Status404NotFound,
                "Product not found",
                $"No product exists with ID '{id}'."));
        }

        return Ok(ProductResponse.From(product));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "Invalid pagination parameters",
                "Page must be at least 1 and pageSize must be between 1 and 100."));
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
                    "Product not found",
                    $"No product exists with ID '{id}'."));
            }

            var product = await _productService.GetByIdAsync(id, cancellationToken);

            return Ok(ProductResponse.From(product!));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(CreateProblem(
                StatusCodes.Status400BadRequest,
                "Invalid product data",
                exception.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound(CreateProblem(
                StatusCodes.Status404NotFound,
                "Product not found",
                $"No product exists with ID '{id}'."));
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
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
                    "Invalid stock adjustment",
                    "Quantity cannot be zero."));

            case StockAdjustmentResult.NotFound:
                return NotFound(CreateProblem(
                    StatusCodes.Status404NotFound,
                    "Product not found",
                    $"No product exists with ID '{id}'."));

            case StockAdjustmentResult.InsufficientStock:
                return Conflict(CreateProblem(
                    StatusCodes.Status409Conflict,
                    "Insufficient stock",
                    "The adjustment would cause the product stock to become negative."));

            case StockAdjustmentResult.StockLimitExceeded:
                return Conflict(CreateProblem(
                    StatusCodes.Status409Conflict,
                    "Stock limit exceeded",
                    "The adjustment exceeds the maximum supported stock."));

            case StockAdjustmentResult.Updated:
                var product = await _productService.GetByIdAsync(id, cancellationToken);
                return Ok(ProductResponse.From(product!));

            default:
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Unexpected error",
                    detail: "The stock adjustment could not be completed.");
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

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock)
{
    public static ProductResponse From(Product product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.Stock);
}

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);
