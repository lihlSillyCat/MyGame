using System;
using UnityEngine;
using XLua;

namespace War.Script
{
    [LuaCallCSharp]
    public class AkSoundEngineLua
    {

        public static void AddBasePath(string path)
        {
#if !DISABLE_AKSOUNDENGINE
            AkSoundEngine.AddBasePath(path);
#endif
        }

        public static void LoadSoundBank(string bankName)
        {
#if !DISABLE_AKSOUNDENGINE
            uint bankId;
            AkSoundEngine.LoadBank(bankName, AkSoundEngine.AK_DEFAULT_POOL_ID, out bankId);
#endif
        }

        public static void LoadSoundBankAsync(string bankName)
        {
#if !DISABLE_AKSOUNDENGINE
            AkBankManager.LoadBankAsync(bankName);
#endif
        }

        public static void UnLoadSoundBank(String bankName)
        {
#if !DISABLE_AKSOUNDENGINE
            AkBankManager.UnloadBank(bankName);
#endif
        }

        public static void PostEvent(double eventID, GameObject gameObj)
        {
#if !DISABLE_AKSOUNDENGINE
            uint playingId = AkSoundEngine.PostEvent((uint)eventID, gameObj);

            if (playingId == AkSoundEngine.AK_INVALID_PLAYING_ID)
            {
                Debug.LogError("Could not post event ID \"" + eventID + "\". Did you make sure to load the appropriate SoundBank?");
            }
#endif
        }

        public static void PostEvent(string eventName, GameObject gameObj)
        {
#if !DISABLE_AKSOUNDENGINE
            uint playingId = AkSoundEngine.PostEvent(eventName, gameObj);

            if (playingId == AkSoundEngine.AK_INVALID_PLAYING_ID)
            {
                Debug.LogError("Could not post event name \"" + eventName + "\". Did you make sure to load the appropriate SoundBank?");
            }
#endif
        }

        public static void SetSwitch(double groupID, double switchID, GameObject gameObj)
        {
#if !DISABLE_AKSOUNDENGINE
            AkSoundEngine.SetSwitch((uint)groupID, (uint)switchID, gameObj);
#endif
        }

        public static void SetSwitch(string groupName, string switchName, GameObject gameObj)
        {
#if !DISABLE_AKSOUNDENGINE
            AkSoundEngine.SetSwitch(groupName, switchName, gameObj);
#endif
        }
        public static void SetState(double groupID, double stateID)
        {
#if !DISABLE_AKSOUNDENGINE
            AkSoundEngine.SetState((uint)groupID, (uint)stateID);
#endif
        }

        public static void SetState(string groupName, string stateName)
        {
#if !DISABLE_AKSOUNDENGINE
            AkSoundEngine.SetState(groupName, stateName);
#endif
        }

        public static void SetRTPCValue(string name, double value)
        {
#if !DISABLE_AKSOUNDENGINE
            AkSoundEngine.SetRTPCValue(name, (float)value);
#endif
        }

        public static void SetRTPCValue(string name, double value, GameObject gameObj)
        {
#if !DISABLE_AKSOUNDENGINE
            AkSoundEngine.SetRTPCValue(name, (float)value, gameObj);
#endif
        }

        public static void SetRTPCValue (string name, double value, GameObject gameObj, double duration)
        {
#if !DISABLE_AKSOUNDENGINE
            AkSoundEngine.SetRTPCValue(name, (float)value, gameObj, (int)duration);
#endif
        }

        public static void SetMaxNumVoicesLimit(double maxNumVoices)
        {
#if !DISABLE_AKSOUNDENGINE
            AkSoundEngine.SetMaxNumVoicesLimit((ushort)maxNumVoices);
#endif
        }
    }
}
