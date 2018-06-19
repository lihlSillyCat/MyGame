using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using War.Scene;

namespace War.Game
{
    public class Parachute : MonoBehaviour
    {
        public JoystickAdaptor JoystickAdaptor = null;

        [System.NonSerialized]
        public  float TerrainAltitude; // 当前点的地面海拔
        protected float m_lastAltitude;  // 上帧玩家飞行高度
        protected float m_ImpactDistance; // 当前点的障碍物碰撞面高度

        protected Rigidbody m_Rigidbody = null;
        protected Transform m_Transform;
        protected Animator m_Animator = null;
        protected Transform m_CurTransform;

        protected GameObject m_ChuteGameObject = null;
        protected Animator m_ChuteAnimator = null;

        public float CanDeployHeight = 4800;  // 可开伞高度
        public float ForceDeployHeight = 300;  // 强制开伞高度
        public float BellyToEarthDrag = 0.19f;  // 未开伞 匍匐自由落体 空气阻力
        public float BreakDrag = 0.22f;         // 未开伞 后拽的阻力 摇杆按下
        public float HeadDownDrag = 0.16f;      // 未开伞 倒栽葱自由落体 空气阻力 摇杆按上
        public float AirborneDrag = 0.65f;      // 开伞后 自由落体  空气阻力
        public float AirborneAccDrag = 0.54f;    // 开伞后 下拽下落 空气阻力 摇杆按上
        public float AirborneDecDrag = 0.7f;     // 开伞后  后拽下落 空气阻力 摇杆按下
        public float LandingDrag = 3;            //  落地过程 强制空气阻力 
        public float StartHeight = 5000;         // 飞机航线高度
        public float LandingHeight = 30;          // 落地高度
        public float MaxHorizonAccelerate = 10;   // 水平方向前后左右最大加速度 摇杆控制效果
        public float SkydiveMaxHorizonSpeed = 30;  // 未开伞 水平滑翔速度
        public float AirborneMaxHorizonSpeed = 30; // 开伞后 水平滑翔速度
        public float ParachuteCoefficient = 0.5f;  // 是开伞一瞬间，速度衰减的系数。开伞后的速度 = 开伞前的速度　*　这个值

        public float SkydiveDragCameraClamp = 1.0f;
        public float SkydiveSpeedCameraClamp = 1.0f;

        public  Vector3 Velocity;
        protected float m_Drag;      
        public bool Cuted = false;

        protected bool m_FreeLook;
        public bool freeLook
        {
            set
            {
                m_FreeLook = value;
            }
        }

        private int m_PackLocalMotionHash = 0;

        public enum ChuteState
        {
            Skydive,
            Deployed,
            Landing,
            Landed,
        }

        public ChuteState State;

        protected ObjectToMove m_ObjectToMove;
        protected ShadowRigid m_ShadowRigid;        

        protected void OnAwake()
        {
            m_Transform = GetComponent<Transform>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Animator = GetComponent<Animator>();
            m_ObjectToMove = GetComponent<ObjectToMove>();
            m_lastAltitude = float.MaxValue;
            m_CurTransform = m_Transform;
            m_ImpactDistance = float.MaxValue;
            JoystickAdaptor = new JoystickAdaptor(1);
            m_PackLocalMotionHash = Animator.StringToHash("Base Layer.Pack_Locomotion");
        }

        protected void CheckTerrainHeight(float distance)
        {
            RaycastHit hit;
            if (Physics.Raycast(m_Transform.position + new Vector3(0, 10, 0), -Vector3.up, out hit, distance, LayerConfig.WalkMask))
            {
                TerrainAltitude = hit.point.y;
                if (TerrainAltitude < 9)
                    TerrainAltitude = 9;
            }
        }

        protected bool CanDeployChute()
        {
            return m_Transform.position.y < CanDeployHeight;
        }

        protected bool CanForceDeployChute()
        {
            return m_Transform.position.y < ForceDeployHeight + TerrainAltitude;
        }

        protected bool CanLanding()
        {
            return m_lastAltitude > LandingHeight + TerrainAltitude && m_Transform.position.y <= LandingHeight + TerrainAltitude;
        }

        protected bool CanLaned()
        {
            return m_lastAltitude > 3 + TerrainAltitude && m_Transform.position.y <= 3 + TerrainAltitude;
        }

        public void TurnChute(float x)
        {
            if (m_ChuteGameObject != null)
            {
                m_ChuteGameObject.transform.Rotate(Vector3.up, x);
            }
        }

        public void Jump()
        {
            State = ChuteState.Skydive;
            m_Drag = BellyToEarthDrag;
            m_Animator.SetTrigger("Plane");
        }

        protected void UpdateMove()
        {
            switch (State)
            {
                case ChuteState.Skydive:
                    m_CurTransform = transform;
                    if (m_FreeLook == false)
                    {
                        Vector3 f = Camera.main.transform.forward;
                        Vector3 h = new Vector3(f.x, 0, f.z).normalized;

                        SkydiveDragCameraClamp = Mathf.Lerp(SkydiveDragCameraClamp, Mathf.Clamp01(Vector3.Dot(f, -Vector3.up)),0.07f);
                        SkydiveSpeedCameraClamp = Vector3.Dot(f, new Vector3(f.x, 0, f.z).normalized);
                    }

                    if (JoystickAdaptor.CurrentY < 0)
                        m_Drag = BellyToEarthDrag + (BreakDrag - BellyToEarthDrag) * -JoystickAdaptor.CurrentY * SkydiveDragCameraClamp;
                    else
                        m_Drag = BellyToEarthDrag + (HeadDownDrag - BellyToEarthDrag) * JoystickAdaptor.CurrentY * SkydiveDragCameraClamp;

                    if (JoystickAdaptor.IsPressed)
                    {
                        Vector2 deltaV = JoystickAdaptor.TargetAxis * Time.fixedDeltaTime * MaxHorizonAccelerate * SkydiveSpeedCameraClamp;

                        // can't move backward, but can slow down
                        Vector3 localVelocity = transform.InverseTransformDirection(Velocity);
                        localVelocity += new Vector3(deltaV.x, 0, deltaV.y);

                        if (localVelocity.z < 0)
                            localVelocity.z = 0;

                        Vector3 veloctiy =transform.TransformDirection(localVelocity);
                        veloctiy.x = Mathf.Clamp(veloctiy.x, -SkydiveMaxHorizonSpeed * SkydiveSpeedCameraClamp, SkydiveMaxHorizonSpeed * SkydiveSpeedCameraClamp);
                        veloctiy.z = Mathf.Clamp(veloctiy.z, -SkydiveMaxHorizonSpeed * SkydiveSpeedCameraClamp, SkydiveMaxHorizonSpeed * SkydiveSpeedCameraClamp);
                        Velocity = veloctiy;
                    }
                    else
                    {
                        SlowDown();
                    }

                    DoMove(m_CurTransform);
                    break;

                case ChuteState.Deployed:
                    m_CurTransform = m_ChuteGameObject.transform;

                    if (JoystickAdaptor.CurrentY < 0)
                        m_Drag = AirborneDrag + (AirborneDecDrag - AirborneDrag) * -JoystickAdaptor.CurrentY;
                    else
                        m_Drag = AirborneDrag + (AirborneAccDrag - AirborneDrag) * JoystickAdaptor.CurrentY;

                    if (JoystickAdaptor.IsPressed)
                    {
                        float deltaV = JoystickAdaptor.TargetY * Time.fixedDeltaTime * MaxHorizonAccelerate;

                        Vector3 localVelocity = m_ChuteGameObject.transform.InverseTransformDirection(Velocity);
                        localVelocity.z += deltaV;

                        if (localVelocity.z < 0)
                            localVelocity.z = 0;
                        Vector3 veloctiy = m_ChuteGameObject.transform.TransformDirection(localVelocity);

                        veloctiy.x = Mathf.Clamp(veloctiy.x, -AirborneMaxHorizonSpeed, AirborneMaxHorizonSpeed);
                        veloctiy.z = Mathf.Clamp(veloctiy.z, -AirborneMaxHorizonSpeed, AirborneMaxHorizonSpeed);

                        Velocity = veloctiy;
                    }
                    else
                    {
                        SlowDown();
                    }
                    DoMove(m_CurTransform);
                    break;
                case ChuteState.Landing:
                    m_CurTransform = m_ChuteGameObject.transform;
                    DoMove(m_CurTransform);
                    break;
            }
        }

        protected void CheckFowardBlock()
        {
            Vector3 nV = Velocity.normalized;
            RaycastHit hit;
            if (Physics.Raycast(m_CurTransform.position, nV, out hit, 100, LayerConfig.ParachuteMask))
            {
                m_ImpactDistance = hit.distance;
            }
            else
                m_ImpactDistance = float.MaxValue;
        }

        private void DoMove(Transform t)
        {
            float delta = Time.fixedDeltaTime;
            Velocity = Velocity * (1 - delta * m_Drag);
            Velocity = Velocity + Physics.gravity * delta;
            Vector3 nV = Velocity.normalized;

            float movedDis = (Velocity * delta).magnitude;

            if (movedDis >= m_ImpactDistance + LandingHeight)
            {
                PrepareLand();
            }

            if (movedDis >= m_ImpactDistance)
            {
                movedDis = m_ImpactDistance;
                if (State != ChuteState.Landing)
                    PrepareLand();
                if (State != ChuteState.Landed)
                    Landed();
            }

            t.position = t.position + nV * movedDis;
        }

        private void SlowDown()
        {
            Vector3 velocity = Velocity;
            float vSpeed = velocity.y;
            velocity.y = 0;

            float speed = velocity.magnitude;
            float newSpeed = speed - MaxHorizonAccelerate * Time.fixedDeltaTime;
            if (newSpeed < 0)
                newSpeed = 0;
            velocity = velocity.normalized * newSpeed;

            Velocity = new Vector3(velocity.x, vSpeed, velocity.z);
        }

        protected float SkyDirveAnimatorControl()
        {
            float forward = JoystickAdaptor.CurrentY > 0 ? JoystickAdaptor.CurrentY : JoystickAdaptor.CurrentY * 0.5f;
            float sideslip = JoystickAdaptor.CurrentX;
            forward = forward * SkydiveDragCameraClamp * 2;
            m_Animator.SetFloat("Forward", forward);
            m_Animator.SetFloat("Sideslip", sideslip);
            return forward;
        }

        protected void DeployedAnimatorControl()
        {
            if (ChuteCanRotate() == false)
                JoystickAdaptor.CurrentAxis = Vector2.zero;

            m_ChuteAnimator.SetFloat("Forward", JoystickAdaptor.CurrentY);
            m_ChuteAnimator.SetFloat("Sideslip", JoystickAdaptor.CurrentX);
            m_Animator.SetFloat("Forward", JoystickAdaptor.CurrentY);
            m_Animator.SetFloat("Sideslip", JoystickAdaptor.CurrentX);
        }       

        public bool ChuteCanRotate()
        {
            return State == ChuteState.Deployed && m_ChuteAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == m_PackLocalMotionHash;
        }

        public void DeployChute(GameObject chuteGameObject)
        {
            if (State == ChuteState.Deployed)
                return;

            JoystickAdaptor.Delta = 1.5f;

            m_ChuteGameObject = chuteGameObject;
            m_ChuteAnimator = chuteGameObject.GetComponent<Animator>();

            Transform pack_root = chuteGameObject.transform.Find("Pack_Root");
            Transform pack_attach = chuteGameObject.transform.Find("Pack_Root/Pack_Attach");
            Transform chest = transform.Find("Main/Root_M/BackA_M/BackB_M/Chest_M");

            chuteGameObject.transform.parent = null;
            chuteGameObject.transform.position = transform.position;
            chuteGameObject.transform.rotation = transform.rotation;
            chuteGameObject.AddComponent<ObjectToMove>();

            pack_attach.SetParent(chest, true);
            transform.SetParent(pack_root, true);
            transform.localPosition = new Vector3(0, 0, -4.6f);

            pack_attach.transform.localPosition = Vector3.zero;
            pack_attach.transform.localRotation = Quaternion.Euler(-5.231f, -90, -90);
#if UNITY_EDITOR
            Debug.LogWarning("DeployChute begin");
#endif
            State = ChuteState.Deployed;
            m_Rigidbody.useGravity = true;
            m_Drag = AirborneDrag;
            Velocity.x = Velocity.z = 0;
            Velocity.y *= ParachuteCoefficient;

            m_Rigidbody.velocity = Vector3.zero;

            m_Animator.SetTrigger("Parachute");
            m_Animator.SetFloat("Forward", 0);
            m_Animator.SetFloat("Sideslip", 0);
            m_ChuteAnimator.SetTrigger("Pack_Parachute");
        }

        public void PrepareLand()
        {
            if (State == ChuteState.Landing)
                return;

            State = ChuteState.Landing;
            m_Animator.SetTrigger("Landing");
            // 断线重连有可能没执行到DeployChute函数，m_ChuteAnimator为空
            if (m_ChuteAnimator != null)
                m_ChuteAnimator.SetTrigger("Landing");
            m_Drag = LandingDrag;         
        }

        public void CutChute()
        {
            if (Cuted)
                return;

            Cuted = true;
            Transform pack_root = m_ChuteGameObject.transform.Find("Pack_Root");
            Transform pack_attach = transform.Find("Main/Root_M/BackA_M/BackB_M/Chest_M/Pack_Attach");
            if (pack_attach != null)
                pack_attach.SetParent(pack_root, true);

            transform.rotation = m_ChuteGameObject.transform.rotation;
            gameObject.transform.SetParent(null, true);    

            m_ChuteGameObject.AddComponent<Rigidbody>();
            m_Rigidbody.useGravity = true;
        }

        public void Landed()
        {
            if (State == ChuteState.Landed)
                return;

            if (State != ChuteState.Landing)
                PrepareLand();

            CutChute();
#if UNITY_EDITOR
            Debug.LogWarning("Landed");
#endif
            State = ChuteState.Landed;
            m_Rigidbody.useGravity = true;
            m_Rigidbody.velocity = Vector3.zero;
            m_Animator.SetTrigger("Landed");
            m_Drag = 0;                   
        }

        public void Abort()
        {
            // joystick
            this.JoystickAdaptor.CurrentAxis = Vector2.zero;
            this.JoystickAdaptor.TargetAxis = Vector2.zero;

            // rigid body
            m_Rigidbody.useGravity = true;
            m_Rigidbody.velocity = Vector3.zero;


            if (StreamerManager.Instance)
                StreamerManager.Instance.SetPlayer(transform);

            // self phyics
            Velocity = Vector3.zero;
            m_Drag = 0;

            // animator
            m_Animator.SetFloat("Forward", 0);
            m_Animator.SetFloat("Sideslip", 0);

            if (m_ChuteAnimator != null)
            {
                m_ChuteAnimator.SetFloat("Forward", 0);
                m_ChuteAnimator.SetFloat("Sideslip", 0);
            }

            switch (State)
            {
                case ChuteState.Skydive:
                    m_Animator.SetTrigger("Parachute");
                    m_Animator.SetTrigger("Landing");
                    m_Animator.SetTrigger("Landed");
                    break;

                case ChuteState.Deployed:
                    m_Animator.SetTrigger("Landing");
                    m_Animator.SetTrigger("Landed");

                    if (m_ChuteAnimator != null)
                    {
                        m_ChuteAnimator.SetTrigger("Landing");
                        Transform pack_root = m_ChuteGameObject.transform.Find("Pack_Root");
                        Transform pack_attach = transform.Find("Main/Root_M/BackA_M/BackB_M/Chest_M/Pack_Attach");
                        pack_attach.SetParent(pack_root, true);
                        transform.rotation = m_ChuteGameObject.transform.rotation;
                    }

                    gameObject.transform.SetParent(null, true);
                    break;
                case ChuteState.Landing:
                    m_Animator.SetTrigger("Landed");
                    break;

                case ChuteState.Landed:
                    break;
            }
        }     
    }

    public class JoystickAdaptor
    {
        public float CurrentX = 0;
        public float CurrentY = 0;
        public float TargetX = 0;
        public float TargetY = 0;
        public float Delta;
        public bool IsPressed = false;

        public JoystickAdaptor(float delta)
        {
            Delta = delta;
        }

        public Vector2 TargetAxis
        {
            get { return new Vector2(TargetX, TargetY); }
            set
            {
                TargetX = value.x;
                TargetY = value.y;
            }
        }

        public Vector2 CurrentAxis
        {
            get { return new Vector2(CurrentX, CurrentY); }
            set
            {
                CurrentX = value.x;
                CurrentY = value.y;
            }
        }

        public void Update()
        {
            CurrentX = UpdateValue(CurrentX, TargetX, Delta);
            CurrentY = UpdateValue(CurrentY, TargetY, Delta);
        }

        private float UpdateValue(float current, float target, float delta)
        {
            if (target > current)
            {
                float t = current + delta * Time.deltaTime;
                if (t > target)
                    t = target;

                return t;
            }
            else if (target < current)
            {
                float t = current - delta * Time.deltaTime;
                if (t < target)
                    t = target;

                return t;
            }
            else
                return current;
        }
    }
}