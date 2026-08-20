using Microsoft.AspNetCore.Mvc.Filters;
using ZeissAssessment.Application.Exceptions;

namespace ZeissAssessment.Filters;

public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

        throw new ValidationException(errors);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
