# SpritesheetCropper
## Overciew
This tool has been developed to simplify the proces ofmetadata extraction from a single Spritesheet.

### Behaviour
Once the spritesheet (.jpg .png) is loaded, the user can crop a single sprite on the SP, and select its frame value,
simultaneously the system write the sprite metadata on the table at the right side of the screen.
Once the sprite is `cropped` the user can crop the next sprite.
As a final result of clippings the user will have a list of metadata sprite, that he can save as a file (.txt)

### Metadata
With the expression `sprite metadata` we want denote a tuple of integers in such format <br /> 
**A:B:C:D:E:F:G;**

A - sprite X-coordinate (the top left corner) <br /> 
B - sprite Y-coordinate (the top left corner) <br /> 
C - sprite width <br /> 
D - sprite height <br /> 
E - pivot X-coordinate <br /> 
F - pivot Y-coordinate <br /> 
G - sprite frame duration <br /> 

