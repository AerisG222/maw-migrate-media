#!/bin/bash

# this references the local dev container that is not up to date
# podman run \
#     -it \
#     --rm \
#     --pod dev-maw-pod \
#     --volume "$(pwd):/data" \
#     --workdir /data \
#     --security-opt label=disable \
#     mcr.microsoft.com/dotnet/sdk:9.0 \
#     dotnet src/MawDbMigrate/bin/Debug/net9.0/MawDbMigrate.dll \
#         "${MAW_API_Environment__DbConnectionString}" \
#         "/data/_output"


# this references a fresh backup that is running locally and exposed on port 9898 per 'restore-legacy-backup.sh'
cd src/MawDbMigrate

dotnet run \
    "Server=localhost;Port=9898;Database=maw_website;Username=postgres;Password=mysecretpassword;" \
    ../../output-db-migrate
