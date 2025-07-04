#!/bin/bash
ROOTDIR=$(pwd)

# db migrate
echo "-- DB MIGRATE --"
cd src/MawDbMigrate

dotnet run \
    "${MAW_API_Environment__DbConnectionString}" \
    "${ROOTDIR}/output-db-migrate/"

cd -

# migrate media
echo "-- MIGRATE MEDIA --"
cd src/MawMediaMigrate

dotnet run \
    /data/www/website_assets/ \
    /data/www/media \
    "${ROOTDIR}/output-migrate-dry-run/" \
    --dry-run

cd -

# sql update
echo "-- SQL UPDATE --"
cd src/MawMediaSqlUpdate

dotnet run \
    /data/www/website_assets/ \
    /data/www/media \
    "${ROOTDIR}/output-migrate-dry-run/" \
    "${ROOTDIR}/output-sql-update/"

cd -

chmod u+x "${ROOTDIR}/output-db-migrate/import.sh"
chmod u+x "${ROOTDIR}/output-sql-update/import.sh"

echo "** complete **"
echo ""
echo "to update dev database, review and execute the following in their directories:"
echo "    - output-db-migrate/import.sh"
echo "    - output-sql-update/import.sh"
echo ""
