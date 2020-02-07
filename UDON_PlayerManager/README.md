# Player Manager
This system allows you to keep track of all PlayerAPIs in the current instance.

## Requirements:
This system requires the Extended Udon Assembly by Toocanzs
https://github.com/Toocanzs/Udon-Extended-Assembly
This adds default variable setting and advanced labels to the serialiser which is necessary for this to work.

## How to use:
*IMPORTANT: You need to import the Extended Assembly first, as this includes an importer.*

Drag and drop the PlayerManger .euasm file into the program source slot of a UdonBehaviour on a GameObject. This will now act as your PlayerManager.

The PlayerManager has two public variables: PLAYERS and PLAYERS_AMOUNT. PLAYERS is the array that keeps track of all Player APIs. PLAYERS_AMOUNT is the variable that keeps track of how many players are in the instance.

To loop through all of the APIs set up a simple for-loop that has 0 as the start, 1 as the step and PLAYERS_AMOUNT as the end. You can then access each element in the array by that index. An example graph is provided.
