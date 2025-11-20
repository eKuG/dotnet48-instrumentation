# Build Stage
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2019 AS build

WORKDIR /src

COPY Net48OtelSignozDemo.csproj .
RUN nuget restore Net48OtelSignozDemo.csproj

COPY . .
RUN msbuild Net48OtelSignozDemo.csproj /p:Configuration=Release /p:Platform="Any CPU"

# Runtime Stage
FROM mcr.microsoft.com/dotnet/framework/runtime:4.8-windowsservercore-ltsc2019

WORKDIR /app

COPY --from=build /src/bin/Release/ .

ENTRYPOINT ["Net48OtelSignozDemo.exe"]