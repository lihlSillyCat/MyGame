using UnityEngine;
using System.Collections;
using XLua;

namespace War.Script
{
    [LuaCallCSharp]
    public class FollowPosition : MonoBehaviour
    {       
        public Transform targetTransform; // Current target transform to constrain to, can be left null for use of provided Vector3
        private void OnDisable()
        {
            targetTransform = null;
        }

        void LateUpdate()
        {
            if (targetTransform != null)
            {
                transform.position = targetTransform.position;
            }
        }
    }
}
