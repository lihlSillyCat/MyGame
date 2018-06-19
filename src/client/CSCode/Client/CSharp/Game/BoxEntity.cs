using UnityEngine;
using System.Collections;

namespace War.Game
{
    /// <summary>
    /// 可拾取组件
    /// </summary>
    public class BoxEntity : MonoBehaviour
    {
        public int boxSid = 0;
        public delegate void OnColliderHandler(int doorID, int playrSid);
        public OnColliderHandler OnPlayerEnter;
        public OnColliderHandler OnPlayerLeave;

        private void OnEnable()
        {
            boxSid = 0;
        }

        private void OnDisable()
        {
            OnPlayerEnter = null;
            OnPlayerLeave = null;
            OnCollisionPlayer = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerConfig.Player)
            {
                CharacterEntity entity = other.gameObject.GetComponent<CharacterEntity>();
                if (OnPlayerEnter != null)
                    OnPlayerEnter(boxSid, entity.sid);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerConfig.Player)
            {
                CharacterEntity entity = other.gameObject.GetComponent<CharacterEntity>();
                if (OnPlayerLeave != null)
                    OnPlayerLeave(boxSid, entity.sid);
            }
        }

        public delegate void OnCollisionHandler(int boxsid, int playerSid);
        public OnCollisionHandler OnCollisionPlayer;
        public OnCollisionHandler OnCollisionTank;
        public void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer == LayerConfig.Player)
            {
                CharacterEntity entity = other.gameObject.GetComponent<CharacterEntity>();
                if (OnCollisionPlayer != null)
                    OnCollisionPlayer(boxSid, entity.sid);
            }
            if(other.gameObject.layer == LayerConfig.Tank)
            {
                TankEntity tankEntity = other.gameObject.GetComponent<TankEntity>();
                if (OnCollisionTank != null)
                    OnCollisionTank(boxSid, tankEntity.sid);
            }
        }
    }
}
