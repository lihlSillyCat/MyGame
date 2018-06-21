#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// Base class for the generic triggering mechanism for Wwise Integration.
/// All Wwise components will use this mechanism to drive their behavior.
/// Derive from this class to add your own triggering condition, as decribed in \ref unity_add_triggers
public abstract class AkTriggerBase : MonoBehaviour 
{
	/// Delegate declaration for all Wwise Triggers.  
	public delegate void Trigger(
	GameObject in_gameObject ///< in_gameObject is used to pass "Collidee" objects when Colliders are used.  Some components have the option "Use other object", this is the object they'll use.
	);
	
	/// All components reacting to the trigger will be registered in this delegate.
	public Trigger triggerDelegate = null;
    public static Dictionary<uint, string> GetAllDerivedTypes()
	{
        return  AkInitializer.derivedTypes;
	}
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.