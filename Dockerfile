FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

RUN apt-get update && apt-get install -y tzdata
ENV TZ=America/Sao_Paulo

WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/FundoInvestimento.Api/FundoInvestimento.Api.csproj", "FundoInvestimento.Api/"]
COPY ["src/FundoInvestimento.Application/FundoInvestimento.Application.csproj", "FundoInvestimento.Application/"]
COPY ["src/FundoInvestimento.Domain/FundoInvestimento.Domain.csproj", "FundoInvestimento.Domain/"]
COPY ["src/FundoInvestimento.Infrastructure/FundoInvestimento.Infrastructure.csproj", "FundoInvestimento.Infrastructure/"]
COPY ["src/FundoInvestimento.Libs/FundoInvestimento.Libs.csproj", "FundoInvestimento.Libs/"]

RUN dotnet restore "FundoInvestimento.Api/FundoInvestimento.Api.csproj"

COPY src/ .

WORKDIR "/src/FundoInvestimento.Api"

RUN dotnet publish "FundoInvestimento.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FundoInvestimento.Api.dll"]