FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AestheticStudySpace.sln ./
COPY src/AestheticStudySpace.Domain/AestheticStudySpace.Domain.csproj src/AestheticStudySpace.Domain/
COPY src/AestheticStudySpace.Application/AestheticStudySpace.Application.csproj src/AestheticStudySpace.Application/
COPY src/AestheticStudySpace.Infrastructure/AestheticStudySpace.Infrastructure.csproj src/AestheticStudySpace.Infrastructure/
COPY src/AestheticStudySpace.Api/AestheticStudySpace.Api.csproj src/AestheticStudySpace.Api/

RUN dotnet restore src/AestheticStudySpace.Api/AestheticStudySpace.Api.csproj

COPY src/ src/
RUN dotnet publish src/AestheticStudySpace.Api/AestheticStudySpace.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "AestheticStudySpace.Api.dll"]
