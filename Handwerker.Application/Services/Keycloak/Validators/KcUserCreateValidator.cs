using FluentValidation;
using Handwerker.Application.Services.Keycloak.Models;

namespace Handwerker.Application.Services.Keycloak.Validators;

public class KcUserRequestValidator : AbstractValidator<KcUserDto>
{
    public KcUserRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty();
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}