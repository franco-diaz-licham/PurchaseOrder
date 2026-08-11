# Build the API with the full .NET SDK.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first so Docker can cache package restore.
COPY backend/PurchaseOrderApp.Domain/PurchaseOrderApp.Domain.csproj backend/PurchaseOrderApp.Domain/
COPY backend/PurchaseOrderApp.Application/PurchaseOrderApp.Application.csproj backend/PurchaseOrderApp.Application/
COPY backend/PurchaseOrderApp.Infrastructure/PurchaseOrderApp.Infrastructure.csproj backend/PurchaseOrderApp.Infrastructure/
COPY backend/PurchaseOrderApp.Api/PurchaseOrderApp.Api.csproj backend/PurchaseOrderApp.Api/

RUN dotnet restore backend/PurchaseOrderApp.Api/PurchaseOrderApp.Api.csproj

# Copy the source code after restore and publish the API.
COPY backend/ backend/

RUN dotnet publish backend/PurchaseOrderApp.Api/PurchaseOrderApp.Api.csproj -c Release -o /app/publish --no-restore

# Run the published output on the smaller ASP.NET runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# The compose file maps host port 5180 to container port 8080.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PurchaseOrderApp.Api.dll"]
