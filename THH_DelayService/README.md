**DelayService v1.3**

Purpose: Delay events by a chosen amount of time in seconds.<br/>
Complexity Level: Moderate

*SETUP:* Drag and drop the '[DS_Manager]' prefab from the Prefabs folder into your scene.

Every UdonBehaviour that wants to use the DelayService needs to have a '{DS_Handler}' object as a direct child (also found in the Prefabs folder)
and a 'public UdonBehaviour DS_Manager' variable.

Set the MAX_TRACKED_EVENTS value on the Manager to whatever you seem fit. By default a maximum of 100 events can be tracked at the same time.
