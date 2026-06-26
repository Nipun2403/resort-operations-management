using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;

namespace HotelManagement.API.Filters;

public sealed class IdempotentAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey))
        {
            await next();
            return;
        }

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var keyString = idempotencyKey.ToString();

        if (await dbContext.IdempotentRequests.FindAsync(keyString) != null)
        {
            context.Result = new ConflictObjectResult(new { Message = "Duplicate request detected." });
            return;
        }

        var executedContext = await next();

        if (executedContext.Exception == null && executedContext.Result is ObjectResult objectResult)
        {
            // Only cache if it was a successful request (e.g. 200/201)
            var statusCode = objectResult.StatusCode;
            if (statusCode == null || (statusCode >= 200 && statusCode < 300))
            {
                dbContext.IdempotentRequests.Add(new IdempotentRequest 
                { 
                    IdempotencyKey = keyString, 
                    Path = context.HttpContext.Request.Path 
                });
                await dbContext.SaveChangesAsync();
            }
        }
        else if (executedContext.Exception == null && executedContext.Result is StatusCodeResult statusCodeResult)
        {
            if (statusCodeResult.StatusCode >= 200 && statusCodeResult.StatusCode < 300)
            {
                dbContext.IdempotentRequests.Add(new IdempotentRequest 
                { 
                    IdempotencyKey = keyString, 
                    Path = context.HttpContext.Request.Path 
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
