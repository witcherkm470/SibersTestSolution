FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SibersTestSolution.sln ./
COPY appsettings.json ./
COPY SibersTestSolution.Api/SibersTestSolution.Api.csproj SibersTestSolution.Api/
COPY SibersTestSolution.Application/SibersTestSolution.Application.csproj SibersTestSolution.Application/
COPY SibersTestSolution.Domain/SibersTestSolution.Domain.csproj SibersTestSolution.Domain/
COPY SibersTestSolution.Infrastructure/SibersTestSolution.Infrastructure.csproj SibersTestSolution.Infrastructure/

RUN dotnet restore SibersTestSolution.Api/SibersTestSolution.Api.csproj

COPY . .
RUN dotnet publish SibersTestSolution.Api/SibersTestSolution.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN mkdir -p /data/project-documents

COPY --from=build /app/publish .
COPY --from=build /src/appsettings.json /appsettings.json

EXPOSE 8080
ENTRYPOINT ["dotnet", "SibersTestSolution.Api.dll"]
