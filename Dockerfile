#this dockerfile is for deploying the API in production manner but should also be compliant with Mac OS local production building for test prod before pushing to the cloud

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY ["./", "."]
ARG TARGETARCH
RUN DOTNET_ARCH=$([ "$TARGETARCH" = "amd64" ] && echo "x64" || echo "$TARGETARCH") && \
    dotnet publish "API/API.csproj" \
    -c Release \
    -o /app/publish \
    --runtime linux-musl-${DOTNET_ARCH} \
    --self-contained true \
    /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["./API"]