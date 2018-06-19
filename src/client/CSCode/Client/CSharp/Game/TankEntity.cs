using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using System.Collections;
using War.Game.Ani;
using War.Scene;

namespace War.Game
{
    /// <summary>
    /// 载具碰撞部件
    /// </summary>
    public enum TankHitPart
    {
        TankBody = 1,       //载具车体
        TankLFW,        //载具左前轮
        TankRFW,        //载具右前轮
        TankLBW,        //载具左后轮
        TankRBW,        //载具右后轮
    }

    /// <summary>
    /// 载具碰撞部件
    /// </summary>
    public class TankHitAction : MonoBehaviour
    {
        public TankHitPart hitPart;
    }

    
    /// <summary>
    /// 载具信息
    /// </summary>
    public class TankInfo: MonoBehaviour
    {
        /// <summary>
        /// 是否受自己监控
        /// </summary>
        public bool IsMontoring = false;

        /// <summary>
        /// 司机SID
        /// </summary>
        public double DriverSID = 0;

        /// <summary>
        /// 是否为静止的
        /// </summary>
        public bool IsStatic = true;

        /// <summary>
        /// 当前油门值
        /// </summary>
        public float Throttle = 0;

        /// <summary>
        /// 方向盘
        /// </summary>
        public float SteerAngle = 0;

        /// <summary>
        /// 当前状态
        /// </summary>
        public string State = "None";

        /// <summary>
        /// 是否启用物理模拟
        /// </summary>
        public bool IsEnablePhySimulation = false;

        /// <summary>
        /// 是否损坏
        /// </summary>
        public bool IsDestroyed = false;

        /// <summary>
        /// 是否熄火
        /// </summary>
        public bool IsFlameOut = false;

        /// <summary>
        /// 当前刹车力
        /// </summary>
        public float BrakeTorque = 0;

        /// <summary>
        /// 声音列表
        /// </summary>
        public string SoundList = "";

        /// <summary>
        /// 光效列表
        /// </summary>
        public string EffectList = "";

        /// <summary>
        /// 乘客列表
        /// </summary>
        public string PassengerSid = "";

    }

    /// <summary>
    /// 乘客信息，在乘客下车的时候要进行恢复
    /// </summary>
    class PassengerInfo
    {
        public int sid;
        public int nSeatIndex;
        public bool isEntityColliderTrigger;
        public bool isShadowColliderTrigger;
        public RigidbodyConstraints entityConstraints = RigidbodyConstraints.None;
        public RigidbodyConstraints shadowConstraints = RigidbodyConstraints.None;
        public Transform passenger;
        public Transform shadow;
    }

    /// <summary>
    /// 载具实体组件
    /// </summary>
    public class TankEntity : MonoBehaviour
    {
        /// <summary>
        /// 载具信息统计
        /// </summary>
        public delegate void OnTankEventHandler();
        public OnTankEventHandler onTankInfo = null;
        public OnTankEventHandler onInitComplete = null;

        /// <summary>
        /// 对象移动控制，该对象封装了地图坐标与世界3D坐标的转换
        /// </summary>
        protected Scene.ObjectToMove m_ObjectToMove;

        /// <summary>
        /// 角色逻辑sid
        /// </summary>
        public int sid = 0;

        /// <summary>
        /// 是否激活载具信息输出
        /// </summary>
        public bool isEnableTankInfo = false;

        /// <summary>
        /// 刚体对象
        /// </summary>
        private Rigidbody m_Rigidbody;

        /// <summary>
        /// 乘客信息列表
        /// </summary>
        private Dictionary<Transform, PassengerInfo> m_dicPassengerInfo;

        /// <summary>
        /// 绑定骨骼点
        /// </summary>
        private ArrayList m_ListBones = new ArrayList();

        /// <summary>
        /// 载具信息显示
        /// </summary>
        private TankInfo m_info = null;

        /// <summary>
        /// 是否为静止
        /// </summary>
        private bool m_bStatic = false;

        /// <summary>
        /// 临时变量
        /// </summary>
        static private CharacterEntity s_entity = null;
        static private ArrayList s_arrDelInfo = new ArrayList();
        static private bool s_bNeedsUnbind = false;

        void Awake()
        {
            //乘客列表
            m_dicPassengerInfo = new Dictionary<Transform, PassengerInfo>();

            //位置处理器
            m_ObjectToMove = GetComponent<Scene.ObjectToMove>();

            //刚体对象
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        void Start()
        {
        }

        private void Update()
        {
            if (m_dicPassengerInfo.Count == 0)
            {
                return;
            }

            s_entity = null;
            s_bNeedsUnbind = false;

            foreach (PassengerInfo info in m_dicPassengerInfo.Values)
            {
                s_bNeedsUnbind = false;

                //sid变了，说明该对象没有成功被放下载具
                s_entity = info.passenger.GetComponent<CharacterEntity>();
                if (null == s_entity || s_entity.sid != info.sid)
                {
                    s_bNeedsUnbind = true;
                }

                //发现不是该载具的乘客，要强制放下
                if(s_bNeedsUnbind)
                {
                    s_arrDelInfo.Add(info);
                    continue;
                }

                if (null != info.shadow)
                {
                    info.shadow.localRotation = Quaternion.identity;
                    info.shadow.localPosition = Vector3.zero;
                }
                if (null != info.passenger)
                {
                    info.passenger.localRotation = Quaternion.identity;
                    info.passenger.localPosition = Vector3.zero;
                }

                if ((info.passenger && info.passenger.parent == null) || (info.shadow && info.shadow.parent == null))
                {
                    s_entity = info.passenger.GetComponent<CharacterEntity>();

                    //再次进行绑定
                    PassengerEnter(info.passenger, info.shadow, info.nSeatIndex);

                    //打印错误Log
                    Debug.LogErrorFormat("发现载具{0}上的乘客{1}被无故拉下载具，再次进行绑定！", sid, s_entity.sid);
                }
            }

            //取消对象绑定
            if (s_arrDelInfo.Count > 0)
            {
                foreach (PassengerInfo info in s_arrDelInfo)
                {
                    UnbindObject(info.shadow, info, true);
                    UnbindObject(info.passenger, info, false);
                    m_dicPassengerInfo.Remove(info.passenger);

                    Debug.LogErrorFormat("发现载具{0}上的乘客{1}没有被正常下载具，主动放下！", sid, info.sid);
                }

                s_arrDelInfo.Clear();
            }

            //统计信息
            if (isEnableTankInfo)
            {
                if (null == m_info)
                {
                    m_info = gameObject.AddComponent<TankInfo>();
                }
            }

            if (null != onTankInfo && isEnableTankInfo)
            {
                onTankInfo();

                CarController carController = GetComponent<CarController>();
                if(carController != null)
                {
                    m_info.Throttle = carController.throttle;
                    m_info.IsStatic = m_bStatic;
                    m_info.IsEnablePhySimulation = carController.EnablePhySimulation;
                    m_info.IsFlameOut = carController.Flameout;
                    m_info.SteerAngle = carController.steerAngle;
                    m_info.BrakeTorque = carController.brakeTorque;
                }
                
            }
        }

        IEnumerator DelayInit(Vector3 pos, Vector3 rotate)
        {
            yield return new WaitForSeconds(0.2f);

            //设置坐标
            SetPosition(pos.x, pos.y, pos.z);

            //设置角度
            SetRotation(rotate.x, rotate.y, rotate.z);

            //在启用控制器
            enableController = true;

            //取消静止
            SetStatic(false);

            if (null != onInitComplete)
            {
                onInitComplete();
            }

        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(Vector3 pos, Vector3 rotate)
        {
            StartCoroutine(DelayInit(pos, rotate));
        }

        /// <summary>
        /// 设置静止状态
        /// </summary>
        /// <param name="bStatic"></param>
        public void SetStatic(bool bStatic)
        {
            m_bStatic = bStatic;

            if(m_Rigidbody != null)
            {
                if(bStatic)
                {
                    m_Rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
                }
                else
                {
                    m_Rigidbody.constraints = 0;
                }
            }

        }

        /// <summary>
        /// 获取当前的位置
        /// </summary>
        public Vector3 position
        {
            get
            {
                return m_ObjectToMove.GetPosition();
            }
        }

        /// <summary>
        /// 增加绑定骨骼点
        /// </summary>
        /// <param name="bindBone"></param>
        public void AddBindBone(string bindBone)
        {
            m_ListBones.Add(bindBone);
        }

        /// <summary>
        /// 判断指定的乘客是否绑定在车上
        /// </summary>
        /// <param name="passenger"></param>
        /// <param name="nSeatIndex"></param>
        /// <returns></returns>
        public bool IsPassengerBinded(Transform passenger, int nSeatIndex)
        {
            if (passenger.parent.parent != transform)
            {
                return false;
            }

            CharacterEntity entity = passenger.GetComponent<CharacterEntity>();
            if(null == entity)
            {
                return false;
            }

            foreach (PassengerInfo info in m_dicPassengerInfo.Values)
            {
                if (info.nSeatIndex == nSeatIndex)
                {
                    if(info.sid == entity.sid)
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 玩家进入载具
        /// </summary>
        /// <param name="passenger"></param>
        /// <param name="seadIndex"></param>
        public bool PassengerEnter(Transform passenger, Transform shadow, int seatIndex)
        {
            if(seatIndex < 0 || seatIndex >= m_ListBones.Count)
            {
                Debug.LogError("进入载具失败，载具的座位不够！seatIndex=" + seatIndex);
                return false;
            }

            //获取角色实体组件
            CharacterEntity charEntity = passenger.GetComponent<CharacterEntity>();
            if (charEntity == null)
            {
                return false;
            }

            PassengerInfo info = null;
            bool infoExistAreay = false;
            if(m_dicPassengerInfo.ContainsKey(passenger))
            {
                info = m_dicPassengerInfo[passenger];
                infoExistAreay = true;
            }
            else
            {
                info = new PassengerInfo();
                m_dicPassengerInfo.Add(passenger, info);
            }
            info.sid = charEntity.sid;
            info.nSeatIndex = seatIndex;

            BindObject(passenger, seatIndex, info, false, infoExistAreay);
            if(null != shadow)
            {
                BindObject(shadow, seatIndex, info, true, infoExistAreay);
            }

            //设置处于载具状态
            charEntity.SetAniState(Ani.EnAniState.Tank);

            return true;

        }

        /// <summary>
        /// 乘客退出
        /// </summary>
        /// <param name="passenger"></param>
        public bool PassengerExit(Transform passenger, Transform shadow)
        {
            //离开载具状态
            CharacterEntity charEntity = passenger.GetComponent<CharacterEntity>();
            if (charEntity == null)
            {
                return false;
            }

            //判断是否存在
            if(!m_dicPassengerInfo.ContainsKey(passenger))
            {
                //Debug.LogError("尝试让不在载具上的人下车！ uidPaassenger=" + charEntity.sid);
                return false;
            }

            //取消对象绑定
            PassengerInfo info = m_dicPassengerInfo[passenger];
            UnbindObject(shadow, info, true);
            UnbindObject(passenger, info, false);

            m_dicPassengerInfo.Remove(passenger);

            //退出载具动作状态
            charEntity.SetAniState(Ani.EnAniState.None);

            return true;

        }

        /// <summary>
        /// 设置旋转角度（欧拉角）
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetRotation(float x,float y, float z)
        {
            transform.eulerAngles = new Vector3(x, y, z);
        }

        /// <summary>
        /// 获取载具刚体的速度向量
        /// </summary>
        /// <returns></returns>
        public Vector3 GetVelocity()
        {
            return m_Rigidbody.velocity;
        }

        /// <summary>
        /// 判断某个位置是否可以下车
        /// </summary>
        /// <returns></returns>
        public bool CanGetDown(Vector3 pos)
        {
            Vector3 origin = transform.position;
            origin.y += 2.5f;

            pos = StreamerManager.GetTilePosition(pos.x, pos.y, pos.z);

            Vector3 drection = pos - origin;

            RaycastHit hit;
            if (Physics.Raycast(origin, drection.normalized, out hit, 20.0f, LayerConfig.WalkMask))
            {
               if(hit.distance - drection.magnitude  > 0.3f)
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        /// <summary>
        /// 设置位置
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void SetPosition(float x, float y, float z)
        {
            m_ObjectToMove.SetPosition(x, y, z);
        }

        /// <summary>
        /// 取消对象绑定
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="info"></param>
        /// <param name="bShadow"></param>
        private void UnbindObject(Transform trans, PassengerInfo info, bool bShadow)
        {
            if (null == trans)
            {
                return;
            }

            //获取角色实体组件
            CharacterEntity charEntity = trans.GetComponent<CharacterEntity>();
            if(null != charEntity)
            {
                charEntity.SetParent(null, false, "Tank");
            }
            else
            {
                trans.SetParent(null);
            }

            //恢复刚体的设置
            Rigidbody rigidbody = trans.GetComponent<Rigidbody>();
            if(bShadow)
            {
                rigidbody.constraints = info.shadowConstraints;
            }
            else
            {
                rigidbody.constraints = info.entityConstraints;
            }
            rigidbody.useGravity = true;

            //恢复角度
            trans.rotation = Quaternion.identity;

            //恢复碰撞体
            CapsuleCollider c = trans.GetComponent<CapsuleCollider>();
            if (c != null)
            {
                if (bShadow)
                {
                    c.isTrigger = info.isShadowColliderTrigger;
                }
                else
                {
                    c.isTrigger = info.isEntityColliderTrigger;
                }
            }

        }

        /// <summary>
        ///  绑定对象
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="seatIndex"></param>
        /// <param name="info"></param>
        /// <param name="bShadow"></param>
        /// <param name="bInfoExistAreay"></param>
        private void BindObject(Transform trans, int seatIndex, PassengerInfo info, bool bShadow, bool bInfoExistAreay)
        {
            if(null == trans)
            {
                return;
            }

            //设置刚体
            Rigidbody rigidbody = trans.GetComponent<Rigidbody>();

            //保存刚体状态，后面用来恢复
            if(bShadow)
            {
                info.shadow = trans;
                if (!bInfoExistAreay)
                {
                    info.shadowConstraints = rigidbody.constraints;
                }
            }
            else
            {
                info.passenger = trans;
                if (!bInfoExistAreay)
                {
                    info.entityConstraints = rigidbody.constraints;
                }
            }

            //取消乘客的刚体动力学特性，使其可以进入载具
            rigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            rigidbody.useGravity = false;

            //修改碰撞提
            CapsuleCollider c = trans.GetComponent<CapsuleCollider>();
            if (c != null)
            {
                //保存碰撞触发设置
                if (!bInfoExistAreay)
                {
                    if (bShadow)
                    {
                        info.isShadowColliderTrigger = c.isTrigger;
                    }
                    else
                    {
                        info.isEntityColliderTrigger = c.isTrigger;
                    }
                }
                c.isTrigger = true;
            }

            //获取角色实体组件
            CharacterEntity charEntity = trans.GetComponent<CharacterEntity>();

            //绑定到载具上
            string bindBoneName = m_ListBones[seatIndex] as string;
            Transform bone = transform.Find(bindBoneName);
            if (bone == null)
            {
                Debug.LogError("PassengerEnter >> 没有找到对应的骨骼，直接绑定到0点！seatIndex=" + seatIndex + "; bone=" + bindBoneName);
                if(charEntity != null)
                {
                    charEntity.SetParent(transform, false, "Tank");
                }
                else
                {
                    trans.SetParent(transform, false);
                }
               
            }
            else
            {
                if (charEntity != null)
                {
                    charEntity.SetParent(bone, false, "Tank");
                }
                else
                {
                    trans.SetParent(bone, false);
                }
            }

            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;

        }

        private void FixedUpdate()
        {
            if(m_dicPassengerInfo.Count == 0)
            {
                return;
            }

            foreach (PassengerInfo info in m_dicPassengerInfo.Values)
            {
                if (null != info.shadow)
                {
                    ShadowRigid rigid = info.shadow.GetComponent<ShadowRigid>();
                    if (rigid != null)
                    {
                        rigid.SetVelocity(0, 0, 0);
                    }
                }
                if (null != info.passenger)
                {
                    CharacterEntity charEntity = info.passenger.GetComponent<CharacterEntity>();
                    if (charEntity != null)
                    {
                        charEntity.SetVelocity(0, 0, 0);
                    }
                }
            }
        }

        private void OnEnable()
        {
            foreach (PassengerInfo info in m_dicPassengerInfo.Values)
            {
                //Debug.LogError("销毁载具时，发现没有被放下载具的对象，sid=" + info.sid);
                if (null != info.shadow)
                {
                    info.shadow.transform.SetParent(null);
                }
                if (null != info.passenger)
                {
                    info.passenger.transform.SetParent(null);
                }
            }

            m_dicPassengerInfo.Clear();
            isEnableTankInfo = false;

            ///禁用控制器
            enableController = false;

        }

        private void OnDisable()
        {
            ///设置载具的Y位置，防止载具出生时候在水里，直接熄火
            transform.position = new Vector3(0, 8, 0);

            ///清除回调
            onTankInfo = null;

            ///取消静止状态
            if (m_bStatic)
            {
                SetStatic(false);
            }

            ///初始化完成回调
            onInitComplete = null;

            ///禁用控制器
            enableController = false;
        }

        /// <summary>
        /// 启用或者禁用控制器
        /// </summary>
        private bool enableController
        {
            set
            {
                if(value)
                {
                    CarController carController = GetComponent<CarController>();
                    if (carController != null)
                    {
                        carController.enabled = true;
                    }

                    MotorboatController boatController = GetComponent<MotorboatController>();
                    if (boatController != null)
                    {
                        boatController.enabled = true;
                    }
                }
                else
                {
                    CarController carController = GetComponent<CarController>();
                    if (carController != null)
                    {
                        carController.enabled = false;
                    }

                    MotorboatController boatController = GetComponent<MotorboatController>();
                    if (boatController != null)
                    {
                        boatController.enabled = false;
                    }
                }
            }
        }

    }
}
