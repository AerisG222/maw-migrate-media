#!/bin/bash

podman container ps -f name=legacy-migration-pgsql |grep legacy-migration-pgsql

if [ $? -ne 0 ]; then
    podman run \
        --detach \
        --rm \
        --name legacy-migration-pgsql \
        --userns "keep-id:uid=999" \
        --volume "$(pwd)/legacy-pg/pg:/var/lib/postgresql/data:Z" \
        --security-opt label=disable \
        --publish 9898:5432 \
        --env POSTGRES_PASSWORD=mysecretpassword \
        docker.io/library/postgres:17 \
            postgres
fi

echo 'starting restore'

pg_restore \
    -U postgres \
    -h localhost \
    -p 9898 \
    --create \
    --dbname postgres \
    /home/mmorano/git/maw-migrate-media/legacy-pg/backup/maw_website.20250922.dump

echo 'completed restore'
