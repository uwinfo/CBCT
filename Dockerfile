FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY CBCT.sln .
COPY Core/Core.csproj ./Core/
COPY AdminApi/AdminApi.csproj ./AdminApi/
COPY Su/Su.csproj ./Su/
RUN dotnet restore AdminApi/AdminApi.csproj

# Copy everything else and build
COPY AdminApi/ AdminApi/
COPY Core/ Core/
COPY Su/ Su/
RUN dotnet publish AdminApi/AdminApi.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
RUN echo "Asia/Taipei" > /etc/timezone
ENV TZ Asia/Taipei
ENV LANG zh_TW.UTF-8

WORKDIR /app
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "AdminApi.dll"]
