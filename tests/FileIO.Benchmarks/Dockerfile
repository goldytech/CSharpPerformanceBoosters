FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["tests/FileIO.Benchmarks/FileIO.Benchmarks.csproj", "FileIO.Benchmarks/"]
RUN dotnet restore "tests/FileIO.Benchmarks/FileIO.Benchmarks.csproj"
COPY . .
WORKDIR "/src/FileIO.Benchmarks"
RUN dotnet build "FileIO.Benchmarks.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileIO.Benchmarks.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileIO.Benchmarks.dll"]
