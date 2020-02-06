# VR Detection System
This system allows you to detect if a User is in VR or Desktop Mode.

## Requirements:
This system requires the Extended Udon Assembly by Toocanzs
https://github.com/Toocanzs/Udon-Extended-Assembly
This adds default variable setting to the serialiser which is necessary for this to work.

## How to use:
*IMPORTANT: You need to import the Extended Assembly first, as this includes an importer.*

Every GameObject that is subscribed to this system **needs** a Boolean Variable called "IS_VR" (in all caps) that is **public**. This is the variable that will be set to true if the User is in VR mode and false if he is in Dekstop mode (false by default).
They also **need** a custom event called "UPDATE_VR" (also in all caps). This is the event that will be called as soon as the "IS_VR" variable is set accordingly. Do not access the variable before as you might get a wrong result. (for example: Getting "IS_VR" right at start may cause it to be false, because it has not yet been set to true, but you're already accessing it before its *ready to use*, you can instead use UPDATE_VR as your "Start" for everything that is dependant on "IS_VR".)

1. Drag and drop the VR_DETECTION prefab into your Hierarchy. This should automatically come with the .euasm file as the program source. If it doesnt, or something went wrong, manually drag the .euasm file into the program source slot.
2. Drag-n-drop the VR_DETECTION_CAMERA into the testerCamera slot and the CAMERA_CONTAINER into the cameraContainer slot.
3. Set the size of the SUBSCRIBERS array as big as you need it and drag-n-drop all of the GameObjects that should be subscribed into the array slots. 

After Start the detection system will automatically set the "IS_VR" variable on all subscribed UdonBehaviours (including itself) and call "UPDATE_VR".
The array cannot be resized, but GameObjects can be added and removed as usual.
