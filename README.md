# MaW Migration Tools

The following tools will help manage different parts of the restructuring of the media.

## MaW DB Migrate Tool

Silly little tool to migrate data to new schema.

## MawFileMover

Tool to move / rename / resize files to the new structure and sizes.
Will likely rescale images to new dimensions in avif
Will likely rescale videos to new dimensions in av1

## MawExifExport

Runs exiftool for all 'src' media to export a json file to be later loaded into the new media database

## MawExifImport

TBD - Tool to import exif json documents into pg
