using Microsoft.Extensions.DependencyInjection;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Workflows;

namespace PurchaseOrderApp.BL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        services.AddScoped<PurchaseOrderApprovalPolicy>();
        services.AddScoped<IPurchaseOrderWorkflowService, PurchaseOrderWorkflowService>();

        return services;
    }
}
