**THH_PpOP v1.0**

Purpose: PpOP stands for 'Per-player Object Pooled'. After joining each player is assigned an object out of a pool, which they become owner of. After leaving the object is released and can be assigned again. <br/>
Complexity Level: Moderate/High

*SETUP:*
Drag and drop the 'THH_PpOP' prefab from the Prefabs folder into your scene.
It contains a single PpOP_Handler. When a player joins the system tries to find an "unowned" Handler and the player will get Ownership of the Handler as well as ALL childs of it.
This means that you need 2 * (Maximum Amount of Players) + 1 Handlers. (Soft cap is the maximum amount of players, however in VRChat you can still join a "full" world if you directly join a friend up to twice the soft cap + the creator of the instance is always able to join hence the plus 1)

If there is no more unowned handlers the player simply will never receive a handler, so make sure there is definitely enough.

After setting up the first handler you can simply Ctrl + D it until you have enough Handlers.
