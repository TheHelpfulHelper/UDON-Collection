Explanation Video available at: 
https://youtu.be/DkyhF5d9st8
(Some of the explanations have changed! This document will overule whatever is said in the video)

Objects named with [] may not be renamed!

StringEvent format:
Receiver GameObject Name, Udon Component to target (0 is first, 1 is second etc.), Target: A(ll) or O(wner) OF THE RECEIVER, Event Name, Parameters (divided by vertical seperators | )

Parameters can only be literals. (You can get a reference to a GameObject via its name)

*Setup:*
Drop the 'THH_StringEvents' prefab into your scene.
If your program should be able to receive and send events, it will need these variables:

public UdonBehaviour EventHandler;
public string[] PARAMS;

And and an'{SE_Handler}' object as a direct child!