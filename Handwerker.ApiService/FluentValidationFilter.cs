namespace Handwerker.ApiService;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public sealed class FluentValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            var validator = context.HttpContext.RequestServices.GetService(validatorType);

            if (validator is null) continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await ((IValidator)validator).ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                context.Result = new BadRequestObjectResult(
                    result.Errors.Select(e => new
                    {
                        field = e.PropertyName,
                        error = e.ErrorMessage
                    })
                );
                return;
            }
        }

        await next();
    }
}
