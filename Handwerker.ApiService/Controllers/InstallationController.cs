using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Handwerker.Application.Services;
using Handwerker.Domain.Entities;
using Handwerker.ApiService.Models;
using Handwerker.Application.Services.Keycloak;
using Handwerker.Domain.Interfaces;

namespace Handwerker.ApiService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class InstallationController(
    SettingsService settingsService,
    CompanyService companyService,
    ProviderService providerService,
    IFileStorageService fileStorage,
    IKcUserService userService) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetInstallationStatus()
    {
        var userId = userService.GetCurrentUserId();
        var settings = await settingsService.GetSettingsAsync(userId, CancellationToken.None);
        return Ok(new { IsCompleted = settings.IsInstallationCompleted });
    }

    [HttpPost("user-data")]
    public async Task<IActionResult> SaveUserData([FromForm] InstallationUserDataDto dto)
    {
        var userId = userService.GetCurrentUserId();
        var settings = await settingsService.GetSettingsAsync(userId, CancellationToken.None);

        settings.Salutation = dto.Salutation;
        settings.Title = dto.Title;
        settings.FirstName = dto.FirstName;
        settings.LastName = dto.LastName;

        if (dto.ProfileImage != null)
        {
            var path = await fileStorage.SaveFileAsync(dto.ProfileImage.OpenReadStream(), dto.ProfileImage.FileName, dto.ProfileImage.ContentType);
            settings.ProfileImagePath = path;
        }

        await settingsService.UpdateSettingsAsync(settings, CancellationToken.None);
        return Ok();
    }

    [HttpPost("company-data")]
    public async Task<IActionResult> SaveCompanyData([FromForm] InstallationCompanyDataDto dto)
    {
        var userId = userService.GetCurrentUserId();
        var companies = await companyService.GetAllAsync();
        var company = companies.FirstOrDefault() ?? new Company { CreatedAt = DateTime.UtcNow };

        company.Name = dto.Name;
        company.Street = dto.Street;
        company.ZipCode = dto.ZipCode;
        company.City = dto.City;
        company.Phone = dto.Phone;
        company.Email = dto.Email;
        company.CommercialRegister = dto.CommercialRegister;
        company.RegisterCourt = dto.RegisterCourt;
        company.TaxId = dto.TaxId;
        company.TaxNumber = dto.TaxNumber;
        company.VatExemption = dto.VatExemption;

        if (dto.Logo != null)
        {
            var path = await fileStorage.SaveFileAsync(dto.Logo.OpenReadStream(), dto.Logo.FileName, dto.Logo.ContentType);
            company.LogoPath = path;
        }

        if (company.Id == 0)
        {
            await companyService.CreateAsync(company);
        }
        else
        {
            await companyService.UpdateAsync(company);
        }

        return Ok();
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> GetSuppliers()
    {
        var providers = await providerService.GetAllAsync();
        var dtos = providers.Select(p => new ProviderDto
        {
            Id = p.Id,
            Name = p.Name,
            Company = p.Company,
            Street = p.Street,
            ZipCode = p.ZipCode,
            City = p.City,
            Email = p.Email,
            Phone = p.Phone,
            Website = p.Website,
            TaxId = p.TaxId,
            TaxNumber = p.TaxNumber,
            CommercialRegister = p.CommercialRegister,
            RegisterCourt = p.RegisterCourt,
            Bank = new BankDto { Name = p.Bank.Name, Iban = p.Bank.Iban, Bic = p.Bank.Bic }
        });
        return Ok(dtos);
    }

    [HttpPost("suppliers")]
    public async Task<IActionResult> SaveSuppliers(InstallationSuppliersDto dto)
    {
        var allProviders = await providerService.GetAllAsync();
        var selectedIds = dto.SelectedSupplierIds ?? new List<int>();

        // Lösche alle Anbieter, die nicht ausgewählt wurden
        foreach (var provider in allProviders)
        {
            if (!selectedIds.Contains(provider.Id))
            {
                await providerService.DeleteAsync(provider.Id);
            }
        }

        return Ok();
    }

    [HttpPost("final")]
    public async Task<IActionResult> SaveFinal(InstallationFinalDto dto)
    {
        var userId = userService.GetCurrentUserId();
        var settings = await settingsService.GetSettingsAsync(userId, CancellationToken.None);

        settings.Industry = dto.Industry;
        settings.ReferralSource = dto.ReferralSource;
        settings.AvAgreementAccepted = dto.AvAgreementAccepted;
        settings.IsInstallationCompleted = true;

        await settingsService.UpdateSettingsAsync(settings, CancellationToken.None);
        return Ok();
    }
}
