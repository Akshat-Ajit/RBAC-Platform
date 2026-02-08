FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ERBMS.sln .
COPY ERBMS.Domain/ERBMS.Domain.csproj ERBMS.Domain/
COPY ERBMS.Application/ERBMS.Application.csproj ERBMS.Application/
COPY ERBMS.Infrastructure/ERBMS.Infrastructure.csproj ERBMS.Infrastructure/
COPY ERBMS.API/ERBMS.API.csproj ERBMS.API/
RUN dotnet restore
COPY . .
WORKDIR /src/ERBMS.API
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ERBMS.API.dll"]
