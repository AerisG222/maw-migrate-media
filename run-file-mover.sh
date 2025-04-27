#!/bin/bash

dotnet src/MawFileMover/bin/Debug/net9.0/MawFileMover.dll \
    /data/www/website_assets \
    /data/media \
    /data/media-migration.csv
