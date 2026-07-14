using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductApi.API.Filters;

/// <summary>
/// Short-circuits the pipeline with a 400 response when model binding/validation fails,
/// producing a consistent error response shape.
/// </summary>
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

            context.Result = new BadRequestObjectResult(new
            {
                title = "Validation failed.",
                status = 400,
                errors
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
