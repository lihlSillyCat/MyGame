using UnityEngine;

namespace War.Game
{
    [RequireComponent(typeof(Renderer))]
    public class CharacterPart : MonoBehaviour
    {
        [SerializeField]
        protected CharacterEntity m_CharacterEntity;

        [SerializeField]
        protected BodyPart m_BodyPart;

        protected Renderer m_Renderer;

        private void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
            if (m_Renderer.isVisible)
            {
                OnBecameVisible();
            }
            else
            {
                OnBecameInvisible();
            }
        }

        public void OnBecameVisible()
        {
            m_CharacterEntity.OnBodyPartVisible(m_BodyPart);
        }

        public void OnBecameInvisible()
        {
            m_CharacterEntity.OnBodyPartInvisible(m_BodyPart);
        }
    }
}