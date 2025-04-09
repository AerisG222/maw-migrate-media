#!/bin/bash

podman run \
    -it \
    --rm \
    --pod dev-maw-pod \
    --volume "$(pwd):/data" \
    --workdir /data \
    --security-opt label=disable \
    mcr.microsoft.com/dotnet/sdk:9.0 \
    dotnet src/MawDbMigrateTool/bin/Debug/net9.0/MawDbMigrateTool.dll \
        "${MAW_API_Environment__DbConnectionString}" \
        "/data/migrate.sql"
