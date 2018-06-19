using UnityEngine;
using System.Collections;
using XLua;
using War.Base;
using War.Game;
using War.Scene;

namespace War.Script
{
    [LuaCallCSharp]
    public class ParaPack : War.Script.BaseObject
    {
        public bool isClientVerify;
        public float speed;
        public float height;
        public float duration;
        public float startTime;
        public Vector3 serverPos;
        public Vector3 currentPos;
        public ObjectToMove entity;

        public ParaPack()
        {

        }

        public void setPosition(float x, float y, float z)
        {
            currentPos = new Vector3(x, y, z);
            if (entity != null)
            {
                entity.SetPosition(currentPos);
            }
        }

        public void VerifyLandedPos()
        {
            if(isClientVerify)
            {
                return;
            }
            Vector3 tilePos = StreamerManager.GetTilePosition(currentPos);
            RaycastHit hit;
            if(PhysicsUtility.Raycast(tilePos, Vector3.down, height, LayerConfig.ParachuteMask, out hit))
            {
                isClientVerify = true;
                Vector3 hitRealpos = StreamerManager.GetRealPosition(hit.point);
                height = serverPos.y - hitRealpos.y;
            }

        }

        public bool TickFall()
        {
            float deltaHeight = (Time.realtimeSinceStartup - startTime + duration) * speed;
            VerifyLandedPos();
            if(deltaHeight > height)
            {
                return false;
            }
            currentPos = serverPos - new Vector3(0.0f, deltaHeight, 0.0f);
            if (entity != null)
            {
                entity.SetPosition(currentPos);
            }
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
