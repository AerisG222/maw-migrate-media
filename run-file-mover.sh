#!/bin/bash

dotnet src/MawMediaMigrate/bin/Debug/net9.0/MawMediaMigrate.dll \
    /data/www/website_assets \
    /data/maw-media-assets \
    /data/maw-media-migration.csv
