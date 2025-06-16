# MaW Migration Tools

The following tools will help manage different parts of the restructuring of the media.

## Step 1: MaWDbMigrate

Silly little tool to migrate data to new schema.

## Step 2: MawMediaMigrate

Tool to move / rename / resize files to the new structure and sizes.
This also runs exiftool for all 'src' media to export a json file to be later loaded into the new media database

## Step 3: MawMediaSqlUpdate

Tool to update database based on results of the media migration tool.
