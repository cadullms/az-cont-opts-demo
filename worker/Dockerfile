# Stage 1 - Build the car (using the sdk, our factory)
FROM microsoft/dotnet:2.2-sdk AS builder 
WORKDIR /source
COPY . .
RUN dotnet publish --output package

# Stage 2 - Ship the car (but not the factory)
FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /source/package .
ENTRYPOINT ["dotnet", "worker.dll"]