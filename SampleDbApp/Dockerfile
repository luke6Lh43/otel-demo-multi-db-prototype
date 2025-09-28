FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release --use-current-runtime -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

RUN useradd -m appuser
RUN chmod 755 ./instrument.sh && chown -R appuser /app

USER appuser

ENV OTEL_SERVICE_NAME=sampledbapp \
    OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4318 \
    OTEL_EXPORTER_OTLP_PROTOCOL=http/protobuf \
    OTEL_TRACES_EXPORTER=otlp

ENTRYPOINT ["./instrument.sh", "dotnet", "SampleDbApp.dll"]