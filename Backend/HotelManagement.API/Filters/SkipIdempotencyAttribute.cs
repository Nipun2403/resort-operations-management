namespace HotelManagement.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipIdempotencyAttribute : Attribute { }
