# ===========================
# Build Stage
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src


# Copy solution
COPY SolarECommerce.sln .


# Copy Product API project files
COPY Product.API/Product.API.csproj Product.API/

COPY Product.Application/Product.Application.csproj Product.Application/

COPY Product.Infrastructure/Product.Infrastructure.csproj Product.Infrastructure/

COPY Product.Domain/Product.Domain.csproj Product.Domain/


# Copy Common if referenced
COPY Common/ Common/


# Restore
RUN dotnet restore Product.API/Product.API.csproj


# Copy all source files
COPY . .


# Build and publish
WORKDIR /src/Product.API

RUN dotnet publish Product.API.csproj \
    -c Release \
    -o /app/publish


# ===========================
# Runtime Stage
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Product.API.dll"]