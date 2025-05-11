#!/bin/bash

dotnet src/MawMediaMigrate/bin/Debug/net9.0/MawMediaMigrate.dll \
    /data/www/website_assets \
    /data/media \
    /data/media-migration.csv
