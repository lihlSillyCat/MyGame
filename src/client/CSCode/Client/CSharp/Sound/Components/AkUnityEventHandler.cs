#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class AkUnityEventHandler : MonoBehaviour 
{
    public const int AWAKE_TRIGGER_ID   = 1151176110;
    public const int START_TRIGGER_ID   = 1281810935;
    public const int DESTROY_TRIGGER_ID = unchecked((int)3936390293);
    public const int ENABLE_TRIGGER_ID = unchecked((int)2256756024);
    public const int DISABLE_TRIGGER_ID = 768281003;
    public const int AKTRIGGERENTER_TRIGGER_ID = unchecked((int)3592615159);
    public const int TRIGGERENTER_TRIGGER_ID = 652185819;
    public const int AKTRIGGEREXIT_TRIGGER_ID = unchecked((int)1794322239);
    public const int TRIGGEREXIT_TRIGGER_ID = 686997851;

    ///Since our mask is a 32 bits integer, we can't have more than 32 triggers
    public const int MAX_NB_TRIGGERS	= 32;	

	///Will contain the types of all the triggers derived from AkTriggerBase at runtime
	public static Dictionary<uint, string> triggerTypes = AkTriggerBase.GetAllDerivedTypes ();

	///List containing the enabled triggers.
    public List<int> triggerList = new List<int>() { START_TRIGGER_ID };

	///This property is usefull only when used with colliders.  When enabled, the target of the action will be the other colliding object.  When disabled, it will be the current object.
	public bool useOtherObject = false;

	public abstract void HandleEvent(GameObject in_gameObject);

	protected virtual void Awake()
	{
#if !DISABLE_AKSOUNDENGINE
        //RegisterTriggers(triggerList, HandleEvent);

        //Call the Handle event function if registered to the Awake Trigger
        if (triggerList.Contains(AWAKE_TRIGGER_ID))
		{
			HandleEvent(null);
		}
#endif
    }

	protected virtual void Start()
	{
#if !DISABLE_AKSOUNDENGINE
        //Call the Handle event function if registered to the Start Trigger
        if (triggerList.Contains(START_TRIGGER_ID))
		{
			HandleEvent(null);
		}
#endif
    }

    private bool didDestroy = false;
	protected virtual void OnDestroy()
	{
#if !DISABLE_AKSOUNDENGINE
        if (didDestroy == false)
        {
			DoDestroy();
		}
#endif
    }

    public void DoDestroy()
	{
#if !DISABLE_AKSOUNDENGINE
        //UnregisterTriggers (triggerList, HandleEvent);

        if (triggerList.Contains(DESTROY_TRIGGER_ID))
		{
			HandleEvent(null);
		}
		
		didDestroy = true;
#endif
    }

    protected virtual void OnEnable()
    {
#if !DISABLE_AKSOUNDENGINE
        if (triggerList.Contains(ENABLE_TRIGGER_ID))
        {
            HandleEvent(null);
        }
#endif
    }

    protected virtual void OnDisable()
    {
#if !DISABLE_AKSOUNDENGINE
        if (triggerList.Contains(DISABLE_TRIGGER_ID))
        {
            HandleEvent(null);
        }
#endif
    }

    protected virtual void OnTriggerEnter(Collider in_other)
    {
#if !DISABLE_AKSOUNDENGINE
        if (triggerList.Contains(AKTRIGGERENTER_TRIGGER_ID) || triggerList.Contains(TRIGGERENTER_TRIGGER_ID))
        {
            HandleEvent(in_other.gameObject);
        }
#endif
    }

    protected virtual void OnTriggerExit(Collider in_other)
    {
#if !DISABLE_AKSOUNDENGINE
        if (triggerList.Contains(AKTRIGGEREXIT_TRIGGER_ID) || triggerList.Contains(TRIGGEREXIT_TRIGGER_ID))
        {
            HandleEvent(in_other.gameObject);
        }
#endif
    }

    //protected void RegisterTriggers(List<int> in_triggerList, AkTriggerBase.Trigger in_delegate)
    //{
    //    //Register to the appropriate triggers
    //    foreach (uint triggerID in in_triggerList)
    //    {
    //        string triggerName = string.Empty;
    //        if (triggerTypes.TryGetValue(triggerID, out triggerName) != false)
    //        {
    //            // These special triggers are handled differently
    //            if (triggerName == "Awake" || triggerName == "Start" || triggerName == "Destroy" || triggerName == "Enable" || triggerName == "Disable")
    //            {
    //                continue;
    //            }

    //            AkTriggerBase trigger = (AkTriggerBase)GetComponent(Type.GetType(triggerName));
    //            if (trigger == null)
    //            {
    //                trigger = (AkTriggerBase)gameObject.AddComponent(Type.GetType(triggerName));
    //            }

    //            trigger.triggerDelegate += in_delegate;
    //        }
    //    }
    //}

    //protected void UnregisterTriggers(List<int> in_triggerList, AkTriggerBase.Trigger in_delegate)
    //{
    //    //Unregister all the triggers and delete them if no one else is registered to them
    //    foreach (uint triggerID in in_triggerList)
    //    {
    //        string triggerName = string.Empty;
    //        if (triggerTypes.TryGetValue(triggerID, out triggerName) != false)
    //        {
    //            // These special triggers are handled differently
    //            if (triggerName == "Awake" || triggerName == "Start" || triggerName == "Destroy" || triggerName == "Enable" || triggerName == "Disable")
    //            {
    //                continue;
    //            }

    //            AkTriggerBase trigger = (AkTriggerBase)GetComponent(Type.GetType(triggerName));

    //            if (trigger != null)
    //            {
    //                trigger.triggerDelegate -= in_delegate;

    //                if (trigger.triggerDelegate == null)
    //                {
    //                    Destroy(trigger);
    //                }
    //            }
    //        }
    //    }
    //}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.