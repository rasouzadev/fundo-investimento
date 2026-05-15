FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["FundoInvestimento.Api/FundoInvestimento.Api.csproj", "FundoInvestimento.Api/"]
COPY ["FundoInvestimento.Application/FundoInvestimento.Application.csproj", "FundoInvestimento.Application/"]
COPY ["FundoInvestimento.Domain/FundoInvestimento.Domain.csproj", "FundoInvestimento.Domain/"]
COPY ["FundoInvestimento.Infrastructure/FundoInvestimento.Infrastructure.csproj", "FundoInvestimento.Infrastructure/"]

RUN dotnet restore "FundoInvestimento.Api/FundoInvestimento.Api.csproj"

COPY . .
WORKDIR "/src/FundoInvestimento.Api"
RUN dotnet build "FundoInvestimento.Api.csproj" -c Release -o /app/build

RUN dotnet publish "FundoInvestimento.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FundoInvestimento.Api.dll"]