#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[AddComponentMenu("Wwise/AkSwitch")]
/// @brief This will call AK::SoundEngine::SetSwitch whenever the selected Unity event is triggered.  For example this component could be set on a Unity collider to trigger when an object enters it.
/// \sa 
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=soundengine__switch.html" target="_blank">Integration Details - Switches</a> (Note: This is described in the Wwise SDK documentation.)
public class AkSwitch : AkUnityEventHandler 
{
#if UNITY_EDITOR
	public byte[] groupGuid = new byte[16];
	public byte[] valueGuid = new byte[16];
#endif

	/// Switch Group ID, as defined in WwiseID.cs
    public int groupID;
	/// Switch Value ID, as defined in WwiseID.cs
    public int valueID;
    /// Allowed Mask;
    public int layerMask;

    protected override void Awake()
    {
        base.Awake();

        if (layerMask == 0)
            layerMask = LayerConfigX.Player;
    }

    public override void HandleEvent(GameObject in_gameObject)
	{
#if !DISABLE_AKSOUNDENGINE
        if (in_gameObject != null && ((1 << in_gameObject.layer) & layerMask) != 0)
		    AkSoundEngine.SetSwitch((uint)groupID, (uint)valueID, (useOtherObject && in_gameObject != null) ? in_gameObject : gameObject);
#endif
    }
}

internal static class LayerConfigX
{
    public static readonly int Player = LayerMask.GetMask("Player");
}



#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.