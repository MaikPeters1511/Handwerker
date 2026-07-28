using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Handwerker.Application.Services;
using Handwerker.Application.Services.Keycloak;
using Handwerker.Domain.Entities;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductsController(ProductService productService, IKcUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(int page = 1, int pageSize = 50)
    {
        var result = await productService.GetAsync(page, pageSize);
        return Ok(result);
    }
    
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await productService.GetByIdAsync(id);
    
        if (product == null)
        {
            return NotFound();
        }
    
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> PostProduct([FromBody] Product product)
    {
        try
        {
            var created = await productService.CreateAsync(product);
            return CreatedAtAction("GetProduct", new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Fehler beim Erstellen des Produkts: {ex.Message}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, [FromBody] Product product)
    {
        if (id != product.Id)
        {
            return BadRequest("ID stimmt nicht überein");
        }

        try
        {
            await productService.UpdateAsync(product);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Fehler beim Aktualisieren des Produkts: {ex.Message}" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            await productService.DeleteAsync(id);
            var userId = userService.GetCurrentUserId();
            // await notificationService.NotifyProductDeletedAsync(userId, productName);
            return Ok(true);
        }
        catch (Exception e)
        {
            return NotFound();
        }
       
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(string term)
    {
        var result = await productService.SearchAsync(term);
        return Ok(result);
    }
}
