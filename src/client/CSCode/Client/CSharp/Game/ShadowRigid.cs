using UnityEngine;

namespace War.Game
{
    [RequireComponent(typeof(Scene.ObjectToMove))]
    public class ShadowRigid : MonoBehaviour
    {
        protected Rigidbody m_Rigidbody;
        protected CapsuleCollider m_CapsuleCollider;
        protected Transform m_CharacterTransform;
        protected PostureState m_PostureState;

        [SerializeField]
        protected float m_LerpMultipier = 1f;

        [SerializeField]
        private PhysicMaterial m_FrictionMaterial = null;

        [SerializeField]
        private PhysicMaterial m_MaxFrictionMaterial = null;

        protected Vector3 m_Velocity;

        protected Scene.ObjectToMove m_ObjectToMove;

        public bool isKinematic
        {
            set
            {
                m_Rigidbody.isKinematic = value;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            m_ObjectToMove = GetComponent<Scene.ObjectToMove>();

            m_Rigidbody = GetComponent<Rigidbody>();
            m_CapsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void OnEnable()
        {
            m_Rigidbody.isKinematic = false;
            m_CharacterTransform = null;
            m_Velocity = Vector3.zero;
            SetVelocity(m_Velocity.x, m_Velocity.y, m_Velocity.z);
        }

        private void OnDisable()
        {
            m_CharacterTransform = null;
        }

        public void SetCharacterTransform(Transform trans)
        {
            if (trans != null)
            {
                var charEntity = trans.GetComponent<CharacterEntity>();
                charEntity.onPhysicsPropChange = OnParentPhysicsPropChange;
            }
            else if (m_CharacterTransform != null)
            {
                var charEntity = m_CharacterTransform.GetComponent<CharacterEntity>();
                charEntity.onPhysicsPropChange = null;
            }
            m_CharacterTransform = trans;
        }

        public void SetPosition(float x, float y, float z)
        {
            m_ObjectToMove.SetPosition(x, y, z);
        }

        public void SetVelocity(float x, float y, float z)
        {
            m_Velocity = new Vector3(x, y, z);
            if (m_PostureState == PostureState.Swim)
            {
                m_Velocity = m_Velocity.GetVectorXZ();
            }
            m_Rigidbody.velocity = m_Velocity;

            if (m_Velocity.sqrMagnitude < Vector2.kEpsilon)
            {
                m_CapsuleCollider.material = m_MaxFrictionMaterial;
            }
            else
            {
                m_CapsuleCollider.material = m_FrictionMaterial;
            }
        }

        private void FixedUpdate()
        {
            m_Velocity.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = m_Velocity;
        }

        private void Update()
        {
            if (m_CharacterTransform != null)
            {
                var charPos = m_CharacterTransform.position;
                charPos = Vector3.Lerp(charPos, transform.position, Time.deltaTime * m_LerpMultipier);
                m_CharacterTransform.position = charPos;
            }
        }

        private void OnParentPhysicsPropChange(Rigidbody rigidBody, CapsuleCollider collider, PostureState postureState)
        {
            if (m_Rigidbody == null)
            {
                return;
            }

            m_Rigidbody.isKinematic = rigidBody.isKinematic;
            m_Rigidbody.useGravity = rigidBody.useGravity;
            m_Rigidbody.constraints = rigidBody.constraints;

            m_CapsuleCollider.center = collider.center;
            m_CapsuleCollider.height = collider.height;
            m_CapsuleCollider.direction = collider.direction;
            m_CapsuleCollider.radius = collider.radius;

            m_PostureState = postureState;
            if (m_PostureState == PostureState.Swim)
            {
                m_Rigidbody.velocity = m_Velocity.GetVectorXZ();
            }
        }
    }
}
