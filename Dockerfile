FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ProductInventory.slnx ./
COPY src/Products.Api/Products.Api.csproj src/Products.Api/
COPY src/Products.Application/Products.Application.csproj src/Products.Application/
COPY src/Products.Domain/Products.Domain.csproj src/Products.Domain/
COPY src/Products.Infrastructure/Products.Infrastructure.csproj src/Products.Infrastructure/
COPY tests/Products.Tests/Products.Tests.csproj tests/Products.Tests/

RUN dotnet restore ProductInventory.slnx

COPY . .

RUN dotnet publish src/Products.Api/Products.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Products.Api.dll"]
