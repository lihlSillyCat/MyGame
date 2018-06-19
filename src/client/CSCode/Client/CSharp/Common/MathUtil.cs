using UnityEngine;

namespace War.Common
{
    public class MathUtil
    {
        public static Vector3 LocateTransformByChildPoint(Vector3 originPos, Vector3 offset1, Vector3 offset2,
                Vector3 newOffset1, Vector3 newOffset2)
        {
            var fromDirection = offset2 - offset1;
            var v = originPos - offset1;

            var toDirection = newOffset2 - newOffset1;
            var rotate = Quaternion.FromToRotation(fromDirection, toDirection);

            return newOffset1 + rotate * v;
        }

        public static float InverseLerpUnclamped(float a, float b, float value)
        {
            return (value - a) / (b - a);
        }

        /*
         * |(aimPos - origin)| = |(anchorPoint - origin) + k * anchorForward|
         * 求出k，算出 anchorPoint + k * anchorForward
         */
        public static Vector3 CalculateSameLengthPoint(Vector3 origin, Vector3 anchorPoint, Vector3 anchorForward, Vector3 aimPos, out bool isValidity)
        {
            isValidity = true;
            float sqrLen = (aimPos - origin).sqrMagnitude;

            float x0 = anchorPoint.x - origin.x;
            float y0 = anchorPoint.y - origin.y;
            float z0 = anchorPoint.z - origin.z;

            float x1 = anchorForward.x;
            float y1 = anchorForward.y;
            float z1 = anchorForward.z;

            float c = x0 * x0 + y0 * y0 + z0 * z0 - sqrLen;
            float b = 2 * (x0 * x1 + y0 * y1 + z0 * z1);
            float a = x1 * x1 + y1 * y1 + z1 * z1;
            float root = b * b - 4 * a * c;
            if(root < 0)
            {
                isValidity = false;
                return Vector3.zero;
            }
            float delta = Mathf.Sqrt(root);
            float k = (delta - b) * 0.5f / a;
            return anchorPoint + k * anchorForward;
        }
    }
}