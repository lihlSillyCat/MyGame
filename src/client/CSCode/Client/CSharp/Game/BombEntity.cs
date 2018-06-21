using UnityEngine;
using System.Collections;

namespace War.Game
{
    public class BombEntity : MonoBehaviour
    {
        public delegate void OnBombCollideEventHandle(Vector3 hitPos);
        public delegate void OnBombTriggerEventHandle(double sid, bool isEnter);

        public OnBombCollideEventHandle onBombCollide = null;
        public OnBombTriggerEventHandle onBombTrigger = null;

        private void OnCollisionEnter(Collision other)
        {
            if (onBombCollide != null)
            {
                onBombCollide(this.transform.position);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (onBombTrigger != null)
            {
                double sid = 0;
                CharacterEntity charEntity = other.GetComponentInParent<CharacterEntity>();
                if (charEntity != null)
                {
                    sid = charEntity.sid;
                }
                else
                {
                    TankEntity tankEntity = other.GetComponentInParent<TankEntity>();
                    if (tankEntity != null)
                        sid = tankEntity.sid;
                }

                if (sid > 0)
                    onBombTrigger(sid, true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (onBombTrigger != null)
            {
                double sid = 0;
                CharacterEntity charEntity = other.GetComponentInParent<CharacterEntity>();
                if (charEntity != null)
                {
                    sid = charEntity.sid;
                }
                else
                {
                    TankEntity tankEntity = other.GetComponentInParent<TankEntity>();
                    if (tankEntity != null)
                        sid = tankEntity.sid;
                }

                if (sid > 0)
                    onBombTrigger(sid, false);
            }
        }
    }
}
