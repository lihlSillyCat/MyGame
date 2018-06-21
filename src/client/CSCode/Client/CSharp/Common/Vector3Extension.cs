namespace UnityEngine
{
    public static class Vector3Extension
    {
        public static Vector3 GetVectorXZ(this Vector3 self)
        {
            return new Vector3(self.x, 0f, self.z);
        }
    }
}