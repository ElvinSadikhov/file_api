# Build section
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app
COPY . .

RUN dotnet restore
RUN dotnet publish -o out -c Release

# Run section
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet","RestAPI.dll","--environment=Production"]

# docker build -t image-name .
# docker run -d -p 5102:8080 --name container-name image-name
# docker images
# docker ps -a
# (for deletion) docker rmi image-name / docker rm container-name
# (to get into terminal) docker exec -it container-name sh

# docker login custom-register
# docker buildx build --platform linux/amd64,linux/arm64 -t custom-register/image-name .
# docker push custom-register/image-name
# (docker pull custom-register/image-name)


# docker login
# docker buildx build --platform linux/amd64 -t elvinsadikhov/default-priv-repo:lowkee-ai_prod_latest .
# docker push elvinsadikhov/default-priv-repo:lowkee-ai_prod_latest