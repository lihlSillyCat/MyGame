using UnityEngine;
using System;
using System.Collections;
using War.Scene;

namespace War.Game
{
    public class MeleeColliderTrigger : MonoBehaviour
    {
        public delegate void OnMeleeHitEventHandle(int sid,Transform hit, Vector3 vec);
        public OnMeleeHitEventHandle onMeleeHit = null;

        public CharacterEntity characterEntity { get; set; }

        public bool bHasCollidered = false;

        public bool bCalCollider = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!bCalCollider)
            {
                return;
            }

            if (characterEntity == null)
            {
                return;
            }

            if (!characterEntity.IsMeleeState())
            {
                return;
            }

            if (characterEntity.IsMeleeState() && bHasCollidered)
            {
                return;
            }

            if (other && other.gameObject)
            {
                Transform trans = this.transform;
                Vector3 vec = other.ClosestPoint(trans.position);
                if (trans)
                {
                    string tag = trans.tag;
                    if (onMeleeHit != null)
                    {
                        bHasCollidered = true;
                        bCalCollider = false;
                        if (other.transform.tag == "Tank")
                        {
                            TankBulletCollider colliderComp = null;
                            Transform transform = other.gameObject.transform;
                            while (transform)
                            {
                                colliderComp = transform.GetComponent<TankBulletCollider>();
                                if (colliderComp != null)
                                {
                                    onMeleeHit((int)colliderComp.sid,other.gameObject.transform, vec);
                                    break;
                                }

                                transform = transform.parent;
                            }
                        }
                        else
                        {
                            onMeleeHit(0,other.gameObject.transform, vec);
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
           
        }
    }
}
