FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build-env
WORKDIR /app

COPY . .
RUN dotnet restore Steamboat/Steamboat.csproj
RUN dotnet publish -c Release -o ./publish -r linux-musl-x64 --self-contained=false Steamboat/Steamboat.csproj

FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine

WORKDIR /app
COPY --from=build-env /app/publish .

ENTRYPOINT ["dotnet", "Steamboat.dll"]