FROM microsoft/dotnet:2-sdk-jessie
WORKDIR /app
COPY /bin/Debug/netcoreapp2.0/publish ./
ENTRYPOINT ["dotnet", "CommandScaler.Sample.API.dll"]