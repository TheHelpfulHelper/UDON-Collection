GameObjects named with [] may not be renamed!

StringEvent format:
Receiver GameObject Name, Udon Component to target (0 is first, 1 is second etc.), Target: A(ll) or O(wner) OF THE RECEIVER, Event Name, Parameters (divided by vertical seperators | )

Parameters can only be literals. (You can get a reference to a GameObject via its name)

To make the system work you need to drop the THH_StringEvents prefab into your scene. Every GameObject that should be able to send and receive events need the equivalent of this code:

public UdonBehaviour EventHandler;
public string[] PARAMS;

and {SE_Handler} as a child!!