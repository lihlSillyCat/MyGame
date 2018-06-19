using UnityEngine;
using System.Collections;

namespace War.Game
{
    public class PhysicsUtility
    {
        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, out RaycastHit hitInfo)
        {
            return Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask);
        }

        public static double RaycastDist(Vector3 origin, Vector3 direction, float maxDistance, int layerMask)
        {
            RaycastHit hitInfo;
            bool succeed = Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask);
            if (succeed)
                return hitInfo.distance;
            else
                return -1.0f;
        }

        public static bool Linecast(Vector3 start, Vector3 end, int layerMask)
        {
            return Physics.Linecast(start, end, layerMask);
        }
    }
}