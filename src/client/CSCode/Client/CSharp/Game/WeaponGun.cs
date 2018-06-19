using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

namespace War.Game
{
    public class WeaponGun : MonoBehaviour
    {
        // 分度
        private const float AngleUnit = 0.01666667f;
        private const float SwayUnit = 0.001f;

        #region OverView
        [SerializeField]
        protected float m_HitDamage;
        public float hitDamage
        {
            get { return m_HitDamage; }
            set { m_HitDamage = value; }
        }

        [SerializeField]
        protected float m_InitialBulletSpeed;
        public float initialBulletSpeed
        {
            get { return m_InitialBulletSpeed; }
            set { m_InitialBulletSpeed = value; }
        }

        [SerializeField]
        protected float m_BodyHitImpactPower;
        public float bodyHitImpactPower
        {
            get { return m_BodyHitImpactPower; }
            set { m_BodyHitImpactPower = value; }
        }

        [SerializeField]
        protected float m_ZeroRange;
        public float zeroRange
        {
            get { return m_ZeroRange; }
            set
            {
                m_ZeroRange = value;

                // m_ZeroRange = m_InitialBulletSpeed^2 * sin(2θ) / gravity
                // Sea: https://en.wikipedia.org/wiki/Rifleman%27s_rule
                float thetaDouble = Mathf.Asin((Physics.gravity.y * m_ZeroRange) / (m_InitialBulletSpeed * m_InitialBulletSpeed));
                float shotGunRadianBias = thetaDouble * 0.5f;

                m_ShotGunRorateBias = Quaternion.Euler(-shotGunRadianBias * Mathf.Rad2Deg, 0f, 0f);
            }
        }

        [SerializeField]
        protected float m_TimeBetweenShots;
        public float timeBetweenShots
        {
            get { return m_TimeBetweenShots; }
            set { m_TimeBetweenShots = value; }
        }

        [SerializeField]
        protected float m_BurstShots;
        public float burstShots
        {
            get { return m_BurstShots; }
            set { m_BurstShots = value; }
        }

        [SerializeField]
        protected float m_BurstDelay;
        public float burstDelay
        {
            get { return m_BurstDelay; }
            set { m_BurstDelay = value; }
        }

        [SerializeField]
        protected int m_ShotCount;
        public int shotCount
        {
            get { return m_ShotCount; }
            set { m_ShotCount = value; }
        }
        #endregion

        #region Spread
        [SerializeField]
        protected float m_ShotSpread;
        public float shotSpread
        {
            get { return m_ShotSpread; }
            set { m_ShotSpread = value; }
        }

        [SerializeField]
        protected float m_BaseSpread;
        public float baseSpread
        {
            get { return m_BaseSpread; }
            set { m_BaseSpread = value; }
        }

        [SerializeField]
        protected float m_AimingModifierSpread;
        public float aimingModifierSpread
        {
            get { return m_AimingModifierSpread; }
            set { m_AimingModifierSpread = value; }
        }

        [SerializeField]
        protected float m_ADSModifierSpread;
        public float ADSModifierSpread
        {
            get { return m_ADSModifierSpread; }
            set { m_ADSModifierSpread = value; }
        }

        [SerializeField]
        protected float m_FiringBaseSpread;
        public float firingBaseSpread
        {
            get { return m_FiringBaseSpread; }
            set { m_FiringBaseSpread = value; }
        }

        [SerializeField]
        protected float m_CrouchModifierSpread;
        public float crouchModifierSpread
        {
            get { return m_CrouchModifierSpread; }
            set { m_CrouchModifierSpread = value; }
        }

        [SerializeField]
        protected float m_ProneModifierSpread;
        public float proneModifierSpread
        {
            get { return m_ProneModifierSpread; }
            set { m_ProneModifierSpread = value; }
        }

        [SerializeField]
        protected float m_WalkModifierSpread;
        public float walkModifierSpread
        {
            get { return m_WalkModifierSpread; }
            set { m_WalkModifierSpread = value; }
        }

        [SerializeField]
        protected float m_RunModifierSpread;
        public float runModifierSpread
        {
            get { return m_RunModifierSpread; }
            set { m_RunModifierSpread = value; }
        }

        [SerializeField]
        protected float m_JumpModifierSpread;
        public float jumpModifierSpread
        {
            get { return m_JumpModifierSpread; }
            set { m_JumpModifierSpread = value; }
        }
        #endregion

        #region Deviation
        [SerializeField]
        protected float m_BaseDeviation;
        public float baseDeviation
        {
            get { return m_BaseDeviation; }
            set { m_BaseDeviation = value; }
        }

        [SerializeField]
        protected float m_BaseAimDeviation;
        public float baseAimDeviation
        {
            get { return m_BaseAimDeviation; }
            set { m_BaseAimDeviation = value; }
        }

        [SerializeField]
        protected float m_BaseADSDeviation;
        public float baseADSDeviation
        {
            get { return m_BaseADSDeviation; }
            set { m_BaseADSDeviation = value; }
        }

        [SerializeField]
        protected float m_RecoilGainDeviation;
        public float recoilGainDeviation
        {
            get { return m_RecoilGainDeviation; }
            set { m_RecoilGainDeviation = value; }
        }

        [SerializeField]
        protected float m_RecoilAimGainDeviation;
        public float recoilAimGainDeviation
        {
            get { return m_RecoilAimGainDeviation; }
            set { m_RecoilAimGainDeviation = value; }
        }

        [SerializeField]
        protected float m_RecoilADSGainDeviation;
        public float recoilADSGainDeviation
        {
            get { return m_RecoilADSGainDeviation; }
            set { m_RecoilADSGainDeviation = value; }
        }

        [SerializeField]
        protected float m_MaxLimitDeviation;
        public float maxLimitDeviation
        {
            get { return m_MaxLimitDeviation; }
            set { m_MaxLimitDeviation = value; }
        }

        [SerializeField]
        protected float m_MoveModifierLimitDevation;
        public float moveModifierLimitDevation
        {
            get { return m_MoveModifierLimitDevation; }
            set { m_MoveModifierLimitDevation = value; }
        }

        [SerializeField]
        protected Vector2 m_MoveVelocityReferenceDeviation;
        public Vector2 moveVelocityReferenceDeviation
        {
            get { return m_MoveVelocityReferenceDeviation; }
            set { m_MoveVelocityReferenceDeviation = value; }
        }

        [SerializeField]
        protected float m_CrouchModifierDeviation;
        public float crouchModifierDeviation
        {
            get { return m_CrouchModifierDeviation; }
            set { m_CrouchModifierDeviation = value; }
        }

        [SerializeField]
        protected float m_ProneModifierDeviation;
        public float proneModifierDeviation
        {
            get { return m_ProneModifierDeviation; }
            set { m_ProneModifierDeviation = value; }
        }
        #endregion

        #region Recoil
        private float m_GunUpTimeFactor = 1.0f;

        [SerializeField]
        protected float m_PatternScale;
        public float patternScale
        {
            get { return m_PatternScale; }
            set { m_PatternScale = value; }
        }

        protected float m_ValueClimb;
        public float valueClimb
        {
            get { return m_ValueClimb; }
            set { m_ValueClimb = value; }
        }

        protected float m_ValueFall;
        public float valueFall
        {
            get { return m_ValueFall; }
            set { m_ValueFall = value; }
        }

        [SerializeField]
        protected Vector2 m_VertClampRecoil;
        public Vector2 vertClampRecoil
        {
            get { return m_VertClampRecoil; }
            set { m_VertClampRecoil = value; }
        }

        [SerializeField]
        [Range(0f, 20f)]
        protected float m_VertSpeedRecoil;
        public float vertSpeedRecoil
        {
            get { return m_VertSpeedRecoil; }
            set { m_VertSpeedRecoil = value; }
        }

        [SerializeField]
        protected float m_VertRecoverySpeedRecoil;
        public float vertRecoverySpeedRecoil
        {
            get { return m_VertRecoverySpeedRecoil; }
            set { m_VertRecoverySpeedRecoil = value; }
        }

        [SerializeField]
        protected float m_VertRecoveryMaxRecoil;
        public float vertRecoveryMaxRecoil
        {
            get { return m_VertRecoveryMaxRecoil; }
            set { m_VertRecoveryMaxRecoil = value; }
        }

        [SerializeField]
        protected float m_VertRecoveryModifier;
        public float vertRecoveryModifier
        {
            get { return m_VertRecoveryModifier; }
            set { m_VertRecoveryModifier = value; }
        }

        [SerializeField]
        protected float m_HorSpeedRecoil;
        public float horSpeedRecoil
        {
            get { return m_HorSpeedRecoil; }
            set { m_HorSpeedRecoil = value; }
        }

        [SerializeField]
        protected float m_HorTendencyRecoil;
        public float horTendencyRecoil
        {
            get { return m_HorTendencyRecoil; }
            set { m_HorTendencyRecoil = value; }
        }

        [SerializeField]
        protected float m_LeftMaxRecoil;
        public float leftMaxRecoil
        {
            get { return m_LeftMaxRecoil; }
            set { m_LeftMaxRecoil = value; }
        }

        [SerializeField]
        protected float m_RightMaxRecoil;
        public float rightMaxRecoil
        {
            get { return m_RightMaxRecoil; }
            set { m_RightMaxRecoil = value; }
        }

        [SerializeField]
        protected float m_SpeedRecoil;
        public float speedRecoil
        {
            get { return m_SpeedRecoil; }
            set { m_SpeedRecoil = value; }
        }

        [SerializeField]
        protected float m_RecoverySpeedRecoil;
        public float recoverySpeedRecoil
        {
            get { return m_RecoverySpeedRecoil; }
            set { m_RecoverySpeedRecoil = value; }
        }

        [SerializeField]
        protected float m_CrouchModifierRecoil;
        public float crouchModifierRecoil
        {
            get { return m_CrouchModifierRecoil; }
            set { m_CrouchModifierRecoil = value; }
        }

        [SerializeField]
        protected float m_ProneModifierRecoil;
        public float proneModifierRecoil
        {
            get { return m_ProneModifierRecoil; }
            set { m_ProneModifierRecoil = value; }
        }
        #endregion

        #region Sway
        [SerializeField]
        protected float m_PitchSway;
        public float pitchSway
        {
            get { return m_PitchSway; }
            set { m_PitchSway = value; }
        }

        [SerializeField]
        protected float m_YawOffsetSway;
        public float yawOffsetSway
        {
            get { return m_YawOffsetSway; }
            set { m_YawOffsetSway = value; }
        }

        [SerializeField]
        protected float m_MovementModifierSway;
        public float movementModifierSway
        {
            get { return m_MovementModifierSway; }
            set { m_MovementModifierSway = value; }
        }

        [SerializeField]
        protected float m_CrouchModifierSway;
        public float crouchModifierSway
        {
            get { return m_CrouchModifierSway; }
            set { m_CrouchModifierSway = value; }
        }

        [SerializeField]
        protected float m_ProneModifierSway;
        public float proneModifierSway
        {
            get { return m_ProneModifierSway; }
            set { m_ProneModifierSway = value; }
        }
        #endregion

        #region Camera DOF

        #endregion

        #region Misc
        [SerializeField]
        protected float m_PickupDelay;

        [SerializeField]
        protected float m_ReadyDelay;
        #endregion

        protected float m_RecoilBackDistance; // 后座力向后距离
        protected Quaternion m_ShotGunRorateBias;

        [SerializeField]
        protected Transform m_LeftHandTransform; // 左手在抢上的位置
        public Transform leftHandTransform
        {
            get { return m_LeftHandTransform; }
        }

        [SerializeField]
        protected Transform m_RightHandTransform; // 右手在抢上的位置
        public Transform rightHandTransform
        {
            get { return m_RightHandTransform; }
        }

        [SerializeField]
        protected Transform m_MuzzleTransform; // 枪口位置
        public Transform muzzleTransform
        {
            get { return m_MuzzleTransform; }
        }

        [SerializeField]
        protected Transform m_BulletRefTransform; // 子弹参考坐标系，与枪口位置相同，但默认z轴朝前
        public Transform bulletRefTransform
        {
            get { return m_BulletRefTransform; }
        }

        [SerializeField]
        protected Transform m_GripTransform; // 握把位置
        public Transform gripTransform
        {
            get { return m_GripTransform; }
        }


        [SerializeField]
        protected Transform m_GunSightTransform; // 瞄镜位置
        public Transform gunSightTransform
        {
            get { return m_GunSightTransform; }
        }

        [SerializeField]
        protected Transform m_GunBaseSightTransform; // 机瞄位置
        public Transform gunBaseSightTransform
        {
            get { return m_GunBaseSightTransform; }
        }

        [SerializeField]
        protected Transform m_GunStockTransform; // 枪托位置
        public Transform gunStockTransform
        {
            get { return m_GunStockTransform; }
        }

        [SerializeField]
        protected Transform m_DefaultGunStockTransform; // 枪托位置
        public Transform defaultGunStockTransform
        {
            get { return m_DefaultGunStockTransform; }
        }

        [SerializeField]
        protected Transform m_MagazineTransform; // 弹夹位置
        public Transform magazineTransform
        {
            get { return m_MagazineTransform; }
        }

        [SerializeField]
        protected Transform m_DefaultMagazineTransform; // 默认弹夹位置
        public Transform defaultMagazineTransform
        {
            get { return m_DefaultMagazineTransform; }
        }

        [Tooltip("准心靠近眼睛部位")]
        [SerializeField]
        protected Transform m_AimAnchorTransform;
        protected Transform m_AimAnchorReplacementTransform;

        public Transform aimAnchorTransform
        {
            get
            {
                if (m_AimAnchorReplacementTransform != null)
                {
                    return m_AimAnchorReplacementTransform;
                }
                return m_AimAnchorTransform;
            }

            set
            {
                m_AimAnchorReplacementTransform = value;
            }
        }

        [Tooltip("准心靠近枪口部位")]
        [SerializeField]
        protected Transform m_AimPointTransform;
        protected Transform m_AimPointReplacementTransform;

        public Transform aimPointTransform
        {
            get
            {
                if (m_AimPointReplacementTransform != null)
                {
                    return m_AimPointReplacementTransform;
                }
                return m_AimPointTransform;
            }

            set
            {
                m_AimPointReplacementTransform = value;
            }
        }

        [Tooltip("弹壳弹出位置")]
        [SerializeField]
        protected Transform m_SlotTransform;
        public Transform slotTransform
        {
            get
            {
                return m_SlotTransform;
            }
            set
            {
                m_SlotTransform = value;
            }
        }

        [Tooltip("枪械朝向")]
        [SerializeField]
        protected Vector3 m_Axis = Vector3.left;

        // 枪口世界坐标系朝向
        public Vector3 forward
        {
            get
            {
                return transform.rotation * m_Axis;
            }
        }

        [SerializeField]
        protected AnimationCurve m_PitchCurve;

        [SerializeField]
        protected AnimationCurve m_YawCurve;

        protected float m_PitchYawTime;

        //由于开枪获得的额外偏差
        private float m_RecoilShotDeviation = 0.0f;

        protected Coroutine m_RecoilCoroutine;

        public delegate void SpawnBulletEventHandler(Vector3 initialVelocity, Vector3 bulletStartPos);

        public delegate void OnRecoilRotateEventHandler(float x, float y, float z);

        protected OnRecoilRotateEventHandler m_OnRecoilRotate;

        
        public OnRecoilRotateEventHandler onRecoilRotate
        {
            set
            {
                m_OnRecoilRotate = value;
            }
        }

        public delegate void OnRecoilBackEventHandler(float distance);

        protected OnRecoilBackEventHandler m_OnRecoilBack;
        public OnRecoilBackEventHandler onRecoilBack
        {
            get
            {
                return m_OnRecoilBack;
            }
            set
            {
                m_OnRecoilBack = value;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            m_RecoilShotDeviation = Mathf.Lerp(m_RecoilShotDeviation, 0.0f, Time.deltaTime * m_ValueFall);
        }

        public void AttachGunSight(GunSightEntity gunSightEntity)
        {
            if (gunSightEntity == null)
            {
                if(m_GunBaseSightTransform != null)
                {
                    m_GunBaseSightTransform.gameObject.SetActive(true);
                }
                m_AimAnchorReplacementTransform = null;
                m_AimPointReplacementTransform = null;
            }
            else
            {
                if (m_GunBaseSightTransform != null)
                {
                    m_GunBaseSightTransform.gameObject.SetActive(false);
                }
                gunSightEntity.transform.SetParent(m_GunSightTransform, false);
                gunSightEntity.transform.localPosition = Vector3.zero;
                gunSightEntity.transform.localRotation = Quaternion.identity;
                m_AimAnchorReplacementTransform = gunSightEntity.aimAnchorTransform;
                m_AimPointReplacementTransform = gunSightEntity.aimPointTransform;
            }
        }

        public float CalculateCrosshair(AimingState aimingState, MoveState moveState,
            PostureState postureState, float velocityMagitude)
        {
            var spreadAndDeviation = CalculateSpreadAndDeviation(aimingState, moveState, postureState, velocityMagitude);
            return spreadAndDeviation.x + spreadAndDeviation.y;
        }

        protected float CalculateSpread(AimingState aimingState, MoveState moveState,
            PostureState postureState, float velocityMagitude)
        {
            float baseSpread = m_BaseSpread;
            if (m_ShotSpread != 0f)
            {
                baseSpread *= m_ShotSpread;
            }

            float aimingModifierSpread = 1f;
            float moveModifierSpread = 1f;
            float postureModifierSpread = 1f;
            switch (aimingState)
            {
                case AimingState.Stand:
                    break;

                case AimingState.Aim:
                    aimingModifierSpread = m_AimingModifierSpread;
                    break;

                case AimingState.ADS:
                    aimingModifierSpread = m_ADSModifierSpread;
                    break;

                default:
                    break;
            }

            switch (moveState)
            {
                case MoveState.Stand:
                    break;

                case MoveState.Walk:
                    moveModifierSpread = m_WalkModifierSpread;
                    break;

                case MoveState.Run:
                    moveModifierSpread = m_RunModifierSpread;
                    break;

                case MoveState.Jump:
                    moveModifierSpread = m_JumpModifierSpread;
                    break;

                default:
                    break;
            }

            switch (postureState)
            {
                case PostureState.Stand:
                    break;

                case PostureState.Crouch:
                    postureModifierSpread = m_CrouchModifierSpread;
                    break;

                case PostureState.Prone:
                    postureModifierSpread = m_ProneModifierSpread;
                    break;
                default:
                    break;
            }
            float spread = baseSpread * aimingModifierSpread + m_FiringBaseSpread * postureModifierSpread * moveModifierSpread + m_ShotSpread * 30.0f;
            return spread;
        }

        protected float CalculateShotDeviation(AimingState aimingState, MoveState moveState,
            PostureState postureState, float velocityMagitude)
        {
            float recoilShotDeviation = 0;
            switch (aimingState)
            {
                case AimingState.Stand:
                    recoilShotDeviation = m_RecoilGainDeviation;
                    break;

                case AimingState.Aim:
                    //recoilShotDeviation = m_RecoilAimGainDeviation;
                    recoilShotDeviation = m_RecoilGainDeviation;
                    break;

                case AimingState.ADS:
                    recoilShotDeviation = m_RecoilADSGainDeviation;
                    break;

                default:
                    recoilShotDeviation = m_RecoilGainDeviation;
                    break;
            }

            m_RecoilShotDeviation += recoilShotDeviation * m_PatternScale * m_ValueClimb;
            m_RecoilShotDeviation = Mathf.Min(recoilShotDeviation * m_MaxLimitDeviation * m_PatternScale * m_ValueClimb, m_RecoilShotDeviation);
            return m_RecoilShotDeviation;
        }

        protected float CalculateDeviation(AimingState aimingState, MoveState moveState,
            PostureState postureState, float velocityMagitude, bool isShoot = false)
        {
            float baseDeviation = 0f;
            float postureModifierDeviation = 1f;

            float moveDeviation = m_MoveModifierLimitDevation *
                Mathf.InverseLerp(m_MoveVelocityReferenceDeviation.x / 100, m_MoveVelocityReferenceDeviation.y / 100, velocityMagitude);
            switch (aimingState)
            {
                case AimingState.Stand:
                    baseDeviation = m_BaseDeviation;
                    break;

                case AimingState.Aim:
                    //baseDeviation = m_BaseAimDeviation;
                    baseDeviation = m_BaseDeviation;
                    break;

                case AimingState.ADS:
                    baseDeviation = m_BaseADSDeviation;
                    break;

                default:
                    baseDeviation = m_BaseDeviation;
                    break;
            }
            switch (postureState)
            {
                case PostureState.Stand:
                    break;

                case PostureState.Crouch:
                    postureModifierDeviation = m_CrouchModifierDeviation;
                    break;

                case PostureState.Prone:
                    postureModifierDeviation = m_ProneModifierDeviation;
                    break;

                default:
                    break;
            }

            float deviation = (baseDeviation + m_RecoilShotDeviation) * (1 + moveDeviation) * postureModifierDeviation;
            return deviation * 60.0f;
        }
        /*
        * 开枪时不同姿势对枪口上升时间的修正
        * */

        private void GunUpTimeModifier(PostureState postureState)
        {
            switch (postureState)
            {
                case PostureState.Stand:
                    m_GunUpTimeFactor = 1.0f;
                    break;

                case PostureState.Crouch:
                    m_GunUpTimeFactor = m_CrouchModifierRecoil;
                    break;

                case PostureState.Prone:
                    m_GunUpTimeFactor = m_ProneModifierRecoil;
                    break;

                default:
                    m_GunUpTimeFactor = 1f;
                    break;
            }
        }

        /*
         * 计算散布以及偏差
         * */
        protected Vector2 CalculateSpreadAndDeviation(AimingState aimingState, MoveState moveState, 
            PostureState postureState, float velocityMagitude)
        {
            float spread = CalculateSpread(aimingState, moveState, postureState, velocityMagitude);
            float deviation = CalculateDeviation(aimingState, moveState, postureState, velocityMagitude);
            return new Vector2(spread, deviation);
        }

        /**
         * @param aimPosition 瞄点
         * @param aimingState 需要播放的动画
         * @param moveState 人物所处的状态，如站立，行走，跑动等
         * @param postureState 人物所处的姿势，如下蹲，趴着等
         * @param velocityMagitude 用于计算偏差的一个参数
         * @param spawnBulletAction 回调函数，第一个参数是造成的伤害，第二个参数是子弹的速度，第三个参数是击中目标后造成的冲击力,第四个参数子弹生成的位置
         */

        public int leixing = 0;
        public bool startRecoil = true;
        public void Shot(Vector3 aimPosition, int aimingState, int moveState,
            int postureState, float velocityMagitude, SpawnBulletEventHandler spawnBulletAction)
        {
            GunUpTimeModifier((PostureState)postureState);
            CalculateShotDeviation((AimingState)aimingState, (MoveState)moveState, 
                                                                    (PostureState)postureState, velocityMagitude);
           var spreadAndDeviation = CalculateSpreadAndDeviation((AimingState)aimingState, (MoveState)moveState, 
                                                                    (PostureState)postureState, velocityMagitude);
            float deviation = spreadAndDeviation.y;
            float spread = spreadAndDeviation.x;
            int shotCount = 1;

            if (m_ShotCount > shotCount)
            {
                shotCount = m_ShotCount;
            }

            if(m_ShotSpread > 0)
            {
                ShotWithShotgun(aimPosition, aimingState, deviation, spread, shotCount, spawnBulletAction);
            }
            else
            {
                ShotWithCommonGun(aimPosition, aimingState, deviation, spread, spawnBulletAction);
            }

            if (m_RecoilCoroutine != null && startRecoil)
            {
                StopCoroutine(m_RecoilCoroutine);
            }
            if(startRecoil)
            {
                m_RecoilCoroutine = StartCoroutine(Recoil());
            }
           
        }

        private void ShotWithCommonGun(Vector3 aimPosition, int aimingState, float deviation, float spread, SpawnBulletEventHandler spawnBulletAction)
        {
            var muzzlePoint = m_MuzzleTransform.position;
            var forward = (aimPosition - muzzlePoint).normalized;
            forward = m_BulletRefTransform.InverseTransformDirection(forward);
            if ((AimingState)aimingState == AimingState.ADS)
            {
                forward = m_ShotGunRorateBias * forward;
            }

            float randomAngle = UnityEngine.Random.Range(0f, 1f) * deviation * AngleUnit; //将分度转换为度
            Vector2 deviationAngle = Vector2.zero;

            if (leixing == 0)
            {
                //在单位圆上随机
                deviationAngle = OnUnitCircle(randomAngle); //wangXun
            }
            else
            {
                //在单位球上均匀随机，然后将球压平实现内密外疏
                deviationAngle = UnityEngine.Random.insideUnitSphere * randomAngle; //hongXian
            }

            // Calculate bullet velocity by spread.
            Vector2 spreadAngle = UnityEngine.Random.insideUnitCircle * spread * AngleUnit;        //将分度转换为度
            var spreadRotation = Quaternion.Euler(spreadAngle.x, spreadAngle.y, 0.0f) * Quaternion.Euler(deviationAngle.x, deviationAngle.y, 0.0f);

            // 利用散布和偏差对子弹的初始设计方向进行修正
            var bulletInitialVelocity = m_BulletRefTransform.rotation * spreadRotation * forward * m_InitialBulletSpeed;
            // Spawn bullet.
            if (spawnBulletAction != null)
            {
                spawnBulletAction(bulletInitialVelocity, muzzlePoint);
            }
        }

        private void ShotWithShotgun(Vector3 aimPosition, int aimingState, float deviation, float spread, float shotCount, SpawnBulletEventHandler spawnBulletAction)
        {
            var muzzlePoint = m_MuzzleTransform.position;
            var forward = (aimPosition - muzzlePoint).normalized;
            forward = m_BulletRefTransform.InverseTransformDirection(forward);
            if ((AimingState)aimingState == AimingState.ADS)
            {
                forward = m_ShotGunRorateBias * forward;
            }
            for (int i = 0; i < shotCount; ++i)
            {
                // Calculate bullet velocity by spread.
                Vector2 spreadAngle = UnityEngine.Random.insideUnitCircle * (spread + deviation ) * AngleUnit;        //将分度转换为度
                var spreadRotation = Quaternion.Euler(spreadAngle.x, spreadAngle.y, 0.0f);

                // 利用散布和偏差对子弹的初始设计方向进行修正
                var bulletInitialVelocity = m_BulletRefTransform.rotation * spreadRotation * forward * m_InitialBulletSpeed;
                // Spawn bullet.
                if (spawnBulletAction != null)
                {
                    spawnBulletAction(bulletInitialVelocity, muzzlePoint);
                }
            }
        }

        protected IEnumerator Recoil()
        {
            float time = Time.time;
            float recoilTotalTime = 0f;

            float nodTotalTime = m_TimeBetweenShots;
            nodTotalTime = Mathf.Min(nodTotalTime, vertClampRecoil.y * 2);

            float backwardTime = nodTotalTime * m_RecoverySpeedRecoil / (m_SpeedRecoil + m_RecoverySpeedRecoil);
            float gunUpTime = (1 + m_VertClampRecoil.x * UnityEngine.Random.Range(0f, 1f)) * m_VertClampRecoil.y * m_GunUpTimeFactor;
            float gunUpDegree = gunUpTime * m_VertSpeedRecoil;
            float goUpTotalTime = 0.0f;
            float goDownTimeRemainder = 0.0f;

            float horDirection = (UnityEngine.Random.Range(0f, 1f) + m_HorTendencyRecoil > 0.5f) ? 1f : -1f;
            float horDegreeClamp = horDirection < 0 ? m_LeftMaxRecoil : m_RightMaxRecoil;
            float gunHorDegree = Mathf.Abs(UnityEngine.Random.Range(0f, 1f) * horDegreeClamp);
            float horTotalDegree = 0.0f;
            do
            {
                float deltaTime = Time.deltaTime;
                Nod(deltaTime, recoilTotalTime, backwardTime, nodTotalTime);
                recoilTotalTime += deltaTime;

                float tempGoUpTotalTime = goUpTotalTime;
                goUpTotalTime += deltaTime;
                if(goUpTotalTime > gunUpTime)
                {
                    deltaTime = gunUpTime - tempGoUpTotalTime;
                    goDownTimeRemainder = goUpTotalTime - gunUpTime;
                }

                float upDegree = m_VertSpeedRecoil * deltaTime;

                float horDegree = m_HorSpeedRecoil * deltaTime;
                horTotalDegree += horDegree;
                horTotalDegree = Mathf.Clamp(horTotalDegree, 0.0f, gunHorDegree);
                horDegree = Mathf.Clamp(horDegree, 0.0f, gunHorDegree - horTotalDegree);
                if (m_OnRecoilRotate != null)
                {
                    m_OnRecoilRotate(-upDegree, horDegree * horDirection, 0f);
                }
                yield return null;

            } while (goUpTotalTime < gunUpTime);

            float gunDownDegree = gunUpDegree * m_VertRecoverySpeedRecoil * (m_VertRecoveryModifier +
                                    (1.0f - m_VertRecoveryModifier) * UnityEngine.Random.Range(0f, 1f));
            Vector2 totalDownDis = new Vector2(gunDownDegree, horTotalDegree);
            Vector2 speed = totalDownDis.normalized * m_VertRecoveryMaxRecoil;
            Vector2 preDownDis = speed * goDownTimeRemainder;
            preDownDis.x = Mathf.Clamp(preDownDis.x, 0, totalDownDis.x);
            preDownDis.y = Mathf.Clamp(preDownDis.y, 0, totalDownDis.y);
            totalDownDis -= preDownDis;
            if (m_OnRecoilRotate != null)
            {
                m_OnRecoilRotate(preDownDis.x, -preDownDis.y * horDirection, 0f);
            }
            float gunDownTime = totalDownDis.x / speed.x;
            while (gunDownTime > 0)
            {
                float deltaTime = Time.deltaTime;
                Nod(deltaTime, recoilTotalTime, backwardTime, nodTotalTime);
                recoilTotalTime += deltaTime;
                float tempGunDownTime = gunDownTime;
                gunDownTime -= deltaTime;
                if(gunDownTime < 0)
                {
                    deltaTime = tempGunDownTime;
                }
                Vector2 deltaDis = speed * deltaTime;
                if (m_OnRecoilRotate != null)
                {
                    m_OnRecoilRotate(deltaDis.x, -deltaDis.y * horDirection, 0f);
                }
                yield return null;
            }
            m_RecoilBackDistance = 0f;
            if (m_OnRecoilBack != null)
            {
                m_OnRecoilBack(m_RecoilBackDistance);
            }
        }

        /**
        * 平移.
        * @param dir 平移方向.
        * @param deltaTime 平移时间.
        * @param panningTotal 总平移距离.
        * @return 总平移距离
        */
        private float Panning(float dir, float deltaTime, float panningTotal)
        {
            if (panningTotal < m_LeftMaxRecoil || panningTotal > m_RightMaxRecoil)
            {
                return panningTotal;
            }

            float panning = m_HorSpeedRecoil * dir * deltaTime;
            float panningTempTotal = panningTotal + panning;
            panningTotal = Mathf.Clamp(panningTempTotal, m_LeftMaxRecoil, m_RightMaxRecoil);
            panning += panningTotal - panningTempTotal;

            if (m_OnRecoilRotate != null)
            {
                m_OnRecoilRotate(0f, panning, 0f);
            }
            return panningTotal;
        }

        private void Nod(float deltaTime, float time, float backwardTime, float nodTotalTime)
        {
            if (time > nodTotalTime)
            {
                return;
            }

            if (time < backwardTime)
            {
                float preDeltaTime = deltaTime;
                deltaTime = Mathf.Min(time + deltaTime, backwardTime) - time;

                float backward = -deltaTime * m_SpeedRecoil;
                m_RecoilBackDistance += backward;
                if (m_OnRecoilBack != null)
                {
                    m_OnRecoilBack(m_RecoilBackDistance);
                }

                if (Mathf.Abs(preDeltaTime - deltaTime) < Mathf.Epsilon)
                {
                    return;
                }

                time = backwardTime;
                deltaTime = preDeltaTime - deltaTime;
            }
            
            deltaTime = Mathf.Min(time + deltaTime, nodTotalTime) - time;

            float forward = deltaTime * m_RecoverySpeedRecoil;
            m_RecoilBackDistance += forward;
            if (m_OnRecoilBack != null)
            {
                m_OnRecoilBack(m_RecoilBackDistance);
            }
        }

        public Vector3 GetPitchYawOffset(PostureState postureState, float deltaTime)
        {
            float swayModifier = 1f;
            switch (postureState)
            {
                case PostureState.Stand:
                    swayModifier = m_MovementModifierSway;
                    break;
                case PostureState.Crouch:
                    swayModifier = m_CrouchModifierSway;
                    break;
                case PostureState.Prone:
                    swayModifier = m_ProneModifierSway;
                    break;
                default:
                    swayModifier = m_MovementModifierSway;
                    break;
            }

            m_PitchYawTime += deltaTime;
            float pitchOffset = m_PitchSway * swayModifier * SwayUnit;
            float yawOffset = m_YawOffsetSway * swayModifier * SwayUnit;
            return new Vector3(yawOffset * (m_YawCurve.Evaluate(m_PitchYawTime) - 0.5f), pitchOffset * m_PitchCurve.Evaluate(m_PitchYawTime), 0f);
        }

        private Vector2 OnUnitCircle(float radius)
        {
            Vector2 randomPoint = Vector2.zero;
            float radian = UnityEngine.Random.Range(0f, 1f) * Mathf.PI * 2.0f; ;
            randomPoint.x = radius * Mathf.Cos(radian);
            randomPoint.y = radius * Mathf.Sin(radian);
            return randomPoint;
        }

        private void OnDisable()
        {
            m_OnRecoilRotate = null;
            m_OnRecoilBack = null;
        }
    }
}