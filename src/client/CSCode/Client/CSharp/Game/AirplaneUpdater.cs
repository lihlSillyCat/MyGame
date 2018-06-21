using UnityEngine;
using War.Scene;

namespace War.Game
{
    public class AirplaneUpdater : MonoBehaviour
    {
        public Vector3 SpawnPos;
        public Vector3 FlyDir;
        public float SpawnTime;
        public float DisposeTime;
        public float Speed;

        public delegate bool OnFlyEnd();
        public OnFlyEnd onFlyEnd;

        private ObjectToMove objToMove;

        private void Awake()
        {
            objToMove = GetComponent<ObjectToMove>();
        }

        public void StartFly(Vector3 startPos, Vector3 endPos, float speed, float spawnTime, OnFlyEnd callback)
        {
            Vector3 l = endPos - startPos;

            SpawnPos = startPos;
            FlyDir = l.normalized;
            SpawnTime = spawnTime;
            Speed = speed;
            onFlyEnd = callback;
            DisposeTime = spawnTime + l.magnitude / speed;
        }

        private void Update()
        {
            float time = Time.time;
            if (time > DisposeTime)
                onFlyEnd.Invoke();

            objToMove.SetPosition(SpawnPos + FlyDir * (time - SpawnTime) * Speed);
        }
    }
}
