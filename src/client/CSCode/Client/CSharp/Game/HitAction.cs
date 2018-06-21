using UnityEngine;
using System.Collections;
namespace War.Game
{
    public class HitAction : MonoBehaviour
    {
        public CharacterEntity characterEntity;
        public BodyPart bodyPart;

        private void Start()
        {
            characterEntity = GetComponentInParent<CharacterEntity>();
        }

        // 获取部件的中心位置（世界坐标）
        public Vector3 GetCenter()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
                return collider.bounds.center;
            else
                return Vector3.zero;
        }

        public double GetBodyPart()
        {
            return (double)bodyPart;
        }
    }
}
