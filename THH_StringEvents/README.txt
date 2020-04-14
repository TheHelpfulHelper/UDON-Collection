Objects named with [] may not be renamed!

StringEvent format:
Receiver GameObject Name, Udon Component to target (0 is first, 1 is second etc.), Target: A(ll) or O(wner) OF THE RECEIVER, Event Name, Parameters (divided by commas)

Parameters can only be literals.

Objects that should be able to receive and send events need this:

// These members are required for the StringEventSystem
    private UdonBehaviour EventHandler;
    public string[] PARAMS;
    void GetEventHandler()
    {
        EventHandler = (UdonBehaviour)gameObject.transform.Find("[EventHandler]").GetComponent(typeof(UdonBehaviour));
    }
    void SendEvent(string eventString)
    {
        EventHandler.SetProgramVariable("EventOutbox", eventString);
    }

AND GetEventHandler() must be called in Start!