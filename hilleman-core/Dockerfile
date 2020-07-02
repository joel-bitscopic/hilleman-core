FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 50501
EXPOSE 44388

COPY . .

FROM base AS final
WORKDIR /app
# COPY --from=publish /app .
ENTRYPOINT ["dotnet", "hillemancore.dll"]

# NOTES
# use DOTNET PUBLISH -c Release command like usual to build files, edit appsettings.json to reflect path of database w/ mounted volume
# ensure DB cofig file is pointing to mapped path, too
# build Dockerfile (docker build -t hilleman:hilleman4.2.0 .) <-- build w/ tag at current dir
# docker volume create test-volume 
# docker volume inspect
# docker run --mount source=test-volume,target=C:\ProgramData\Docker\volumes\test-volume\_data -p 80:80  hilleman:hilleman4.2.0
# docker container ls <-- fetch running containers to obtain IP
# navigate w/ browser to http://<ip>/swagger to verify app is up and running!
# unanswered questions:
# 1) project is configured to listen on port 50501, 44388 but appears to start on port 80 - not sure why this is... note the comment command above shows mapping host 80 to container 80
# 1-followup) maybe try this if it really matters what the internal port is https://stackoverflow.com/questions/48669548/why-does-aspnet-core-start-on-port-80-from-within-docker/48669703
# 2) how to make docker use local cert store