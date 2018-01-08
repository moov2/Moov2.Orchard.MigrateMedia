# Moov2.Orchard.MigrateMedia

Easily transfer files between local file system and Azure blob storage through the Orchard admin area.

## Getting Setup

Download module source code and place within the "Modules" directory of your Orchard installation.

Alternatively, use the command below to add this module as a sub-module within your Orchard project.

    git submodule add git@github.com:moov2/Moov2.Orchard.MigrateMedia.git modules/Moov2.Orchard.MigrateMedia

# Usage

Enable the "Moov2.Orchard.MigrateMedia" module. Once enabled a "Migrate" sub-menu option in the admin dashboard will be added to the "Media" menu option. From here you can configure a media migration transferring media from the file system to Azure blob storage or the other way. The module will automatically pick up a configured Azure blob storage connection string if one is present in the `Web.config`. However, you can define another if desired. Once submitted it may take a while to perform the migration dependant on how many media items need to be migrated.

Once the migration has completed it'll present a success message detailing how many items were transferred. If configured not to overwrite, it'll also include information on how many media items were ignored because a file already exists.

![alt text](https://raw.githubusercontent.com/moov2/Moov2.Orchard.MigrateMedia/master/docs/demo-module-features.gif "Example of module feature")
