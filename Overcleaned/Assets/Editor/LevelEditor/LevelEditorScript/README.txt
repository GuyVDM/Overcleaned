//LevelEditor Instructions

- Creating Tiles -

Step 1. To add new tiles to the level editor, simply create a prefab with it's anchor set correctly, and add the 'Tile.cs' component to the parent (Can be found at the root of the package).
Step 2. Once the prefab has been created, you can simply add them in the configuration file located in the /Configurations/ Folder.

- Adding Materials -

//NOTE: The Editor assumes that all the tiles share 1 UV, this means that the shared material will be adjusted so it'll be applied to all the tiles using that UV at once.

Step 1. Create a material
Step 2. Go to the Configurations located at /Configurations/
Step 3. Add the material to the Array of themes
