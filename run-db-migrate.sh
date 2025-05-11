#!/bin/bash

podman run \
    -it \
    --rm \
    --pod dev-maw-pod \
    --volume "$(pwd):/data" \
    --workdir /data \
    --security-opt label=disable \
    mcr.microsoft.com/dotnet/sdk:9.0 \
    dotnet src/MawDbMigrate/bin/Debug/net9.0/MawDbMigrate.dll \
        "${MAW_API_Environment__DbConnectionString}" \
        "/data/_output"
