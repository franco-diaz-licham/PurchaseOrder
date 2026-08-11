using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace PurchaseOrderApp.Api.Configuration;

public sealed class RouteTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is null) return null;

        var route = value.ToString()!;
        route = Regex.Replace(route, "([a-z0-9])([A-Z])", "$1-$2");
        route = Regex.Replace(route, "([A-Z])([A-Z][a-z])", "$1-$2");

        return route.ToLowerInvariant();
    }
}
