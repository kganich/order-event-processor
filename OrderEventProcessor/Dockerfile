FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OrderEventProcessor/OrderEventProcessor.csproj", "OrderEventProcessor/"]
RUN dotnet restore "OrderEventProcessor/OrderEventProcessor.csproj"
COPY . ./
RUN dotnet build "OrderEventProcessor/OrderEventProcessor.csproj" -c Release -o /app/build

RUN dotnet publish "OrderEventProcessor/OrderEventProcessor.csproj" -c Release -o /app/publish

WORKDIR /app/publish

ENTRYPOINT [ "dotnet", "OrderEventProcessor.dll" ]