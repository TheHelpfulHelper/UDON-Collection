**THH_StringEvents v1.2**

Purpose: Sending parameterized events over the network. Very experimental!
Complexity Level: Very High

Explanatory Video available at: https://youtu.be/DkyhF5d9st8
(Some things have changed since the Video was made, this README overules anything said in the video!)

*SETUP:*
Drag and drop the 'THH_StringEvents' prefab from the Prefabs folder into your scene.

Every program that should be able to send and receive events will need these variables (exact name, type and access modifier):
public UdonBehaviour EventHandler;
public string[] PARAMS;

And a '{SE_Handler}' object as a *direct* child (also found in the Prefabs folder).


Objects named with [] may not be renamed!

StringEvent format:
Receiver GameObject Name, UdonBehaviour component to target (0 is first, 1 is second etc.), Target: A(ll) or O(wner) OF THE RECEIVER, Event Name, Parameters (divided by vertical seperators | )

Parameters can only be literals. (You can get a reference to a GameObject via its name)

Important Note:
Due to variable sync updates being unreliable, it is not recommended to use this for frequent updates. There should be at least a second of delay between events.
