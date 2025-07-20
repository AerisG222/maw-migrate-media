#!/bin/bash
ROOTDIR=$(pwd)

# db migrate
#echo "-- DB MIGRATE --"
#cd src/MawDbMigrate

#dotnet run \
#    "${MAW_API_Environment__DbConnectionString}" \
#    "${ROOTDIR}/output-db-migrate/"

#cd -

# migrate media
echo "-- MIGRATE MEDIA --"
./src/MawMediaMigrate/bin/Release/net9.0/MawMediaMigrate \
    /data/www/website_assets \
    /data/maw-media-assets \
    "${ROOTDIR}/output-migrate-media/"

# sql update
#echo "-- SQL UPDATE --"
#cd src/MawMediaSqlUpdate

#dotnet run \
#    /data/www/website_assets \
#    /data/maw-media-assets \
#    "${ROOTDIR}/output-migrate-media/" \
#    "${ROOTDIR}/output-sql-update/"

#cd -

#chmod u+x "${ROOTDIR}/output-db-migrate/import.sh"
#chmod u+x "${ROOTDIR}/output-sql-update/import.sh"

echo "** complete **"
echo ""
#echo "to update dev database, review and execute the following in their directories:"
#echo "    - output-db-migrate/import.sh"
#echo "    - output-sql-update/import.sh"
#echo ""
