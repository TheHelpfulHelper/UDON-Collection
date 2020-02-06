# Subscription System
This system allows you to send a custom event to as many GameObjects as you like that have been stored in an Array.

## Requirements:
This system requires the Extended Udon Assembly by @Toocanzs
https://github.com/Toocanzs/Udon-Extended-Assembly
This adds default variable setting to the serialiser which is necessary for this to work.

## How to use:
*IMPORTANT: You need to import the Extended Assembly first, since this includes an importer.*
1. Drag and drop the imported .euasm file into a program source slot of a UdonBehaviour component.
This will now act as your subscription system. 
2. Set the size of the array as big as you need it and drag-n-drop all of the GameObjects that should be subscribed into the array slots. 
3. Set the event name to whatever event you want to be called on all of the GameObjects.

Now whenever you send a custom event named "PUBLISH" (in all caps) to the subscription system, it will send an event with the specified name to all subscribers.
The array cannot be resized, but GameObjects can be added and removed as usual.