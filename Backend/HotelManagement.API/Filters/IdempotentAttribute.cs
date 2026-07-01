using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace HotelManagement.API.Filters;

public sealed class IdempotentAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Skip if endpoint opts out
        var skip = context.ActionDescriptor.EndpointMetadata
            .Any(m => m is SkipIdempotencyAttribute);
        if (skip)
        {
            await next();
            return;
        }

        var method = context.HttpContext.Request.Method;
        if (!HttpMethods.IsPost(method) && !HttpMethods.IsPut(method) && !HttpMethods.IsPatch(method))
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var rawKey) || string.IsNullOrWhiteSpace(rawKey))
        {
            context.Result = new BadRequestObjectResult(new { Message = "X-Idempotency-Key header is required for mutation requests." });
            return;
        }

        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var effectiveKey = string.IsNullOrEmpty(userId)
            ? $"anon:{rawKey}"
            : $"{userId}:{rawKey}";

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

        var existing = await dbContext.IdempotentRequests.FindAsync(effectiveKey);
        if (existing != null)
        {
            context.Result = new ContentResult
            {
                Content = existing.ResponseBody,
                ContentType = "application/json",
                StatusCode = existing.StatusCode
            };
            return;
        }

        var executedContext = await next();

        if (executedContext.Exception != null)
            return;

        int? statusCode = null;
        string? responseBody = null;

        if (executedContext.Result is ObjectResult objectResult)
        {
            statusCode = objectResult.StatusCode ?? 200;
            if (statusCode >= 200 && statusCode < 300)
                responseBody = JsonSerializer.Serialize(objectResult.Value);
        }
        else if (executedContext.Result is StatusCodeResult statusCodeResult)
        {
            statusCode = statusCodeResult.StatusCode;
            if (statusCode >= 200 && statusCode < 300)
                responseBody = string.Empty;
        }

        if (statusCode == null || responseBody == null)
            return;

        dbContext.IdempotentRequests.Add(new IdempotentRequest
        {
            IdempotencyKey = effectiveKey,
            Path = context.HttpContext.Request.Path,
            StatusCode = statusCode.Value,
            ResponseBody = responseBody
        });

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Concurrent duplicate request won the race — the key is already stored, which is fine
        }
    }
}
