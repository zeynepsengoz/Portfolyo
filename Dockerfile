FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY Portfolyo.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish Portfolyo.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Portfolyo.dll"]
