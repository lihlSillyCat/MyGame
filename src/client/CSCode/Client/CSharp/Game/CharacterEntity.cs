using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

using RootMotion.FinalIK;

using War.Game.Ani;


namespace War.Game
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Scene.ObjectToMove))]
    [RequireComponent(typeof(AvatarSystem))]
    public class CharacterEntity : MonoBehaviour
    {
        [System.Serializable]
        public struct CapsuleState
        {
            public Vector3 center;
            public float height;
            public float radius;
            [Tooltip("0 1 2 分别对应x y z轴")]
            public int direction; // 0 1 2 分别对应x y z轴
        }
        private const float LookForwardDistance = 50f;
        private const float TurnRoundAngleLimit = 90f;
        private const float WholeBodyAttackSpeedLimit = 0.1f;

        #region specialJump需要使用到的参数
        public delegate void OnBeginSpecialJump();
        public OnBeginSpecialJump onBeginSpecialJump;
        public delegate void OnEndSpecialJump();
        public OnEndSpecialJump onEndSpecialJump;
        public float downOverJumpThreshold = 0.5f;  //向下大于多少高度可以进行翻越
        public Vector3 posForJump = Vector3.zero; //跳跃时检测到的需要跳上去的位置
        public float cameraTargetYOffset = 0;
        public float degreeLimit = 3.0f;
        public int jumpType = 0;
        public bool isInSpecialJumpState
        {
            get
            {
                return m_IsInSpecialJumpState;
            }
            set
            {
                m_IsInSpecialJumpState = value;
                if(m_IsInSpecialJumpState == false)
                {
                    m_FreeLook = m_TempFreeLook;
                    if(m_FreeLook == false)
                    {
                        CheckIfNeedTurn();
                    }
                }
            }
        }
        private bool m_TempFreeLook = true;
        private bool m_IsInSpecialJumpState = false;
        #endregion

        #region Animator layer define
        private const int FullBodyLayer = 0;
        private const int ReloadLayer = 1;
        private const int LobbyLayer = 2;
        private const int SwimLayer = 3;
        private const int LeftHandLayer = 4;
        private const int RightHandLayer = 5;
        private const int UpperBodyLayer = 6;
        #endregion

        private const float InvalidWeaponSlot = -1f;

        [Tooltip("animatorStandStateName应该与动画状态机中，站立状态时动画State的名字一直")]
        [SerializeField]
        protected string[] m_StandStateNames;

        [SerializeField]
        protected float m_JumpPower = 12f;

        [SerializeField]
        protected float m_GroundCheckDistance = 0.1f;

        [SerializeField]
        protected float m_MovingTurnSpeed = 360f;

        [SerializeField]
        protected float m_StationaryTurnSpeed = 180f;

        [SerializeField]
        protected Transform m_HeadTransform;

        protected Transform m_LeftHandTransform;

        protected Transform m_RightHandTransform;

        [SerializeField]
        protected Transform m_RightHandItemTransform;
        public Transform rightHandItemTransform
        {
            get
            {
                return m_RightHandItemTransform;
            }
            set
            {
                m_RightHandItemTransform = value;
            }
        }

        [SerializeField]
        protected Transform m_LeftHandItemTransform;
        public Transform leftHandItemTransform
        {
            get
            {
                return m_LeftHandItemTransform;
            }
            set
            {
                m_LeftHandItemTransform = value;
            }
        }

        protected Animator m_Animator;
        [SerializeField]
        protected Rigidbody m_Rigidbody;

        [SerializeField]
        protected float m_HandWeightMultipier = 2f;
        [SerializeField]
        private UpperBodyController m_UpperBodyController = null;
        public bool upperBodyToggle
        {
            get
            {
                return m_UpperBodyToggle;
            }
            set
            {
                m_UpperBodyToggle = value;
                RefreshUpperBodyState();
            }
        }
        protected bool m_UpperBodyToggle;

        public int upperBodyIkEnter;
        public float upperBodyLeftHandPosWeight;
        public float upperBodyLeftHandRotWeight;
        public float upperBodyRightHandPosWeight;
        public float upperBodyRightHandRotWeight;
        private bool m_IsRunState;
        private float m_LeftHandIKWeightInRunState;
        private float m_RightHandIKWeightInRunState;

        private float m_LeftHandPositionWeight;
        public float leftHandPositionWeight
        {
            set
            {
                m_LeftHandPositionWeight = value;
            }
            get
            {
                return m_LeftHandPositionWeight;
            }
        }

        private float m_LeftHandRotationWeight;
        public float leftHandRotationWeight
        {
            set
            {
                m_LeftHandRotationWeight = value;
            }
            get
            {
                return m_LeftHandRotationWeight;
            }
        }

        private float m_RightHandPositionWeight;
        public float rightHandPositionWeight
        {
            set
            {
                m_RightHandPositionWeight = value;
            }
            get
            {
                return m_RightHandPositionWeight;
            }
        }

        private float m_RightHandRotationWeight;
        public float rightHandRotationWeight
        {
            set
            {
                m_RightHandRotationWeight = value;
            }
            get
            {
                return m_RightHandRotationWeight;
            }
        }

        #region optimization member
        public bool isKinematic
        {
            get
            {
                return m_Rigidbody.isKinematic;
            }
        }

        public bool isVisible
        {
            get
            {
                return m_IsRenderVisible != 0 || m_CameraTrans != null;
            }
        }

        [SerializeField]
        protected float m_IKUpdateEnableThreshold = 200f;
        #endregion

        protected bool m_IsGrounded;
        public bool isOnGround
        {
            get { return m_IsGrounded; }

        }

        protected float m_OrigGroundCheckDistance;

        protected float m_ForwardAmount;
        protected float m_Sideslip;

        protected Vector3 m_Velocity;

        protected Coroutine m_TurningCoroutine;

        protected WeaponGun m_WeaponGun;
        protected bool m_HasBomb;

        [Tooltip("开始跑楼梯的高度")]
        [SerializeField]
        protected float m_StepStart = 0.1f;
        [Tooltip("允许爬楼梯的最高高度")]
        [SerializeField]
        protected float m_StepEnd = 0.5f;

        [SerializeField]
        protected float m_MinStepSpeed = 3.5f;

        [Tooltip("爬坡速度减速角度限制")]
        [SerializeField]
        protected float m_StepSlopLimit = 70f;

        [SerializeField]
        protected float m_SlopeLimit = 35.0f; //趴下坡度限制

        private RaycastHit m_GroundHit;
        private float m_GroundDistance;
        private CapsuleCollider m_CapsuleCollider;
        [SerializeField]
        protected CapsuleState m_CapsuleStandState;
        [SerializeField]
        protected CapsuleState m_CapsuleCrouchState;
        [SerializeField]
        protected CapsuleState m_CapsuleProneState;

        [SerializeField]
        private PhysicMaterial m_FrictionMaterial = null;

        [SerializeField]
        private PhysicMaterial m_MaxFrictionMaterial = null;

        private Vector2 m_MoveDir;
        private float m_Speed;

        [SerializeField]
        protected bool m_IsAllowLookIK;
        public bool isAllowLookIK
        {
            get { return m_IsAllowLookIK; }
            set
            {
                m_IsAllowLookIK = value;
            }
        }

        [SerializeField]
        protected bool m_IsAllowFBBK;
        public bool isAllowFBBK
        {
            get { return m_IsAllowFBBK; }
            set
            {
                m_IsAllowFBBK = value;
            }
        }

        [System.Serializable]
        public class AimSetting
        {
            [SerializeField]
            public Transform aimTransform;
            public float aimWeight;
            public float aimVerticalWeight;

            [HideInInspector]
            public Vector3 localOffset;
            public Vector3 offset;
            public float keepPosWeight;
        }

        #region AimIK
        [SerializeField]
        protected Transform m_IKHandGun;

        [SerializeField]
        protected Transform m_IKHandGunLeft;

        [SerializeField]
        protected Transform m_IKHandGunRight;

        [SerializeField]
        protected float m_IKHandGunWeight = 0.8f;

        [SerializeField]
        protected Vector3[] m_IKHandGunAnchorOffset;

        [SerializeField]
        protected Vector3[] m_IKHandGunRootOffset;

        [SerializeField]
        protected float m_AimWeightMultipier = 10f;
        protected float m_AimWeight;
        protected float m_AimWeightTarget;

        [SerializeField]
        protected AimSetting[] m_AimSettings;

        [SerializeField]
        protected bool m_IsAllowAimIK;
        public bool isAllowAimIK
        {
            get { return m_IsAllowAimIK; }
            set
            {
                m_IsAllowAimIK = value;
                if (value)
                {
                    m_AimWeightTarget = 1f;
                }
                else
                {
                    m_AimWeightTarget = 0f;
                }
            }
        }
        #endregion

        #region ADS
        protected Transform m_CameraTrans;

        [SerializeField]
        protected Transform m_IKAimRoot;

        [SerializeField]
        protected Transform m_IKADSLeft;

        [SerializeField]
        protected Transform m_IKADSRight;

        [SerializeField]
        protected Transform m_CameraFppTransform;

        [SerializeField]
        protected Transform m_CameraAnchorTransform;
        public Transform cameraAnchorTransform
        {
            get
            {
                return m_CameraAnchorTransform;
            }
        }

        [SerializeField]
        protected AimSetting[] m_ADSSettings;

        [SerializeField]
        private float m_ADSAnchorMultiplier = 20f;

        [Tooltip("屏息")]
        [SerializeField]
        protected bool m_IsHoldOnBreath;
        public bool isHoldOnBreath
        {
            get { return m_IsHoldOnBreath; }
            set { m_IsHoldOnBreath = value; }
        }

        [SerializeField]
        protected float m_HoldOnBreathMultipier = 0.3f;

        [SerializeField]
        protected bool m_IsAllowADS;
        public bool isAllowADS
        {
            get { return m_IsAllowADS; }
            set
            {
                m_IsAllowADS = value;
                if (value)
                {
                    m_AnchorMirrorPos = (m_CameraFppTransform.position * 2 - m_CameraAnchorTransform.position)
                                        - m_IKAimRoot.position;
                }
            }
        }

        protected Vector3 m_AnchorMirrorPos;
        #endregion

        protected bool m_FreeLook = true;
        protected Vector3 m_LookForward;
        protected Vector3 m_LookForwardTarget;

        [System.Serializable]
        public class LookSetting
        {
            public Transform bone;

            [Range(0f, 1f)]
            public float weight;
        }

        [SerializeField]
        protected float m_LookWeight = 1.0f;

        [SerializeField]
        protected LookSetting[] m_LookSettings;

        [SerializeField]
        protected float m_LookForwardMultipier = 10;

        protected GunRecoil m_GunRecoil;
        protected FullBodyBipedIK m_FullBodyBipedIK;

        protected Scene.ObjectToMove m_ObjectToMove;

        protected PostureState m_PostureState;
        public PostureState postureState
        {
            get { return m_PostureState; }
        }

        protected AimingState m_AimingState;

        public enum BodyVisible
        {
            Head = 0,
            Body = 1,
        }
        protected int m_IsRenderVisible;

        [System.Serializable]
        public class RenderVisibleEvent : UnityEvent<bool>
        {
        }

        [SerializeField]
        protected RenderVisibleEvent m_RenderVisibleEvent = null;

        [SerializeField]
        protected List<Transform> m_SlotTransforms;

        public Vector3 position
        {
            get
            {
                return m_ObjectToMove.GetPosition();
            }
        }

        public Vector3 velocity
        {
            get
            {
                return m_Rigidbody.velocity;
            }
        }

        public float forward
        {
            get
            {
                return transform.eulerAngles.y;
            }
        }

        [Tooltip("游泳水深")]
        [SerializeField]
        private float m_SwimDepth = 1.5f;
        [Tooltip("游泳切换到站立的偏移")]
        [SerializeField]
        private float m_SwinToStandDepth = 1.35f;
        [Tooltip("游泳要求从趴下切换到蹲高度")]
        [SerializeField]
        private float m_InWaterCrouchDepth = 0.3f;
        [Tooltip("游泳要求切换到站立高度")]
        [SerializeField]
        private float m_InWaterStandDepth = 0.8f;
        [Tooltip("因为浮点问题，水中姿势切换给个框量")]
        [SerializeField]
        private float m_InWaterPostureBias = 0.05f;

        private float m_SwimLayerWeight = 0;

        // 角色入水深度
        private float m_WaterDepth = 0f;
        // 水位高度
        private float m_WaterLevel = 0f;

        //水花特效：
        private GameObject floatEffect = null;
        private GameObject swimEffect = null;
        private GameObject hitWaterEffectLeft = null;
        private GameObject hitWaterEffectRight = null;

        private int elapseCountLeft = 0;
        private int elapseCountRight = 0;

        // 角色逻辑sid
        protected int m_Sid;
        public int sid
        {
            get
            {
                return m_Sid;
            }
            set
            {
                m_Sid = value;
            }
        }

        protected IKEffector m_LeftHandEffector;
        protected IKEffector m_RightHandEffector;

        #region event handlers
        public delegate void OnTakeWeaponEventHandler();

        public delegate void OnPutWeaponEventHandler();
        public delegate void OnThrowBombEventHandler();

        public OnTakeWeaponEventHandler onTakeWeapon;
        public OnTakeWeaponEventHandler onTakeWeaponReady;
        public OnPutWeaponEventHandler onPutWeapon;
        public OnThrowBombEventHandler onThrowBomb;

        public delegate void OnJumpToProneEventHandler();
        public OnJumpToProneEventHandler onJumpToProne;

        public delegate void OnMeleeHitEventHandler(int sid, BodyPart bodyPart, Vector3 hitPos, HitActionType hitType);
        protected OnMeleeHitEventHandler m_OnMeleeHit;

        public delegate void OnMeleeHitExitStateEventHandler();
        public OnMeleeHitExitStateEventHandler onMeleeExit;

        public delegate void OnMeleeHitAictionStateEventHandler();
        public OnMeleeHitAictionStateEventHandler OnMeleeAction;

        public delegate void OnEnterRigidityEventHandler();
        public OnEnterRigidityEventHandler onEnterRigidity;
        public delegate void OnLeaveRigidityEventHandler();
        public OnLeaveRigidityEventHandler onLeaveRigidity;

        public delegate void OnEnterFullRigidityEventHandler();
        public OnEnterFullRigidityEventHandler onEnterFullRigidity;
        public delegate void OnLeaveFullRigidityEventHandler();
        public OnLeaveFullRigidityEventHandler onLeaveFullRigidity;

        public delegate void OnLeftHandIKEventHandler(bool toggle);
        public OnLeftHandIKEventHandler onLeftHandIK;
        public delegate void OnRightHandIKEventHandler(bool toggle);
        public OnRightHandIKEventHandler onRightHandIK;

        public delegate void OnStartReloadEventHandler();
        public OnStartReloadEventHandler onStartReload;
        public delegate void OnFinishReloadEventHandler();
        public OnFinishReloadEventHandler onFinishReload;
        public delegate void OnExitReloadEventHandler();
        public OnExitReloadEventHandler onExitReload;

        public delegate void OnStandStateEnterEventHandler();
        public OnStandStateEnterEventHandler onStandStateEnter;
        public delegate void OnCrouchStateEnterEventHandler();
        public OnCrouchStateEnterEventHandler onCrouchStateEnter;
        public delegate void OnProneStateEnterEventHandler();
        public OnProneStateEnterEventHandler onProneStateEnter;
        public delegate void OnWoundStateEnterEventHandler();
        public OnWoundStateEnterEventHandler onWoundStateEnter;
        public delegate void OnGunChamberingStateExitEventHandler();
        public OnGunChamberingStateExitEventHandler onChamberingStateExit;

        // 游泳姿势切换请求
        public delegate void OnWaterEventHandler(bool swim);
        public OnWaterEventHandler onWaterEvent;

        public delegate void OnHitGroundEventHandler(float verticalVelocity, float horizontalVelocity);
        public OnHitGroundEventHandler onHitGround;

        public delegate void OnPhysicsPropChange(Rigidbody rigidBody, CapsuleCollider collider, PostureState postureState);
        public OnPhysicsPropChange onPhysicsPropChange;
        #endregion

        [SerializeField]
        private HandPoser m_LeftHandPoser = null;
        [SerializeField]
        private HandPoser m_RightHandPoser = null;

        // 近战伤害标记
        private bool m_bCalDamage = false;

        //动作控制器
        private AniCtrl m_AniCtrl;
        //换枪动作与槽位的对应关系
        private AnimationToSlot m_AnimationToSlot;

        public Collider headCollider;
        public Collider waistCollider;

        public bool leftHandIKToggle = false;
        public bool rightHandIKToggle = false;

        [HideInInspector]
        public float reloadTime = 1.0f;

        protected bool m_PresentationMode;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

            m_ObjectToMove = GetComponent<Scene.ObjectToMove>();

            m_CapsuleCollider = GetComponent<CapsuleCollider>();
            if (m_CapsuleCollider == null)
            {
                Debug.LogError("m_CapsuleCollider is null");
            }

            m_Animator = GetComponent<Animator>();

            if (m_Rigidbody == null)
            {
                m_Rigidbody = GetComponent<Rigidbody>();
            }

            m_AnimationToSlot = GetComponent<AnimationToSlot>();

            m_GunRecoil = GetComponent<GunRecoil>();

            m_FullBodyBipedIK = GetComponent<FullBodyBipedIK>();

            var solver = m_FullBodyBipedIK.solver;

            m_LeftHandTransform = solver.leftHandEffector.bone;
            m_RightHandTransform = solver.rightHandEffector.bone;

            m_LeftHandEffector = solver.leftHandEffector;
            m_RightHandEffector = solver.rightHandEffector;

            m_OrigGroundCheckDistance = m_GroundCheckDistance;
        }

        private void OnEnable()
        {
            m_Animator.Rebind();
            m_Animator.applyRootMotion = false;
            SetUpperBodyOn();
            m_Animator.SetLayerWeight(LobbyLayer, 0f);
            m_Animator.SetLayerWeight(SwimLayer, 0f);
            m_Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            m_Animator.SetInteger("WeaponType", (int)WeaponType.Unarmed);

            m_AimWeight = 0f;
            m_AimWeightTarget = 0f;

            m_UpperBodyToggle = true;
            // update IK manual

            m_FullBodyBipedIK.enabled = false;

            m_LeftHandPoser.enabled = false;
            m_RightHandPoser.enabled = false;

            // 初始化关闭IK
            m_LeftHandPositionWeight = 0f;
            m_LeftHandRotationWeight = 0f;
            m_RightHandPositionWeight = 0f;
            m_RightHandRotationWeight = 0f;

            m_LeftHandEffector.positionWeight = 0f;
            m_LeftHandEffector.rotationWeight = 0f;
            m_RightHandEffector.positionWeight = 0f;
            m_RightHandEffector.rotationWeight = 0f;

            upperBodyIkEnter = 0;
            upperBodyLeftHandPosWeight = 0f;
            upperBodyLeftHandRotWeight = 0f;
            upperBodyRightHandPosWeight = 0f;
            upperBodyRightHandRotWeight = 0f;

            m_IsRunState = false;
            m_LeftHandIKWeightInRunState = 0f;
            m_RightHandIKWeightInRunState = 0f;

            m_PostureState = PostureState.Stand;

            m_CapsuleCollider.isTrigger = false;
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.useGravity = true;
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX
                                        | RigidbodyConstraints.FreezeRotationY
                                        | RigidbodyConstraints.FreezeRotationZ;

            m_Velocity.Set(0f, 0f, 0f);
            m_Rigidbody.velocity = m_Velocity;

            m_AniCtrl = new AniCtrl();
            m_AniCtrl.Create(m_Animator);

            m_IsAllowLookIK = true;
            m_IsAllowAimIK = true;
            m_IsAllowFBBK = true;
            m_IsAllowADS = false;

            m_IsHoldOnBreath = false;
            m_WaterDepth = 0f;
            m_WaterLevel = float.MinValue;

            m_PresentationMode = false;
        }

        private void OnDisable()
        {
            ClearSwimEffects();

            onWaterEvent = null;
            onTakeWeaponReady = null;
            onPutWeapon = null;
            onEnterRigidity = null;
            onLeaveRigidity = null;
            onEnterFullRigidity = null;
            onLeaveFullRigidity = null;
            onLeftHandIK = null;
            onRightHandIK = null;
            onStartReload = null;
            onFinishReload = null;
            onExitReload = null;
            onStandStateEnter = null;
            onCrouchStateEnter = null;
            onProneStateEnter = null;
            onWoundStateEnter = null;
            onChamberingStateExit = null;
            onPhysicsPropChange = null;
            onTakeWeapon = null;
            onTakeWeaponReady = null;
            onMeleeExit = null;
            onHitGround = null;
            m_OnMeleeHit = null;
            onBeginSpecialJump = null;
            onEndSpecialJump = null;

            m_IsAllowADS = false;
            m_WeaponGun = null;
            m_LeftHandPoser.poseRoot = null;
            m_RightHandPoser.poseRoot = null;
            m_HasBomb = false;

            m_CameraTrans = null;

            m_AniCtrl.Release();
            m_AniCtrl = null;
        }

        public void ReconstructPosture(int posID)
        {
            m_Animator.SetInteger("PosID", posID);
            m_Animator.SetTrigger("CreateEntity");
        }

        public void SetRunState(bool isRunState)
        {
            m_IsRunState = isRunState;
        }

        public void SetRunStateHandIKWeight(float leftHandWeight, float rightHandWeight)
        {
            m_LeftHandIKWeightInRunState = leftHandWeight;
            m_RightHandIKWeightInRunState = rightHandWeight;
        }

        public void SetGunType(float gunType)
        {
            m_Animator.SetFloat("GunType", gunType);
        }

        public void OnBodyPartVisible(BodyPart bodyPart)
        {
            var preVisible = m_IsRenderVisible;
            m_IsRenderVisible |= 1 << (int)bodyPart;
            if (preVisible != m_IsRenderVisible && m_RenderVisibleEvent != null)
            {
                m_RenderVisibleEvent.Invoke(isVisible);
            }

            if (UpdateIsKinematic())
            {
                PhysicsPropChange();
            }
        }

        public void OnBodyPartInvisible(BodyPart bodyPart)
        {
            var preVisible = m_IsRenderVisible;
            m_IsRenderVisible -= m_IsRenderVisible & (1 << (int)bodyPart);
            if (preVisible != m_IsRenderVisible && m_RenderVisibleEvent != null)
            {
                m_RenderVisibleEvent.Invoke(isVisible);
            }

            if (UpdateIsKinematic())
            {
                PhysicsPropChange();
            }
        }

        private bool UpdateIsKinematic()
        {
            bool isKinematic = !isVisible || IsOnVehicle() || m_PresentationMode;
            if (m_Rigidbody.isKinematic != isKinematic)
            {
                m_Rigidbody.isKinematic = isKinematic;
                return true;
            }

            return false;
        }

        private void PhysicsPropChange()
        {
            if (onPhysicsPropChange != null)
            {
                onPhysicsPropChange.Invoke(m_Rigidbody, m_CapsuleCollider, m_PostureState);
            }
        }

        public void SetPosition(float x, float y, float z)
        {
            m_ObjectToMove.SetPosition(x, y, z);
        }

        public void SetVelocity(float x, float y, float z)
        {
            m_Velocity.Set(x, y, z);
            m_Rigidbody.velocity = m_Velocity;
        }

        public void SetForward(float angle)
        {
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        }
        
        public void SetLookForward(bool freeLook, float x, float y, float z)
        {
            m_FreeLook = freeLook;
            m_TempFreeLook = freeLook;
            if (isInSpecialJumpState)
            {
                m_FreeLook = true;
            }
            var lookForward = new Vector3(x, y, z);
            m_LookForwardTarget = lookForward * LookForwardDistance;

            if (!m_FreeLook)
            {
                CheckIfNeedTurn();
            }
        }

        private void CheckIfNeedTurn()
        {
            var turnDirection = transform.InverseTransformDirection(m_LookForwardTarget);
            var deltaAngle = Mathf.Atan2(turnDirection.x, turnDirection.z) * Mathf.Rad2Deg;

            // 看的方向大于90度则旋转
            if (Mathf.Abs(deltaAngle) > TurnRoundAngleLimit && !IsOnVehicle())
            {
                //transform.Rotate(0f, deltaAngle, 0f);
                if (m_TurningCoroutine != null)
                {
                    StopCoroutine(m_TurningCoroutine);
                }
                m_TurningCoroutine = StartCoroutine(TurnTo(deltaAngle));
            }
        }

        private bool IsOnVehicle()
        {
            return m_PostureState >= PostureState.Drive && m_PostureState <= PostureState.Parachute;
        }

        public void Move(float x, float z)
        {
            m_Velocity.x = x;
            m_Velocity.y = m_Rigidbody.velocity.y;
            m_Velocity.z = z;
            m_Rigidbody.velocity = m_Velocity;

            m_MoveDir.x = x;
            m_MoveDir.y = z;
            m_Speed = m_MoveDir.magnitude;

            m_MoveDir.Normalize();
        }

        IEnumerator TurnTo(float turnAngle)
        {
            float turnSign = Mathf.Sign(turnAngle);

            float turnSpeed = turnSign * m_StationaryTurnSpeed;
            float turnTime = turnAngle / turnSpeed;

            for (; turnTime > 0f;)
            {
                float deltaTime = Time.deltaTime;
                turnTime -= deltaTime;
                if (turnTime < 0f)
                {
                    deltaTime += turnTime;
                }
                //m_Animator.SetFloat("Turn", turnSign);
                transform.Rotate(0f, turnSpeed * deltaTime, 0f);
                yield return null;
            }

            //m_Animator.SetFloat("Turn", 0f);
            m_TurningCoroutine = null;
        }

        public Vector3 GetHeadPosition()
        {
            return m_ObjectToMove.GetPosition() + transform.up * m_CapsuleCollider.height;
        }

        public void SetPostureState(int postureState)
        {
            PostureState oldPostureState = m_PostureState;
            m_PostureState = (PostureState)postureState;
            if (m_PostureState <= PostureState.Prone)
            {
                m_Animator.SetInteger("Posture", postureState);
            }

            Vector3 eulerAngles = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, eulerAngles.y, 0);
            // 设置人物碰撞体不同姿势下数据
            if (m_PostureState == PostureState.Stand)
            {
                m_CapsuleCollider.center = m_CapsuleStandState.center;
                m_CapsuleCollider.height = m_CapsuleStandState.height;
                m_CapsuleCollider.direction = m_CapsuleStandState.direction;
                m_CapsuleCollider.radius = m_CapsuleStandState.radius;
            }
            else if (m_PostureState == PostureState.Crouch)
            {
                m_CapsuleCollider.center = m_CapsuleCrouchState.center;
                m_CapsuleCollider.height = m_CapsuleCrouchState.height;
                m_CapsuleCollider.direction = m_CapsuleCrouchState.direction;
                m_CapsuleCollider.radius = m_CapsuleCrouchState.radius;
            }
            else if (m_PostureState == PostureState.Prone)
            {
                m_CapsuleCollider.center = m_CapsuleProneState.center;
                m_CapsuleCollider.height = m_CapsuleProneState.height;
                m_CapsuleCollider.direction = m_CapsuleProneState.direction;
                m_CapsuleCollider.radius = m_CapsuleProneState.radius;
                ProneStateUpdate();
            }
            else if (m_PostureState == PostureState.Swim)
            {
                m_Rigidbody.useGravity = false;
                m_Rigidbody.velocity = velocity.GetVectorXZ();
            }
            else
            {
                m_CapsuleCollider.center = m_CapsuleStandState.center;
                m_CapsuleCollider.height = m_CapsuleStandState.height;
                m_CapsuleCollider.direction = m_CapsuleStandState.direction;
                m_CapsuleCollider.radius = m_CapsuleStandState.radius;
            }

            // 设置上半身控制数据
            if (m_PostureState == PostureState.Prone)
            {
                m_Animator.SetBool("AttackUpperBody", false);
                m_UpperBodyController.SetNeedFollow(false);
                m_Animator.SetFloat("ProneWeight", 1.0f);
            }
            else
            {
                m_UpperBodyController.SetNeedFollow(true);
                m_Animator.SetFloat("ProneWeight", 0.0f);
            }

            if (m_PostureState == PostureState.Parachute)
            {
                m_Animator.SetLayerWeight(SwimLayer, 0);
            }

            if (oldPostureState == PostureState.Swim && m_PostureState != PostureState.Swim)
            {
                m_Rigidbody.useGravity = true;
            }

            RefreshUpperBodyState();

            UpdateIsKinematic();

            PhysicsPropChange();
        }

        public void MeleeAttack(int meleeType, OnMeleeHitEventHandler onMeleeHit)
        {
            if (m_OnMeleeHit != null)
            {
                return;
            }
            m_Animator.SetFloat("Fist", (float)meleeType);
            m_Animator.SetTrigger("Attack");
            m_OnMeleeHit = onMeleeHit;
            m_bCalDamage = false;
        }

        public void OnFistStateExit()
        {
            m_bCalDamage = false;
            m_OnMeleeHit = null;
            if (onMeleeExit != null)
            {
                onMeleeExit();
            }
        }

        public bool IsMeleeState()
        {
            return m_OnMeleeHit != null;
        }

        public void OnRightFist()
        {
            if (OnMeleeAction != null)
            {
                OnMeleeAction();
            }
        }

        public void OnLeftFist()
        {
            if (OnMeleeAction != null)
            {
                OnMeleeAction();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null)
            {
                if (!m_bCalDamage)
                {
                    // 近战
                    if (m_OnMeleeHit != null)
                    {
                        var hitAction = other.GetComponent<HitAction>();
                        if (hitAction != null)
                        {
                            // 判断是否自己，因为自己小腿绑了rigidbody
                            if (hitAction.characterEntity != this)
                            {
                                var otherCharacter = hitAction.characterEntity;
                                m_OnMeleeHit(otherCharacter.sid, hitAction.bodyPart, other.transform.position, HitActionType.Character);
                                m_OnMeleeHit = null;
                                m_bCalDamage = true;
                            }
                        }
                    }
                }
            }
            
            if (other.gameObject.layer == LayerConfig.Water && !m_PresentationMode)
            {
                BoxCollider waterBox = other.gameObject.GetComponent<BoxCollider>();
                m_WaterLevel = waterBox.center.y + waterBox.size.y * 0.5f +
                    other.gameObject.transform.position.y;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerConfig.Water && !m_PresentationMode)
            {
                //由于手臂碰撞体离开水体也会触发这个，所以加判断
                BoxCollider waterBox = other.gameObject.GetComponent<BoxCollider>();
                if (waterBox.bounds.Contains(transform.position) == false)
                {
                    m_WaterDepth = 0f;
                    m_WaterLevel = float.MinValue;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (onHitGround != null)
            {
                //Debug.LogFormat("OnCollisionEnter: {0}, {1}", collision.relativeVelocity, m_Velocity);
                if (((1 << collision.gameObject.layer) | LayerConfig.GroundMask) != 0)
                {
                    var relativeVelocity = collision.relativeVelocity;
                    onHitGround(relativeVelocity.y, relativeVelocity.GetVectorXZ().magnitude);
                }
            }
        }

        public virtual void SetAimingState(int aimingState)
        {
            m_AimingState = (AimingState)aimingState;

            if (m_AimingState == AimingState.Aim || m_AimingState == AimingState.ADS)
            {
                isAllowAimIK = true;
            }
            else
            {
                isAllowAimIK = false;
            }
        }

        public void HandWeapon(GameObject weaponObject)
        {
            WeaponType weaponType = WeaponType.Unarmed;
            if (weaponObject != null)
            {
                m_WeaponGun = weaponObject.GetComponent<WeaponGun>();
                if (m_WeaponGun != null)
                {
                    if (m_WeaponGun.onRecoilBack != null)
                    {
                        m_WeaponGun.onRecoilBack = null;
                    }
                    if (m_WeaponGun.leftHandTransform != null)
                    {
                        m_LeftHandPoser.poseRoot = m_WeaponGun.leftHandTransform;
                    }
                    if (m_WeaponGun.rightHandTransform != null)
                    {
                        m_RightHandPoser.poseRoot = m_WeaponGun.rightHandTransform;
                    }
                    m_WeaponGun.transform.SetParent(m_RightHandItemTransform, false);
                    m_WeaponGun.transform.localPosition = Vector3.zero;
                    m_WeaponGun.transform.localRotation = Quaternion.identity;

                    m_FullBodyBipedIK.solver.leftArmMapping.weight = 1.0f;
                    m_FullBodyBipedIK.solver.rightArmMapping.weight = 1.0f;
                    weaponType = WeaponType.Gun;
                }
                else
                {
                    if (weaponObject != null)
                    {
                        weaponObject.transform.SetParent(m_RightHandItemTransform, false);
                        weaponObject.transform.localPosition = Vector3.zero;
                        weaponObject.transform.localRotation = Quaternion.identity;
                        weaponType = WeaponType.Missile;
                    }
                }
            }
            else
            {
                if (m_WeaponGun != null)
                {
                    m_WeaponGun.onRecoilBack = null;
                }
                m_WeaponGun = null;
                m_LeftHandPoser.poseRoot = null;
                m_RightHandPoser.poseRoot = null;
                m_FullBodyBipedIK.solver.leftArmMapping.weight = 0.0f;
                m_FullBodyBipedIK.solver.rightArmMapping.weight = 0.0f;
            }
            m_Animator.SetInteger("WeaponType", (int)weaponType);
        }

        public void Reload(float reloadTime, float gunReloadType = 0)
        {
            this.reloadTime = reloadTime;
            m_Animator.SetTrigger("Reload");
            m_Animator.SetFloat("GunReloadType", gunReloadType);
        }

        public void StartReload()
        {
            if (onStartReload != null)
            {
                onStartReload.Invoke();
            }
        }

        public void FinishReload()
        {
            if (onFinishReload != null)
            {
                onFinishReload.Invoke();
            }
        }

        public void ExitReload()
        {
            if (onExitReload != null)
            {
                onExitReload.Invoke();
            }
        }

        public void ThrowWeapon()
        {
            m_WeaponGun = null;
            m_Animator.SetTrigger("ThrowWeapon");
        }

        public void Chambering()
        {
            m_Animator.CrossFade("Weapon_Kar98k_Chambering_01", 0.15f, ReloadLayer);
        }

        public void OnLeftHandIK(bool toggle)
        {
            if (onLeftHandIK != null)
            {
                onLeftHandIK.Invoke(toggle);
            }
            leftHandIKToggle = toggle;
        }

        public void OnRightHandIK(bool toggle)
        {
            if (onRightHandIK != null)
            {
                onRightHandIK.Invoke(toggle);
            }
            rightHandIKToggle = toggle;
        }

        public void OnEnterRigidity()
        {
            if (onEnterRigidity != null)
            {
                onEnterRigidity.Invoke();
            }
        }

        public void OnLeaveRigidity()
        {
            if (onLeaveRigidity != null)
            {
                onLeaveRigidity.Invoke();
            }
        }

        public void OnEnterFullRigidity()
        {
            if (onEnterFullRigidity != null)
            {
                onEnterFullRigidity.Invoke();
            }
        }

        public void OnLeaveFullRigidity()
        {
            if (onLeaveFullRigidity != null)
            {
                onLeaveFullRigidity.Invoke();
            }
        }

        public void OnStandStateEnter()
        {
            if (onStandStateEnter != null)
            {
                onStandStateEnter.Invoke();
            }
        }

        public void OnCrouchStateEnter()
        {
            if (onCrouchStateEnter != null)
            {
                onCrouchStateEnter.Invoke();
            }
        }

        public void OnProneStateEnter()
        {
            if (onProneStateEnter != null)
            {
                onProneStateEnter.Invoke();
            }
        }

        public void OnWoundStateEnter()
        {
            m_Animator.SetBool("Wound", true);
            //if (onWoundStateEnter != null)
            //{
            //    onWoundStateEnter.Invoke();
            //}
        }

        public void OnChamberingStateExit()
        {
            if(onChamberingStateExit != null)
            {
                onChamberingStateExit.Invoke();
            }
        }

        private void OnJumpToProne()
        {
            if (onJumpToProne != null)
            {
                onJumpToProne.Invoke();
            }
        }

        public void TakeWeapon(string animationName, int layer)
        {
            m_Animator.CrossFade(animationName, 0.15f, layer);
        }

        public void PutWeapon(string animationName, int layer)
        {
            m_Animator.CrossFade(animationName, 0.15f, layer);
        }

        //test

        public void TakeWeapon(int slotIdx)
        {
            TakeWeapon(m_AnimationToSlot.takeWeaponAnimationToSlot[slotIdx].animationName, m_AnimationToSlot.takeWeaponAnimationToSlot[slotIdx].layer);
        }

        private void OnTakeWeapon()
        {
            if (onTakeWeapon != null)
            {
                onTakeWeapon.Invoke();
            }
        }

        private void OnTakeWeaponReady()
        {
            if (onTakeWeaponReady != null)
            {
                onTakeWeaponReady.Invoke();
            }
        }

        public void PutWeapon(int slotIdx)
        {
            PutWeapon(m_AnimationToSlot.putWeaponAnimationToSlot[slotIdx].animationName, m_AnimationToSlot.putWeaponAnimationToSlot[slotIdx].layer);
        }

        private void OnPutWeapon()
        {
            if (onPutWeapon != null)
            {
                onPutWeapon.Invoke();
            }
        }

        public void SetMoveSpeedMultiplier(float multiplier)
        {
            m_Animator.SetFloat("MoveSpeedMultiplier", multiplier);
        }

        //将武器挂起
        public void HandUpWeapon(int slotIdx, GameObject gameObject)
        {
            var slotTrans = m_SlotTransforms[slotIdx];
            gameObject.transform.SetParent(slotTrans, false);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
        }

        public void EnterMeleeWeaponState()
        {
            m_Animator.SetInteger("WeaponType", (int)WeaponType.Melee);
        }

        public void ExitMeleeWeaponState()
        {
            m_Animator.SetInteger("WeaponType", (int)WeaponType.Unarmed);
        }

        // 播放拉环动作
        public void PullingBomb()
        {
            m_Animator.SetBool("GrenadeReady", true);
        }

        // 播放扔雷动作
        public void ThrowBomb()
        {
            m_Animator.SetBool("GrenadeThrow", true);
        }

        // 扔雷脱手时回调
        public void OnThrowBomb()
        {
            m_Animator.SetBool("GrenadeReady", false);
            m_Animator.SetBool("GrenadeThrow", false);
            m_Animator.SetInteger("WeaponType", (int)WeaponType.Unarmed);

            if (onThrowBomb != null)
                onThrowBomb.Invoke();
        }

        public void OnRecoil(float distance)
        {
            m_GunRecoil.gunDirection = m_WeaponGun.forward;
            m_GunRecoil.RecoilDist(distance);
        }

        private void FixedUpdate()
        {
            m_Velocity.y = m_Rigidbody.velocity.y;
            m_Rigidbody.velocity = m_Velocity;
        }
        
        private void Update()
        {
            UpdateSwinState();
            if (m_PostureState != PostureState.Parachute)
            {
                if (isVisible)
                {
                    CheckGroundStatus();
                }
            }
            else
            {
                m_IsGrounded = false;
            }

            var velocity = m_Velocity;
            var turnDirection = transform.InverseTransformDirection(velocity.normalized);

            if ((m_IsGrounded || m_PostureState == PostureState.Swim)
                && (Mathf.Abs(velocity.x) > Mathf.Epsilon || Mathf.Abs(velocity.z) > Mathf.Epsilon))
            {
                if (!m_FreeLook)
                {
                    var turnToLookAt = transform.InverseTransformDirection(m_LookForwardTarget.normalized);
                    ApplyExtraTurnRotation(Mathf.Atan2(turnToLookAt.x, turnToLookAt.z));
                }

                if (m_TurningCoroutine != null)
                {
                    StopCoroutine(m_TurningCoroutine);
                    m_Animator.SetFloat("Turn", 0f);
                    m_TurningCoroutine = null;
                }
            }

            // control and velocity handling is different when grounded and airborne:
            if (!m_IsGrounded && m_PostureState != PostureState.Swim)
            {
                HandleAirborneMovement();
            }

            // update m_ForwardAmount and m_Sideslip by velocity
            float speed = velocity.magnitude;
            m_ForwardAmount = turnDirection.z * speed;
            m_Sideslip = turnDirection.x * speed;

            // send input and other state parameters to the animator
            UpdateAnimator();

            if (m_PostureState == PostureState.Prone && isVisible)
            {
                ProneStateUpdate();
            }
            
        }

        public float GetWaterLevel()
        {
            return m_WaterLevel;
        }

        public void SetSwimEffect(int effectID, GameObject obj)
        {
            if (effectID == 1)
                floatEffect = obj;
            else if (effectID == 2)
                swimEffect = obj;
            else if (effectID == 3)
                hitWaterEffectLeft = obj;
            else if (effectID == 4)
                hitWaterEffectRight = obj;
        }

        public void ClearSwimEffects()
        {
            floatEffect = null;
            swimEffect = null;
            hitWaterEffectLeft = null;
            hitWaterEffectRight = null;
        }

        void UpdateSwinState()
        {
            if(m_PostureState == PostureState.Wound)
            {
                return;
            }
            if (m_WaterLevel > float.MinValue * 0.5f)
            {
                float diff = m_WaterLevel - transform.position.y;
                m_WaterDepth = diff;

                if (m_WaterDepth < m_SwinToStandDepth && m_PostureState == PostureState.Swim)
                {
                    //SetPostureState((int)PostureState.Stand);
                    onWaterEvent?.Invoke(false);
                }
                else if (m_WaterDepth > m_SwimDepth)
                {
                    if (m_PostureState != PostureState.Swim)
                    {
                        //SetPostureState((int)PostureState.Swin);
                        onWaterEvent?.Invoke(true);
                    }
                }
                else if (m_WaterDepth > m_InWaterStandDepth)
                {
                    if (m_PostureState == PostureState.Prone || m_PostureState == PostureState.Crouch)
                    {
                        SetPostureState((int)PostureState.Stand);
                    }
                }
                else if (m_WaterDepth > m_InWaterCrouchDepth)
                {
                    if (m_PostureState == PostureState.Prone)
                    {
                        SetPostureState((int)PostureState.Crouch);
                    }
                }
            }

            //process swim position
            if (m_PostureState == PostureState.Swim)
            {
                Vector3 pos = transform.position;
                if (m_WaterDepth > m_SwimDepth)
                {
                    float diff = m_WaterDepth - m_SwimDepth;
                    pos.y += (diff > 0.1f ? 0.1f : diff);
                    transform.position = pos;
                }
            }
        }

        void UpdateSwimEffects()
        {
            if (Mathf.Abs(m_WaterDepth - m_SwimDepth) < 0.15f)
            {
                //bool isFloating = (Mathf.Abs(m_Sideslip) < 0.1f && Mathf.Abs(m_ForwardAmount) < 0.1f &&
                //    m_Velocity.sqrMagnitude < 0.015f);

                bool isFloating = m_Velocity.sqrMagnitude < 0.015f;           

                Vector3 effectPos = m_Animator.GetBoneTransform(HumanBodyBones.Neck).position;
                effectPos.y = m_WaterLevel;

                if(isFloating)
                {
                    elapseCountLeft = elapseCountRight = 0;
                }

                //处理悬浮
                if (isFloating && floatEffect != null)
                {
                    if (floatEffect.activeSelf == false)
                        floatEffect.SetActive(true);

                    floatEffect.transform.position = effectPos;
                }

                //处理游动
                if (isFloating == false && swimEffect != null)
                {
                    if (swimEffect.activeSelf == false)
                        swimEffect.SetActive(true);

                    swimEffect.transform.position = effectPos;
                    swimEffect.transform.rotation = transform.rotation;
                }

                if (isFloating == false && floatEffect && floatEffect.activeSelf == true)
                {
                    floatEffect.SetActive(false);
                }
                if (isFloating == true)
                {
                    if (swimEffect && swimEffect.activeSelf == true)
                        swimEffect.SetActive(false);
                }

                if (elapseCountRight == 0)
                {
                    if (hitWaterEffectRight && hitWaterEffectRight.activeSelf == true)
                        hitWaterEffectRight.SetActive(false);
                }
                if (elapseCountLeft == 0)
                {
                    if (hitWaterEffectLeft && hitWaterEffectLeft.activeSelf == true)
                        hitWaterEffectLeft.SetActive(false);
                }

                if (elapseCountLeft > 0)
                    elapseCountLeft--;
                if(elapseCountRight > 0)
                    elapseCountRight--;
            }
        }

        public void OnSwimBoth()
        {
            OnSwimLeft();
            OnSwimRight();
        }

        public void OnSwimLeft()
        {
            if (hitWaterEffectLeft == null)
                return;

            if (hitWaterEffectLeft.activeSelf == false)
                hitWaterEffectLeft.SetActive(true);

            hitWaterEffectLeft.transform.position = m_LeftHandTransform.position;
            elapseCountLeft = 14;
        }

        public void OnSwimRight()
        {
            if (hitWaterEffectRight == null)
                return;

            if (hitWaterEffectRight.activeSelf == false)
                hitWaterEffectRight.SetActive(true);

            hitWaterEffectRight.transform.position = m_RightHandTransform.position;
            elapseCountRight = 14;
        }

        void CheckGroundStatus()
        {
            CheckGroundDistance();

            if(IsOnVehicle())
            {
                m_IsGrounded = false;
                return;
            }

            var moveDirMag = m_MoveDir.magnitude;
            if (m_IsGrounded && moveDirMag < Vector2.kEpsilon)
            {
                m_CapsuleCollider.material = m_MaxFrictionMaterial;
            }
            else if (m_IsGrounded && moveDirMag >= Vector2.kEpsilon)
            {
                m_CapsuleCollider.material = m_FrictionMaterial;
            }
            var onStep = StepOffset();
#if UNITY_EDITOR
            // helper to visualise the ground check ray in the scene view
            Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
#endif

            if (m_GroundDistance < m_GroundCheckDistance + (onStep ? m_StepEnd : 0.0f))
            {
                m_IsGrounded = true;
            }
            else
            {
                m_IsGrounded = false;
            }
        }

        void CheckGroundDistance()
        {
            if (m_CapsuleCollider != null)
            {
                float capsuleRadius = m_CapsuleCollider.radius;
                float sphereRadius = capsuleRadius * 0.5f;

                var dist = 10f;
                Vector3 pos = transform.position + Vector3.up * (capsuleRadius);
                Ray ray = new Ray(pos, Vector3.down);
                var groundCheckDist = capsuleRadius + m_StepEnd + m_OrigGroundCheckDistance + 0.05f;
                if (Physics.SphereCast(ray, sphereRadius, out m_GroundHit, groundCheckDist, LayerConfig.WalkMask))
                {
                    if (dist > (m_GroundHit.distance - capsuleRadius))
                    {
                        dist = (m_GroundHit.distance - capsuleRadius);
                    }
                }
                m_GroundDistance = (float)System.Math.Round(dist, 2);
            }
        }

        public void Dying()
        {
            m_Animator.SetTrigger("WoundTrigger");
            m_Animator.SetBool("Wound", true);
        }

        public void Revive(bool isReviving)
        {
            m_Animator.SetBool("Revive", isReviving);
        }

        public void Rebirth()
        {
            m_Animator.SetBool("Wound", false);
        }

        bool StepOffset()
        {
            if (!m_IsGrounded || m_MoveDir.magnitude < 0.1f || m_PostureState == PostureState.Prone)
            {
                return false;
            }

            RaycastHit hit;
            var movementDirection = m_MoveDir.magnitude > 0 ? new Vector3(m_MoveDir.x, 0, m_MoveDir.y) : transform.forward;
            var rayStartPos = transform.position
                                    + new Vector3(0, m_StepEnd, 0)
                                    + movementDirection * (m_CapsuleCollider.radius + 0.05f);
            Ray rayStep = new Ray(rayStartPos, Vector3.down);

            Debug.DrawLine(rayStep.origin, rayStep.origin + rayStep.direction * (m_StepEnd - m_StepStart), Color.red);

            if (Physics.Raycast(rayStep, out hit, m_CapsuleCollider.height, LayerConfig.WalkMask))
            {
                var y = transform.position.y;
                if (hit.point.y - y <= m_StepStart)
                {
                    return false;
                }

                if (hit.point.y >= y && hit.point.y <= (y + m_StepEnd))
                {
                    float speed = Mathf.Max(m_Speed, m_MinStepSpeed);
                    var stepSpeed = speed;

                    var velocityDirection = (hit.point - transform.position).normalized;
                    var velocityDirectionXZ = velocityDirection.GetVectorXZ();
                    if (Vector3.Angle(velocityDirection, velocityDirectionXZ) >= m_StepSlopLimit)
                    {
                        stepSpeed *= Vector3.Project(velocityDirection, velocityDirection.GetVectorXZ()).magnitude;
                    }
                    m_Rigidbody.velocity = velocityDirection * stepSpeed;
                    // hack: 临时性解决跳楼触发爬楼梯后，y轴速度不变的bug
                    m_Rigidbody.AddForce(m_Rigidbody.mass * Physics.gravity);
                    Debug.DrawLine(transform.position, transform.position + m_Rigidbody.velocity, Color.red);
                    return true;
                }
            }
            return false;
        }

        void ApplyExtraTurnRotation(float turnAmount)
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, m_ForwardAmount);
            transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
        }

        void HandleAirborneMovement()
        {
            m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
        }
        
        void UpdateAnimator()
        {
            // update the animator parameters
            if (!IsOnVehicle())
            {
                m_Animator.SetFloat("Forward", m_ForwardAmount, 0.1f, Time.deltaTime);
                m_Animator.SetFloat("Sideslip", m_Sideslip, 0.1f, Time.deltaTime);
            }
            m_Animator.SetBool("OnGround", m_IsGrounded);

            if (!m_IsGrounded)
            {
                m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
            }

            if (m_PostureState == PostureState.Swim)
            {
                m_SwimLayerWeight += 0.1f;
                m_SwimLayerWeight = Mathf.Clamp(m_SwimLayerWeight, 0, 1.0f);

                m_Animator.SetLayerWeight(SwimLayer, m_SwimLayerWeight);
            }
            else
            {
                m_SwimLayerWeight -= 0.1f;
                m_SwimLayerWeight = Mathf.Clamp(m_SwimLayerWeight, 0, 1.0f);        
                m_Animator.SetLayerWeight(SwimLayer, m_SwimLayerWeight);
                
                // 速度几乎为0时，使用攻击动作时把上半身关闭, 挥拳动作在蹲下状态时，需要使用上半身动作
                if (m_PostureState != PostureState.Crouch &&
                    Mathf.Abs(m_ForwardAmount) < WholeBodyAttackSpeedLimit && Mathf.Abs(m_Sideslip) < WholeBodyAttackSpeedLimit)
                {
                    m_Animator.SetBool("AttackUpperBody", false);
                }
                else
                {
                    m_Animator.SetBool("AttackUpperBody", true);
                }
            }
        }

        private void RefreshUpperBodyState()
        {
            if (m_UpperBodyToggle == true && 
                (!(m_PostureState == PostureState.Parachute || m_PostureState == PostureState.Swim)))
            {
                SetUpperBodyOn();
            }
            else
            {
                // 卧倒后用全身动作
                SetUpperBodyOff();
            }
        }

        private void SetUpperBodyOn()
        {
            m_Animator.SetBool("UpperBody", true);
            m_Animator.SetLayerWeight(UpperBodyLayer, 1f);
            m_Animator.SetLayerWeight(LeftHandLayer, 0.5f);
            m_Animator.SetLayerWeight(RightHandLayer, 1.0f);
        }

        private void SetUpperBodyOff()
        {
            m_Animator.SetBool("UpperBody", false);
            m_Animator.SetLayerWeight(UpperBodyLayer, 0f);
            m_Animator.SetLayerWeight(LeftHandLayer, 0.0f);
            m_Animator.SetLayerWeight(RightHandLayer, 0.0f);
        }

        private void ProneStateUpdate()
        {
            Vector3 oldPos = transform.position;
            Vector3 headPos = transform.position + transform.up * m_CapsuleCollider.radius + transform.forward * m_CapsuleCollider.height * 0.5f;
            Ray headRay = new Ray(headPos, Vector3.down);
            RaycastHit headHitInfor;
            if (!Physics.Raycast(headRay, out headHitInfor, m_CapsuleCollider.height / 2 + 2f, LayerConfig.WalkMask))
            {
                headHitInfor.normal = transform.up;
            }

            Vector3 footPos = transform.position + transform.up * m_CapsuleCollider.radius - transform.forward * m_CapsuleCollider.height * 0.5f;
            Ray footRay = new Ray(footPos, Vector3.down);
            RaycastHit footHitInfor;
            if (!Physics.Raycast(headRay, out footHitInfor, m_CapsuleCollider.height / 2 + 2f, LayerConfig.WalkMask))
            {
                footHitInfor.normal = transform.up;
            }

            Vector3 newNormal = (m_GroundHit.normal * 0.2f + headHitInfor.normal * 0.5f + footHitInfor.normal * 0.3f).normalized;
            Quaternion rotation = Quaternion.FromToRotation(transform.up, newNormal);
            rotation = rotation * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10 * Time.deltaTime);
            Vector3 e = transform.rotation.eulerAngles;
            e.z = 0;
            transform.rotation = Quaternion.Euler(e);
            transform.position = oldPos;
        }

        // 在一定水位高度不能蹲下
        public bool CanCrouch()
        {
            if (m_WaterLevel > float.MinValue * 0.5f)
            {
                if (m_InWaterCrouchDepth + m_InWaterPostureBias < m_WaterDepth 
                    && m_WaterDepth < m_InWaterStandDepth - m_InWaterPostureBias)
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        public bool CanCrouchUp()
        {
            return AllowActionInLimitSpace(0.8f);
        }

        public bool CanStandUp()
        {
            return AllowActionInLimitSpace(1.8f);
        }

        private bool AllowActionInLimitSpace(float smallestHeight)
        {
            Ray crouchRay = new Ray(transform.position + transform.up * 0.2f, transform.up);
            if (!Physics.Raycast(crouchRay, smallestHeight, LayerConfig.WalkMask))
            {
                return true;
            }
            return false;
        }

        /*
         * 检测当前所处的地形是否能够趴下
         * 当处于水平面和楼梯上时预测趴下时，人头部和脚步所在的位置，
         * 从当前头部的位置向预测的位置测摄像查询，如果产生碰撞，表明当前空间不够，则禁止趴下
         * 当处于斜坡上，预测趴下位置时，需要先计算与斜坡平行的直线，然后做预测
         * 同时还需要注意坡度限制
         */
        public bool CanProne()
        {
            if (m_WaterLevel > float.MinValue * 0.5f 
                && m_WaterDepth > m_InWaterCrouchDepth - m_InWaterPostureBias)
            {
                return false;
            }
            if (!m_IsGrounded)
            {
                return false;
            }
            Ray detectRay = new Ray(transform.position + transform.up + transform.forward * m_CapsuleCollider.radius * 1.2f, Vector3.down);
            Ray groundRay = new Ray(transform.position + transform.up, Vector3.down);
            RaycastHit detectRayInfor;
            RaycastHit groundRayInfor;

            if (!Physics.Raycast(detectRay.origin, detectRay.direction, out detectRayInfor, m_CapsuleCollider.height * 2, LayerConfig.WalkMask))
            {
                detectRayInfor.point = transform.position;
                detectRayInfor.normal = transform.up;
            }

            if (!Physics.Raycast(groundRay.origin, groundRay.direction, out groundRayInfor, m_CapsuleCollider.height * 2, LayerConfig.WalkMask))
            {
                groundRayInfor.point = transform.position;
                groundRayInfor.normal = transform.up;
            }

            if (Vector3.Angle(groundRayInfor.normal, Vector3.up) < 10.0f && Vector3.Angle(detectRayInfor.normal, Vector3.up) < 10.0f)
            {
                //如果在平地或在楼梯上
                Vector3 footPos = transform.position + transform.up * 0.15f - transform.forward * 1.0f;//1.0人物高度的一半
                Vector3 headPos = transform.position + transform.up * m_CapsuleCollider.height;
                Vector3 forwardPos = transform.position + transform.up * 0.3f + transform.forward * 1.0f;//1.0人物高度的一半
                Vector3 backwardPos = forwardPos - transform.forward * 2.0f;//2.0人物高度的
                Debug.DrawLine(forwardPos, backwardPos);
                bool forwardHit = Physics.SphereCast(headPos, 0.1f, (forwardPos - headPos).normalized, out detectRayInfor,
                                                        (forwardPos - headPos).magnitude, LayerConfig.WalkMask);
                bool backwardHit = Physics.SphereCast(headPos, 0.1f, (backwardPos - headPos).normalized, out detectRayInfor,
                                                        (backwardPos - headPos).magnitude, LayerConfig.WalkMask);
                if (forwardHit || backwardHit)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                //如果在斜坡上
                Vector3 newNormal;
                if (Vector3.Angle(Vector3.up, groundRayInfor.normal) > Vector3.Angle(Vector3.up, detectRayInfor.normal))
                {
                    newNormal = groundRayInfor.normal;
                }
                else
                {
                    newNormal = detectRayInfor.normal;
                }
                newNormal = transform.InverseTransformDirection(newNormal);
                if (Mathf.Abs(newNormal.x) > Mathf.Abs(newNormal.y) && Mathf.Abs(newNormal.x) > Mathf.Abs(newNormal.z))
                {
                    return false;
                }
                newNormal.x = 0;
                newNormal = transform.TransformDirection(newNormal);
                Vector3 forward = Vector3.Cross(transform.right, newNormal);

                if (Vector3.Angle(newNormal, Vector3.up) > m_SlopeLimit)
                {
                    return false;
                }

                Vector3 headPos = transform.position + transform.up * m_CapsuleCollider.height;
                Vector3 forwardPos = transform.position + newNormal * 0.3f + forward * 1.0f;//1.0人物高度的一半
                Vector3 backwardPos = forwardPos - forward * 2.0f;//2.0人物高度的
                Debug.DrawLine(forwardPos, backwardPos);
                bool forwardHit = Physics.SphereCast(headPos, 0.05f, (forwardPos - headPos).normalized, out detectRayInfor,
                                                        (forwardPos - headPos).magnitude, LayerConfig.WalkMask);
                bool backwardHit = Physics.SphereCast(headPos, 0.05f, (backwardPos - headPos).normalized, out detectRayInfor,
                                                        (backwardPos - headPos).magnitude, LayerConfig.WalkMask);
                if (forwardHit || backwardHit)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool Jump()
        {
            // check whether conditions are right to allow a jump:
            if (m_IsGrounded && (m_PostureState != PostureState.Prone || m_PostureState != PostureState.Crouch) && !m_IsInSpecialJumpState)
            {
                CheckJumpType();
                switch (jumpType)
                {
                    case 0:
                    case 10:
                        return GeneralJump();
                    default:
                        return SpecialJump();
                }
            }
            return false;
        }

        private bool GeneralJump()
        {
            foreach (string name in m_StandStateNames)
            {
                if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName(name))
                {
                    var velocity = m_Rigidbody.velocity;
                    m_Rigidbody.velocity = new Vector3(velocity.x, m_JumpPower, velocity.z);
                    m_IsGrounded = false;
                    m_GroundCheckDistance = 0.01f;
                    m_Animator.SetBool("OnGround", m_IsGrounded);
                    return true;
                }
            }
            return false;
        }

        public bool SpecialJump()
        {
            BeginSpecialJump();
            m_Animator.CrossFade("Jump" + jumpType, 0.0f, 8);
            return true;
        }

        private void BeginSpecialJump() //为了尽量保证各个客户端动作的同步，BeginSpecialJump需要在SpecialJump里进行调用
        {
            if(onBeginSpecialJump != null)
            {
                onBeginSpecialJump.Invoke();
            }
        }

        public void EndSpecialJump()
        {
            if(onEndSpecialJump != null)
            {
                onEndSpecialJump.Invoke();
            }
        }

        private void CheckJumpType()
        {
            jumpType = 0;
            bool isOverJump = true;
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            Vector3 forward = transform.forward;
            Vector3 up = Vector3.up;
            Vector3 pos = transform.position + up * capsuleCollider.height * 1.3f + forward * capsuleCollider.radius * 1.05f;
            float charHeight = capsuleCollider.height;
            RaycastHit hitInfo;
            Ray ray = new Ray(pos, Vector3.down);

            if (Physics.SphereCast(ray, capsuleCollider.radius, out hitInfo, capsuleCollider.height, LayerConfig.WalkMask))
            {
                Ray rayForward = new Ray(pos, forward);
                if (Physics.SphereCast(rayForward, capsuleCollider.radius, capsuleCollider.radius * 4.05f))
                {
                    jumpType = 0;
                    return;
                }
                float dis = Mathf.Abs(hitInfo.point.y - transform.position.y);
                Ray rayFurther = new Ray(pos + forward * capsuleCollider.radius * 3.0f, Vector3.down); //这里的0.7表示向前探测的距离，用于判断墙的厚度
                RaycastHit hitInfoFurther;
                if (Physics.SphereCast(rayFurther, capsuleCollider.radius, out hitInfoFurther, 100.0f, LayerConfig.WalkMask))
                {
                    Ray rayFurther2 = new Ray(ray.origin + forward * capsuleCollider.radius * 0.2f, Vector3.down);
                    RaycastHit hitInfoFurther2;
                    if (Physics.SphereCast(rayFurther2, capsuleCollider.radius * 0.1f, out hitInfoFurther2, 100.0f, LayerConfig.WalkMask))
                    {
                        float tempDis = Mathf.Abs(hitInfoFurther2.point.y - hitInfo.point.y);
                        if (tempDis > downOverJumpThreshold * 0.5f)
                        {
                            jumpType = 0;
                            return;
                        }
                    }
                    else
                    {
                        jumpType = 0;
                        return;
                    }
                    float vDis = hitInfoFurther.point.y - hitInfo.point.y;
                    if (vDis > -downOverJumpThreshold)
                    {
                        isOverJump = false;
                    }
                }
                else
                {
                    jumpType = 0;
                    return;
                }
                posForJump = hitInfo.point;
                if (isOverJump == false)
                {
                    jumpType += 10;
                }

                if (dis < 0.5f)
                {
                    jumpType = 0;
                }
                else if (dis < 0.7f)
                {
                    jumpType += 1;
                }
                else if (dis < 1.0f)
                {
                    jumpType += 2;
                }
                else if (dis < 1.3f)
                {
                    jumpType += 3;
                }
                else if (dis < 1.6f)
                {
                    jumpType += 4;
                }
                else
                {
                    jumpType = 0;
                }
            }
            return;
        }

        public void Dealth()
        {
            //死亡之后的碰撞体与趴下时一致
            m_CapsuleCollider.center = m_CapsuleProneState.center;
            m_CapsuleCollider.height = m_CapsuleProneState.height;
            m_CapsuleCollider.direction = m_CapsuleProneState.direction;
            m_Animator.SetTrigger("Dealth");
        }

        public Vector2 Redirect(Vector2 dir)
        {
            var forward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 newDir = dir.y * forward + dir.x * transform.right;
            return new Vector2(newDir.x, newDir.z);
        }

        void UpdateHandEffectorWeight()
        {
            var leftHandPosWeight = m_LeftHandPositionWeight;
            var leftHandRotationWeight = m_LeftHandRotationWeight;
            var rightHandPosWeight = m_RightHandPositionWeight;
            var rightHandRotationWeight = m_RightHandRotationWeight;

            if (upperBodyIkEnter != 0)
            {
                leftHandPosWeight = upperBodyLeftHandPosWeight;
                leftHandRotationWeight = upperBodyLeftHandRotWeight;
                rightHandPosWeight = upperBodyRightHandPosWeight;
                rightHandRotationWeight = upperBodyRightHandRotWeight;
            }

            if(m_IsRunState)
            {
                leftHandPosWeight = m_LeftHandIKWeightInRunState;
                leftHandRotationWeight = m_LeftHandIKWeightInRunState;
                rightHandPosWeight = m_RightHandIKWeightInRunState;
                rightHandRotationWeight = m_RightHandIKWeightInRunState;
            }
    
            if (m_WeaponGun == null)
            {
                leftHandPosWeight = 0f;
                leftHandRotationWeight = 0f;
                rightHandPosWeight = 0f;
                rightHandRotationWeight = 0f;
            }

            float factor = Time.deltaTime * m_HandWeightMultipier;
            m_LeftHandEffector.positionWeight = Mathf.Lerp(m_LeftHandEffector.positionWeight, leftHandPosWeight, factor);
            m_LeftHandEffector.rotationWeight = Mathf.Lerp(m_LeftHandEffector.rotationWeight, leftHandRotationWeight, factor);

            m_RightHandEffector.positionWeight = Mathf.Lerp(m_RightHandEffector.positionWeight, rightHandPosWeight, factor);
            m_RightHandEffector.rotationWeight = Mathf.Lerp(m_RightHandEffector.rotationWeight, rightHandRotationWeight, factor);
            m_FullBodyBipedIK.solver.rightArmChain.bendConstraint.weight = rightHandPosWeight;
            m_FullBodyBipedIK.solver.leftArmChain.bendConstraint.weight = leftHandPosWeight;
        }

        private void UpdateAimIK(Vector3 aimPos)
        {
            if (m_AimWeight <= Mathf.Epsilon)
            {
                return;
            }

            if (m_WeaponGun != null)
            {
                // 修正左手IK坐标
                var gunLeftLocalPos = m_IKHandGun.InverseTransformPoint(m_WeaponGun.leftHandTransform.position);
                var gunLocalPos = m_IKHandGun.InverseTransformPoint(m_WeaponGun.transform.position);

                //Debug.DrawLine(m_WeaponGun.leftHandTransform.position, m_IKHandGunLeft.position, Color.red);
                m_IKHandGunLeft.localPosition = gunLeftLocalPos - gunLocalPos;
            }

            // 腰、胸转向IK
            Vector3 forward = transform.forward;
            foreach (var aimSetting in m_AimSettings)
            {
                float weight = aimSetting.aimWeight;
                if (weight > 0f)
                {
                    Transform aimTrans = aimSetting.aimTransform;
                    Vector3 aimForward = aimPos - aimTrans.position;

                    Vector3 aimForwardXZ = aimForward.GetVectorXZ();
                    Quaternion rotateY = Quaternion.FromToRotation(forward, aimForwardXZ);
                    Quaternion rotateXZ = Quaternion.FromToRotation(aimForwardXZ, aimForward);
                    var targetRotate = Quaternion.Slerp(Quaternion.identity, rotateXZ, aimSetting.aimVerticalWeight) * rotateY;
                    var deltaRotate = Quaternion.Slerp(Quaternion.identity, targetRotate, weight * m_AimWeight);

                    aimTrans.rotation = deltaRotate * aimTrans.rotation;
                }
            }

            if (m_WeaponGun != null)
            {
                // 武器IK
                m_IKHandGun.parent.localPosition += m_IKHandGunRootOffset[(int)m_PostureState];
                var anchorOffset = m_IKHandGunAnchorOffset[(int)m_PostureState];
                var gunFixedPos = m_IKHandGun.localPosition + anchorOffset;
                gunFixedPos.z = anchorOffset.z;
                m_IKHandGun.localPosition = m_IKHandGun.localPosition + anchorOffset;

                var ikGunWeight = m_IKHandGunWeight * m_AimWeight;

                Vector3 aimForward = aimPos - m_IKHandGun.position;
                Vector3 aimForwardXZ = aimForward.GetVectorXZ();
                Vector3 gunZeroPos = m_IKHandGun.parent.TransformPoint(gunFixedPos);
                Quaternion rotateY = Quaternion.FromToRotation((m_IKHandGun.position - gunZeroPos).GetVectorXZ(), aimForwardXZ);
                rotateY = Quaternion.Slerp(Quaternion.identity, rotateY, ikGunWeight);
                m_IKHandGun.parent.rotation = rotateY * m_IKHandGun.parent.rotation;

                Vector3 anchorPos = m_IKHandGun.parent.TransformPoint(gunFixedPos);
                Vector3 gunDirection = m_IKHandGun.position - anchorPos;
                Vector3 gunTargetDirection = aimPos - m_IKHandGun.position;
                Quaternion rotateXZ = Quaternion.FromToRotation(gunDirection, gunTargetDirection);
                Quaternion realRotateXZ = Quaternion.Slerp(Quaternion.identity, rotateXZ, ikGunWeight);
                m_IKHandGun.position = realRotateXZ * gunDirection + anchorPos;

                m_IKHandGunLeft.position = rotateXZ * (m_IKHandGunLeft.position - m_IKHandGun.position) + m_IKHandGun.position;
                m_IKHandGunRight.position = rotateXZ * (m_IKHandGunRight.position - m_IKHandGun.position) + m_IKHandGun.position;

                //Debug.DrawLine(m_IKHandGun.position, anchorPos, Color.red);
                //Debug.DrawLine(m_IKHandGun.position, aimPos, Color.blue);

                m_RightHandEffector.position = m_IKHandGunRight.position;

                Vector3 gunDirectionNew = aimPos - m_IKHandGun.position;
                var deltaRotation = Quaternion.FromToRotation(m_WeaponGun.forward, gunDirectionNew);
                m_RightHandEffector.rotation = deltaRotation * m_RightHandTransform.rotation;

                m_LeftHandEffector.position = m_IKHandGunLeft.position;
                m_LeftHandEffector.rotation = deltaRotation * m_WeaponGun.leftHandTransform.rotation;
            }
        }

        private void UpdateADSIK(Vector3 aimPos)
        {
            if (m_WeaponGun == null)
            {
                return;
            }

            foreach (var adsSetting in m_ADSSettings)
            {
                adsSetting.localOffset = m_IKHandGun.transform.InverseTransformPoint(adsSetting.aimTransform.position)
                                            + adsSetting.offset;
            }

            m_CameraFppTransform.position = m_WeaponGun.aimAnchorTransform.position;
            m_IKHandGunLeft.position = m_WeaponGun.leftHandTransform.position;

            Vector3 aimForwardXZ = (aimPos - transform.position).GetVectorXZ();
            var targetRotationY = Quaternion.FromToRotation(transform.forward, aimForwardXZ);
            m_IKAimRoot.rotation = targetRotationY * m_IKAimRoot.rotation;
            m_IKAimRoot.position = targetRotationY * m_IKAimRoot.localPosition + transform.position;
            bool isValidity = true;
            Vector3 rotatePoint = Common.MathUtil.CalculateSameLengthPoint(m_IKAimRoot.position, m_CameraFppTransform.position,
                                (aimPos - m_CameraFppTransform.position).GetVectorXZ(), aimPos, out isValidity);
            if(isValidity == false)
            {
                return;
            }
            Quaternion targetRotationXZ = Quaternion.FromToRotation(rotatePoint - m_IKAimRoot.position, aimPos - m_IKAimRoot.position);
            m_IKAimRoot.rotation = targetRotationXZ * m_IKAimRoot.rotation;

            //Debug.DrawLine(m_IKAimRoot.position, m_CameraFppTransform.position, Color.green);

            // 旋转IKHandGun骨骼点
            m_IKHandGun.parent.rotation = targetRotationY * m_IKHandGun.parent.rotation;
            m_IKHandGun.rotation = targetRotationXZ * m_IKHandGun.rotation;
            m_IKHandGun.position = targetRotationXZ * (m_IKHandGun.position - m_IKAimRoot.position) + m_IKAimRoot.position;
            //Debug.DrawLine(m_CameraFppTransform.position, m_IKHandGun.position, Color.red);

            // 更新摄像机绑定点
            var cameraTargetPos = m_CameraFppTransform.position;
            var cameraLocalTargetPos = cameraTargetPos - m_IKAimRoot.position;
            m_AnchorMirrorPos = Vector3.Slerp(m_AnchorMirrorPos, cameraLocalTargetPos, m_ADSAnchorMultiplier * Time.deltaTime);
            m_CameraAnchorTransform.position = cameraTargetPos * 2 - (m_AnchorMirrorPos + m_IKAimRoot.position);

            // 旋转身体躯干
            Vector3 forward = transform.forward;
            foreach (var adsSetting in m_ADSSettings)
            {
                float weight = adsSetting.aimWeight;
                Transform aimTrans = adsSetting.aimTransform;
                Vector3 targetForward = aimPos - aimTrans.position;

                Vector3 targetForwardXZ = targetForward.GetVectorXZ();
                Quaternion rotateY = Quaternion.FromToRotation(forward, targetForwardXZ);
                rotateY = Quaternion.Slerp(Quaternion.identity, rotateY, weight);

                Quaternion rotateXZ = Quaternion.FromToRotation(targetForwardXZ, targetForward);
                var deltaRotate = Quaternion.Slerp(Quaternion.identity, rotateXZ, adsSetting.aimVerticalWeight) * rotateY;

                aimTrans.rotation = deltaRotate * aimTrans.rotation;

                aimTrans.position = Vector3.Lerp(aimTrans.position,
                                    m_IKHandGun.transform.TransformPoint(adsSetting.localOffset), adsSetting.keepPosWeight);
            }

            m_RightHandEffector.position = m_IKHandGunRight.position;

            // 计算左手IK点
            m_LeftHandEffector.position = m_IKHandGunLeft.position;

            var aimIKDir = aimPos - cameraTargetPos;
            var deltaRotation = Quaternion.FromToRotation(m_WeaponGun.forward, aimIKDir);
            Debug.DrawRay(m_CameraAnchorTransform.position, aimIKDir, Color.red);

            var rightHandRotation = deltaRotation * m_RightHandTransform.rotation;
            var leftHandRotation = deltaRotation * m_WeaponGun.leftHandTransform.rotation; ;

            m_RightHandEffector.rotation = rightHandRotation;
            m_LeftHandEffector.rotation = leftHandRotation;
        }

        private Vector3 CalcFollowPoint(Vector3 lastLocalPos, Vector3 targetLocalPos, float followSpeed, float limit)
        {
            var forward = targetLocalPos - lastLocalPos;
            var delta = forward.normalized * followSpeed * Time.deltaTime;
            if (delta.sqrMagnitude > forward.sqrMagnitude)
            {
                delta = forward;
            }
            Vector3 destPos = lastLocalPos + delta;

            if ((destPos - targetLocalPos).sqrMagnitude > limit * limit)
            {
                destPos = targetLocalPos - forward.normalized * limit;
            }
            return destPos;
        }

        private void UpdateLookIK(Vector3 lookAtPos)
        {
            for (int i = 0; i < m_LookSettings.Length; i++)
            {
                LookSetting setting = m_LookSettings[i];
                Vector3 baseForward = setting.bone.rotation * Vector3.up;
                Vector3 pos = i + 1 < m_LookSettings.Length ? m_LookSettings[i + 1].bone.position : setting.bone.position;
                Vector3 targetForward = (lookAtPos - pos).normalized;
                Quaternion rot = Quaternion.FromToRotation(baseForward, targetForward);
                setting.bone.rotation = Quaternion.Lerp(setting.bone.rotation, rot * setting.bone.rotation, setting.weight * m_LookWeight);
            }
        }

        private void LateUpdate()
        {
            float aimWeightTarget = m_AimWeightTarget;
            // 不持枪不开启AimIK
            if (m_WeaponGun == null)
            {
                aimWeightTarget = 0f;
            }
            m_AimWeight = Mathf.Lerp(m_AimWeight, aimWeightTarget, m_AimWeightMultipier * Time.deltaTime);

            //Debug.DrawLine(transform.position, transform.position + m_Rigidbody.velocity, Color.blue);

            m_LookForward = Vector3.Slerp(m_LookForward, m_LookForwardTarget, Time.deltaTime * m_LookForwardMultipier);

            if (isVisible)
            {
                if (m_ObjectToMove.GetPlayerDistanceSqr() < m_IKUpdateEnableThreshold)
                {
                    var lookAtPos = m_HeadTransform.position + m_LookForward;

                    UpdateHandEffectorWeight();
                    if (m_PostureState <= PostureState.Prone)
                    {
                        if (m_WeaponGun != null)
                        {
                            if (m_IsAllowADS && m_CameraTrans != null)
                            {
                                UpdateADSIK(m_HeadTransform.position + m_LookForwardTarget);
                            }
                            else if (m_IsAllowAimIK)
                            {
                                UpdateAimIK(lookAtPos);
                            }
                            else
                            {
                                m_LeftHandEffector.position = m_WeaponGun.leftHandTransform.position;
                                m_LeftHandEffector.rotation = m_WeaponGun.leftHandTransform.rotation;

                                m_RightHandEffector.position = m_RightHandTransform.position;
                                m_RightHandEffector.rotation = m_RightHandTransform.rotation;
                            }

                            if (m_IsAllowLookIK)
                            {
                                UpdateLookIK(lookAtPos);
                            }

                            if (m_IsAllowFBBK)
                            {
                                m_FullBodyBipedIK.solver.Update();
                            }
                        }
                        else if (m_IsAllowLookIK)
                        {
                            UpdateLookIK(lookAtPos);
                        }
                    }
                }
            }

            //处理游泳水花特效
            if (m_PostureState == PostureState.Swim)
            {
                UpdateSwimEffects();
            }
        }

        public void OnCameraAttach(Transform cameraTrans)
        {
            m_CameraTrans = cameraTrans;

            if (cameraTrans != null)
            {
                // 观察对象的动画一直更新
                m_Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                // 观察对象的Poser开启
                m_LeftHandPoser.enabled = true;
                m_RightHandPoser.enabled = true;
            }
            else
            {
                m_Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

                // 非观察对象的Poser关闭
                m_LeftHandPoser.enabled = false;
                m_RightHandPoser.enabled = false;
            }
        }

        // 大厅演示模式
        public void SetPresentationMode(bool on)
        {
            m_PresentationMode = on;
            m_Animator.SetLayerWeight(LobbyLayer, on ? 1f : 0f);
            UpdateIsKinematic();
        }

        /// <summary>
        /// 播放一个动作
        /// </summary>
        /// <param name="antionID"></param>
        /// <param name="context"></param>
        public void PlayAction(EnActionID antionID, object context)
        {
            m_AniCtrl.PlayAction(antionID, context);
        }

        /// <summary>
        /// 设置动作状态
        /// </summary>
        /// <param name="nState"></param>
        public void SetAniState(EnAniState nState)
        {
            m_AniCtrl.ChangeState(nState);
        }

        /// <summary>
        /// 设置动作参数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void SetAniParam(EnActionParamID param, object value)
        {
            m_AniCtrl.SetAniParam(param, value);
        }

        /// <summary>
        /// 设置父对象，现在存在载具上被无辜拉下来的问题，所以把这个这个行为监控起来，便于追踪类似的问题。
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="worldPositionStays"></param>
        /// <param name="flag"></param>
        public void SetParent(Transform parent, bool worldPositionStays=false, string flag="Unknown")
        {
            transform.SetParent(parent, worldPositionStays);
#if UNITY_EDITOR
            if(null != parent)
            {
                Debug.LogWarningFormat("设置角色{0}的父对象为{1}, flag={2}", sid, parent.name, flag);
            }
            else
            {
                Debug.LogWarningFormat("设置角色{0}的父对象为null, flag={1}", sid, flag);
            }
#endif
        }
    }
}