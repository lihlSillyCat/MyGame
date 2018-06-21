namespace UnityEngine
{
    public static class Vector2Extension
    {
        public static Vector2 Rotate(this Vector2 self, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            float tx = self.x;
            float ty = self.y;
            return new Vector2((cos * tx) - (sin * ty), (sin * tx) + (cos * ty));
        }

        public static float Cross(this Vector2 self, Vector2 other)
        {
            return self.x * other.y - self.y * other.x;
        }

        public static bool SameLine(this Vector2 self, Vector2 other)
        {
            return Mathf.Abs(self.Cross(other)) < Vector2.kEpsilon;
        }

        public static bool SameDirection(this Vector2 self, Vector2 other)
        {
            return self.SameLine(other) && Vector2.Dot(self, other) > 0f;
        }

        public static Vector3 GetVector3(this Vector2 self)
        {
            return new Vector3(self.x, 0f, self.y);
        }
    }
}