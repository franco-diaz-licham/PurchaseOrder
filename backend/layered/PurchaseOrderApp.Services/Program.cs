using FluentValidation;
using PurchaseOrderApp.BL;
using PurchaseOrderApp.DAL;
using PurchaseOrderApp.Services.Configuration;
using PurchaseOrderApp.Services.Validation;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<RequestValidationFilter>();
builder.Services.AddValidatorsFromAssemblyContaining<SubmitPurchaseOrderRequestValidator>();

builder.Services.AddControllers(options => {
    options.Filters.Add<RequestValidationFilter>();
}).AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
});
builder.Services.AddBusinessLayer();
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services
    .AddOptions<CorsOptions>()
    .Bind(builder.Configuration.GetSection(CorsOptions.SectionName))
    .Validate(options => options.AllowedOrigins.All(origin => !string.IsNullOrWhiteSpace(origin)), "Cors origins cannot be empty.")
    .ValidateOnStart();

var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
builder.Services.AddCors(options => {
    options.AddPolicy(CorsOptions.PolicyName, policy => {
        policy
            .WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.MigrateDatabaseAsync();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseApiExceptionHandling();
app.UseHttpsRedirection();
app.UseCors(CorsOptions.PolicyName);
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

public partial class Program;
