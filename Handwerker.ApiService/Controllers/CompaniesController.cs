using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Handwerker.ApiService.Models;
using Handwerker.Domain.Interfaces;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CompaniesController(CompanyService companyService, IFileStorageService fileStorage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        var result = await companyService.GetAllAsync();
        var dto = result.Select(c => new CompanyDto {
            Id = c.Id,
            Name = c.Name,
            TaxId = c.TaxId,
            TaxNumber = c.TaxNumber,
            Street = c.Street,
            ZipCode = c.ZipCode,
            City = c.City,
            Country = c.Country,
            Email = c.Email,
            Phone = c.Phone,
            BankName = c.BankName,
            Iban = c.Iban,
            Bic = c.Bic,
            CommercialRegister = c.CommercialRegister,
            RegisterCourt = c.RegisterCourt,
            VatExemption = c.VatExemption,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            LogoUrl = c.LogoPath
        });
        return Ok(dto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var company = await companyService.GetByIdAsync(id);
        if (company == null) return NotFound();

        var dto = new CompanyDto {
            Id = company.Id,
            Name = company.Name,
            TaxId = company.TaxId,
            TaxNumber = company.TaxNumber,
            Street = company.Street,
            ZipCode = company.ZipCode,
            City = company.City,
            Country = company.Country,
            Email = company.Email,
            Phone = company.Phone,
            BankName = company.BankName,
            Iban = company.Iban,
            Bic = company.Bic,
            CommercialRegister = company.CommercialRegister,
            RegisterCourt = company.RegisterCourt,
            VatExemption = company.VatExemption,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            LogoUrl = company.LogoPath
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> PostCompany([FromBody] CompanyDto request)
    {
        try
        {
            var entity = new Company {
                Name = request.Name,
                TaxId = request.TaxId,
                TaxNumber = request.TaxNumber,
                Street = request.Street,
                ZipCode = request.ZipCode,
                City = request.City,
                Country = request.Country,
                Email = request.Email,
                Phone = request.Phone,
                BankName = request.BankName,
                Iban = request.Iban,
                Bic = request.Bic,
                CommercialRegister = request.CommercialRegister,
                RegisterCourt = request.RegisterCourt,
                VatExemption = request.VatExemption,
                LogoPath = request.LogoUrl
            };
            var created = await companyService.CreateAsync(entity);
            var dto = new CompanyDto {
                Id = created.Id,
                Name = created.Name,
                TaxId = created.TaxId,
                TaxNumber = created.TaxNumber,
                Street = created.Street,
                ZipCode = created.ZipCode,
                City = created.City,
                Country = created.Country,
                Email = created.Email,
                Phone = created.Phone,
                BankName = created.BankName,
                Iban = created.Iban,
                Bic = created.Bic,
                CommercialRegister = created.CommercialRegister,
                RegisterCourt = created.RegisterCourt,
                VatExemption = created.VatExemption,
                LogoUrl = created.LogoPath,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt
            };
            return CreatedAtAction("GetCompany", new { id = dto.Id }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutCompany(int id, [FromBody] CompanyDto request)
    {
        if (id != request.Id) return BadRequest("ID mismatch");

        try
        {
            // Lade das bestehende Entity, um keine Felder zu überschreiben
            var existing = await companyService.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Aktualisiere nur die gesendeten Felder
            existing.Name = request.Name;
            existing.TaxId = request.TaxId;
            existing.Street = request.Street;
            existing.ZipCode = request.ZipCode;
            existing.City = request.City;
            existing.Country = request.Country;
            existing.Email = request.Email;
            existing.Phone = request.Phone;
            existing.BankName = request.BankName;
            existing.Iban = request.Iban;
            existing.Bic = request.Bic;
            existing.TaxNumber = request.TaxNumber;
            existing.VatExemption = request.VatExemption;
            existing.CommercialRegister = request.CommercialRegister ?? existing.CommercialRegister;
            existing.RegisterCourt = request.RegisterCourt ?? existing.RegisterCourt;
            
            // LogoPath nur aktualisieren, wenn gesetzt (Logo wird über separaten Endpoint hochgeladen)
            if (!string.IsNullOrEmpty(request.LogoUrl))
            {
                existing.LogoPath = request.LogoUrl;
            }
            
            existing.UpdatedAt = DateTime.UtcNow;
            
            await companyService.UpdateAsync(existing);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        try
        {
            await companyService.DeleteAsync(id);
            return Ok(true);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/logo")]
    public async Task<IActionResult> UploadLogo(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        // Validate size (<= 2MB)
        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "File too large (max 2 MB)" });

        // Validate content type
        var allowed = new[] { "image/png", "image/jpeg", "image/jpg", "image/svg+xml" };
        if (!allowed.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Invalid file type" });

        var company = await companyService.GetByIdAsync(id);
        if (company == null) return NotFound();

        // Save file
        using var stream = file.OpenReadStream();
        var saved = await fileStorage.SaveFileAsync(stream, file.FileName, file.ContentType);

        // Delete previous logo if any
        if (!string.IsNullOrEmpty(company.LogoPath))
        {
            await fileStorage.DeleteFileAsync(company.LogoPath);
        }

        company.LogoPath = saved; // relative path
        await companyService.UpdateAsync(company);

        var dto = new CompanyDto {
            Id = company.Id,
            Name = company.Name,
            TaxId = company.TaxId,
            TaxNumber = company.TaxNumber,
            Street = company.Street,
            ZipCode = company.ZipCode,
            City = company.City,
            Country = company.Country,
            Email = company.Email,
            Phone = company.Phone,
            BankName = company.BankName,
            Iban = company.Iban,
            Bic = company.Bic,
            CommercialRegister = company.CommercialRegister,
            RegisterCourt = company.RegisterCourt,
            VatExemption = company.VatExemption,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
            LogoUrl = company.LogoPath
        };

        return Ok(new { logoUrl = dto.LogoUrl });
    }

    [HttpDelete("{id}/logo")]
    public async Task<IActionResult> DeleteLogo(int id)
    {
        var company = await companyService.GetByIdAsync(id);
        if (company == null) return NotFound();

        if (!string.IsNullOrEmpty(company.LogoPath))
        {
            await fileStorage.DeleteFileAsync(company.LogoPath);
            company.LogoPath = null;
            await companyService.UpdateAsync(company);
        }

        return Ok();
    }
}
