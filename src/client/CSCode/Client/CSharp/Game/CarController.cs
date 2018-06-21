using UnityEngine;
using System.Collections;
using System;

public enum Wheels
{
    LeftFront,
    RightFront,
    LeftBack,
    RightBack
};

public enum EngineDrivetrain
{  
    FF,
    FR,
    FourWD,
};


public struct WheelData
{
    public WheelCollider wc;
    public float breaked;
    public float motorTorque;
    public GameObject tire;
};

public struct VehiclesSyncData
{
    public Vector3 position;
    public short rotateX;
    public short rotateY;
    public short rotateZ;

    public short rpm;
    public short steerAngle;
    public byte  wheelBreak;

    //public uint rpm_steerAngle_wheelBreak;
}

public delegate string CarCollisionProcess(Collision collision);

public class CarController : MonoBehaviour {

    [SerializeField]
    public bool enableTest = false;

    public float steerAngle = 0;
    [SerializeField]
    private float maxEngineOut = 1000;
    [SerializeField]
    private EngineDrivetrain drivetrain = EngineDrivetrain.FR;

    private bool isFlameout = false;

    public float maxSteerAngle = 30.0f;
    public float throttle = 0.0f;
    public float brakeTorque = 0;
    public float maxBrakeTorque = 1500.0f;

    public float flameoutLevel = 1.5f;

    public float rpm = 0;
    public float speed = 0;

    public float RimRadius = 0.5f;

    [SerializeField]
    public Vector3 massCenter;// = new Vector3(0, -0.12f, 0);

    [SerializeField]
    private bool enablePhySimulation = true;

    [SerializeField]
    public Texture2D destroyedTexture;

    private Texture2D replaceTexture = null;

    public CarCollisionProcess collisionCallBack = null;

    /// <summary>
    /// 碰撞退出时的回调，上层应用会用到这个事件来做一些逻辑处理，比如撞人的时候，
    /// 在这个时机主动的同步一次人物的数据
    /// </summary>
    public CarCollisionProcess collisionExitCallBack = null;

    WheelData[] wheels = new WheelData[4];

    private VehiclesSyncData syncData;

    Transform tran;

    float resistance = 0;
    Vector3 moveDir = Vector3.zero;

    int waterLayerInt;
    public float minBrakeTorque = 0;

    bool destroyed = false;
    private Texture2D oriTexture = null;

    public bool useInterplationSync = true;

    public float MaxEngineOut
    {
        get
        {
            return maxEngineOut;
        }

        set
        {
            maxEngineOut = value;
            applyDrivetrain();
        }
    }
    public EngineDrivetrain Drivetrain
    {
        get
        {
            return drivetrain;
        }

        set
        {
            drivetrain = value;
            applyDrivetrain();
        }
    }

    public bool Flameout
    {
        get
        {
            return isFlameout;
        }

        set
        {
            isFlameout = value;
        }
    }

    public bool EnablePhySimulation {
        get
        {
            return enablePhySimulation;
        }

        set
        {
            enablePhySimulation = value;
            if (enablePhySimulation == false)
                stopPhySimulation();
            else
                startPhySimulation();
        }
    }

    public Texture2D ReplaceTexture
    {
        get
        {
            return replaceTexture;
        }

        set
        {
            replaceTexture = value;
            applyTexture(replaceTexture, false);
        }
    }

    void applyTexture(Texture2D tex, bool saveToOriTex)
    {
        if(tran == null || tex == null)
            return;

        Transform ct = tran.Find("CheTi");
        if (ct != null)
        {
            int count = ct.childCount;
            for (int i = 0; i < count; ++i)
            {
                MeshRenderer crender = ct.GetChild(i).GetComponent<MeshRenderer>();
                if (crender != null)
                {
                    if (saveToOriTex && (oriTexture == null))
                        oriTexture = crender.material.mainTexture as Texture2D;
                    crender.material.mainTexture = tex;
                }
            }
        }
    }

    public void destroyCar(bool addExplosion)
    {
        destroyed = true;
        Flameout = true;

        applyTexture(destroyedTexture, true);
        
        minBrakeTorque = 300.0f;

        if (addExplosion == true)
        {
            float expForce = UnityEngine.Random.Range(210000.0f, 350000.0f);
            float offsetX = UnityEngine.Random.Range(-0.2f, 0.25f);
            float offsetZ = UnityEngine.Random.Range(0.1f, 0.3f);
            GetComponent<Rigidbody>().AddExplosionForce(expForce, tran.position + (new Vector3(offsetX, 0, offsetZ)), 30.0f);
        }
    }

    public void restoreCar()
    {
        destroyed = false;
        Flameout = false;

        if(oriTexture != null)
        {
            Transform ct = tran.Find("CheTi");
            if (ct != null)
            {
                int count = ct.childCount;
                for (int i = 0; i < count; ++i)
                {
                    MeshRenderer crender = ct.GetChild(i).GetComponent<MeshRenderer>();
                    if (crender != null && oriTexture != null)
                        crender.material.mainTexture = oriTexture;
                }
            }
        }

        minBrakeTorque = 0;

        restoreWheel(Wheels.LeftBack);
        restoreWheel(Wheels.RightBack);
        restoreWheel(Wheels.LeftFront);
        restoreWheel(Wheels.RightFront);
    }

    private void startPhySimulation()
    {
        foreach (WheelData wheel in wheels)
        {
            if(wheel.wc != null)
                wheel.wc.enabled = true;
        }

        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        //gameObject.GetComponent<Rigidbody>().WakeUp();
    }

    private void stopPhySimulation()
    {
        foreach(WheelData wheel in wheels)
        {
            if (wheel.wc != null)
                wheel.wc.enabled = false;
        }

        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        //gameObject.GetComponent<Rigidbody>().Sleep();
    }

    private void OnDisable()
    {
        isFlameout = false;
        maxSteerAngle = 30.0f;
        throttle = 0.0f;
        brakeTorque = 0;
        maxBrakeTorque = 1500.0f;
        flameoutLevel = 1.5f;
        rpm = 0;
        speed = 0;
        minBrakeTorque = 0;

        resistance = 0;

        moveDir.x = 0;
        moveDir.y = 0;
        moveDir.z = 0;

        syncData.position.x = 0;
        syncData.position.y = 0;
        syncData.position.z = 0;
        syncData.rotateX = 0;
        syncData.rotateY = 0;
        syncData.rotateZ = 0;

        syncDataSeted = false;

        restoreCar();

        collisionCallBack = null;
        collisionExitCallBack = null;
}

    string luntai = "LunTai";

// Use this for initializations
void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        lastPosition = War.Scene.StreamerManager.GetRealPosition(transform.position);

        rb.centerOfMass = massCenter;

        //设置轮子
        Transform lfw = gameObject.GetComponent<Transform>().Find("lfw");
        WheelCollider lfwc = lfw.GetComponent<WheelCollider>();
        wheels[0].wc = lfwc;

        Transform taiTran = lfw.GetChild(0).Find(luntai);
        if (taiTran != null)
            wheels[0].tire = taiTran.gameObject;
        else
            wheels[0].tire = null;

        Transform rfw = gameObject.GetComponent<Transform>().Find("rfw");
        WheelCollider rfwc = rfw.GetComponent<WheelCollider>();
        wheels[1].wc = rfwc;

        taiTran = rfw.GetChild(0).Find(luntai);
        if (taiTran != null)
            wheels[1].tire = taiTran.gameObject;
        else
            wheels[1].tire = null;


        Transform lbw_wheel = gameObject.GetComponent<Transform>().Find(wheel_lb);
        WheelCollider lbwc = lbw_wheel.GetComponent<WheelCollider>();
        wheels[2].wc = lbwc;

        taiTran = lbw_wheel.GetChild(0).Find(luntai);
        if (taiTran != null)
            wheels[2].tire = taiTran.gameObject;
        else
            wheels[2].tire = null;
            

        Transform rbw_wheel = gameObject.GetComponent<Transform>().Find(wheel_rb);
        WheelCollider rbwc = rbw_wheel.GetComponent<WheelCollider>();
        wheels[3].wc = rbwc;

        taiTran = rbw_wheel.GetChild(0).Find(luntai);
        if (taiTran != null)
            wheels[3].tire = taiTran.gameObject;
        else
            wheels[3].tire = null;

        wheels[0].breaked = wheels[1].breaked = wheels[2].breaked = wheels[3].breaked = 1.0f;
        applyDrivetrain();

        tran = GetComponent<Transform>();

        if(replaceTexture != null)
            applyTexture(replaceTexture, false);

        waterLayerInt = LayerMask.NameToLayer("Water");
    }

    private void applyDrivetrain()
    {
        switch (drivetrain)
        {
            case EngineDrivetrain.FR:
                {
                    wheels[0].motorTorque = wheels[1].motorTorque = 0.0f;
                    wheels[2].motorTorque = wheels[3].motorTorque = maxEngineOut * 0.5f;
                    break;
                }
            case EngineDrivetrain.FourWD:
                {
                    wheels[0].motorTorque = wheels[1].motorTorque = maxEngineOut * 0.25f;
                    wheels[2].motorTorque = wheels[3].motorTorque = maxEngineOut * 0.25f;
                    break;
                }
            default:
                {
                    wheels[0].motorTorque = wheels[1].motorTorque = maxEngineOut * 0.5f;
                    wheels[2].motorTorque = wheels[3].motorTorque = 0.0f;
                    break;
                }
        }
    }

    float lastInputV = 0;
    void processInput()
    {
        isDoBreak = false;
        throttle = 0;

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (h != 0)
        {
            steerAngle = h * maxSteerAngle;
            steerAngle = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);
        }
        else
        {
            steerAngle = 0;
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            EnablePhySimulation = !EnablePhySimulation;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            doBrake();
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (wheels[(int)Wheels.LeftFront].breaked > 0.9f)
                destroyWheel(Wheels.LeftFront);
            else
                restoreWheel(Wheels.LeftFront);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (wheels[(int)Wheels.RightFront].breaked > 0.9f)
                destroyWheel(Wheels.RightFront);
            else
                restoreWheel(Wheels.RightFront);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (wheels[(int)Wheels.LeftBack].breaked > 0.9f)
                destroyWheel(Wheels.LeftBack);
            else
                restoreWheel(Wheels.LeftBack);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (wheels[(int)Wheels.RightBack].breaked > 0.9f)
                destroyWheel(Wheels.RightBack);
            else
                restoreWheel(Wheels.RightBack);
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (destroyed == false)
                destroyCar(true);
            else
                OnDisable();
        }


        throttle = v;
        lastInputV = v;

        if (isDoBreak == false)
            brakeTorque = 0.0f;
    }

    string wheel_lf = "lfw";
    string wheel_rf = "rfw";
    string wheel_lb = "lbw";
    string wheel_rb = "rbw";
    // Update is called once per frame
    void Update() {

        getSpeed();

        if (enableTest == true && enablePhySimulation == true)
            processInput();

        if (enablePhySimulation == false)
        {
            if (useInterplationSync == false)
                syncState();
            else
                interplateSyncState();
        }
        else
        {
            steerAngle = Mathf.Clamp(steerAngle, -maxSteerAngle, maxSteerAngle);
            float flameOutScale = isFlameout ? 0 : 1.0f;

            WheelData wd = wheels[(int)Wheels.LeftFront];
            WheelCollider lfwc = wd.wc;
            lfwc.motorTorque = wd.motorTorque * throttle * wd.breaked * flameOutScale;
            lfwc.brakeTorque = brakeTorque + minBrakeTorque;

            wd = wheels[(int)Wheels.RightFront];
            WheelCollider rfwc = wd.wc;
            rfwc.motorTorque = wd.motorTorque * throttle * wd.breaked * flameOutScale;
            rfwc.brakeTorque = brakeTorque + minBrakeTorque;

            rfwc.steerAngle = steerAngle;
            lfwc.steerAngle = steerAngle;

            rpm = lfwc.rpm;
            float r = (lfwc.rpm / 60.0f) * Time.deltaTime * 360.0f;
            Transform lfw_wheel = gameObject.GetComponent<Transform>().Find(wheel_lf);
            lfw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);
            if (steerAngle != 0)
            {
                lfw_wheel.localRotation = Quaternion.Euler(lfw_wheel.localRotation.x, steerAngle, lfw_wheel.localRotation.z);
            }

            r = (rfwc.rpm / 60.0f) * Time.deltaTime * 360.0f;
            Transform rfw_wheel = gameObject.GetComponent<Transform>().Find(wheel_rf);
            rfw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);
            if (steerAngle != 0)
            {
                rfw_wheel.localRotation = Quaternion.Euler(rfw_wheel.localRotation.x, steerAngle, rfw_wheel.localRotation.z);
            }

            wd = wheels[(int)Wheels.LeftBack];
            Transform lbw_wheel = gameObject.GetComponent<Transform>().Find(wheel_lb);
            WheelCollider lbwc = wd.wc;
            lbwc.motorTorque = wd.motorTorque * throttle * wd.breaked * flameOutScale;

            r = (lbwc.rpm / 60.0f) * Time.deltaTime * 360.0f;
            lbwc.brakeTorque = brakeTorque + minBrakeTorque;
            lbw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);


            wd = wheels[(int)Wheels.RightBack];
            Transform rbw_wheel = gameObject.GetComponent<Transform>().Find(wheel_rb);
            WheelCollider rbwc = wd.wc;
            rbwc.motorTorque = wd.motorTorque * throttle * wd.breaked * flameOutScale;

            r = (rbwc.rpm / 60.0f) * Time.deltaTime * 360.0f;
            rbwc.brakeTorque = brakeTorque + minBrakeTorque;
            rbw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);

            updateSyncData();
        }       
    }

    private void FixedUpdate()
    {
        if (enablePhySimulation == true)
        {
            //添加阻力
            if (resistance > 0)
            {
                GetComponent<Rigidbody>().AddForce(moveDir * -resistance);           
            }
        }

    }

    private void LateUpdate()
    {
        //if (enablePhySimulation == true)
        //{
        //    updateSyncData();
        //}
        //else
        //{
        //    if (useInterplationSync == false)
        //        syncState();
        //    else
        //        interplateSyncState();
        //}
    }

     // 碰撞开始
    void OnCollisionEnter(Collision collision)
    {
        if (collisionCallBack != null)
            collisionCallBack(collision);
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.layer == waterLayerInt)
        {
            BoxCollider waterBox = collision.gameObject.GetComponent<BoxCollider>();
            float waterLevel =  waterBox.center.y + waterBox.size.y * 0.5f +
                collision.gameObject.GetComponent<Transform>().position.y;

            float diff = waterLevel - tran.position.y;
            diff = Mathf.Min(diff, 2.0f);
            if(diff > 0.1f)
            {
                resistance = diff * 3000.0f * speed;

                if (diff > flameoutLevel)
                    isFlameout = true;
            }
            else
            {
                resistance = 0;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collisionExitCallBack != null)
        {
            collisionExitCallBack(collision);
        }
    }

    private void syncState()
    {
        Quaternion Q = Quaternion.Euler(
            ((float)syncData.rotateX) / rotateModifier, 
            ((float)syncData.rotateY) / rotateModifier,
            ((float)syncData.rotateZ) / rotateModifier);
        transform.SetPositionAndRotation(War.Scene.StreamerManager.GetTilePosition(syncData.position), Q);

        //tran.localRotation = Q;
        //tran.position = syncData.position;

        WheelData wd = wheels[(int)Wheels.LeftFront];

        wd = wheels[(int)Wheels.RightFront];

        rpm = syncData.rpm;
        float r = (rpm / 60.0f) * Time.deltaTime * 360.0f;
        Transform lfw_wheel = gameObject.GetComponent<Transform>().Find(wheel_lf);
        lfw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);
        if (syncData.steerAngle != 0)
        {
            lfw_wheel.localRotation = Quaternion.Euler(lfw_wheel.localRotation.x, syncData.steerAngle, lfw_wheel.localRotation.z);
        }

        Transform rfw_wheel = gameObject.GetComponent<Transform>().Find(wheel_rf);
        rfw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);
        if (syncData.steerAngle != 0)
        {
            rfw_wheel.localRotation = Quaternion.Euler(rfw_wheel.localRotation.x, syncData.steerAngle, rfw_wheel.localRotation.z);
        }

        wd = wheels[(int)Wheels.LeftBack];
        Transform lbw_wheel = gameObject.GetComponent<Transform>().Find(wheel_lb);

        r = (rpm / 60.0f) * Time.deltaTime * 360.0f;
        lbw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);

        wd = wheels[(int)Wheels.RightBack];
        Transform rbw_wheel = gameObject.GetComponent<Transform>().Find(wheel_rb);

        r = (rpm / 60.0f) * Time.deltaTime * 360.0f;
        rbw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);


        //同步轮胎破坏？运动上不用管，只是显示隐藏轮胎皮而已，现在没有轮胎皮给你
        //所以后面资源到了再说
    }

    void interplateSyncState()
    {

        float deltaMovement = (syncData.position - 
            War.Scene.StreamerManager.GetRealPosition(transform.position)).magnitude;

        float minSpeed = deltaMovement * 3.0f;
        float interpSpeed = Mathf.Max(minSpeed, averageSyncSpeed);

        float needMove = interpSpeed * 0.5f * UnityEngine.Time.deltaTime;

        float t = needMove / deltaMovement;

        t = Mathf.Clamp(t, 0.1f, 1.0f);

        if (deltaMovement <= 0.1f)
        {
            Quaternion Q = Quaternion.Euler(
                ((float)syncData.rotateX) / rotateModifier,
                ((float)syncData.rotateY) / rotateModifier,
                ((float)syncData.rotateZ) / rotateModifier);
            transform.SetPositionAndRotation(War.Scene.StreamerManager.GetTilePosition(syncData.position), Q);
        }
        else
        {        
            Vector3 newPos = Vector3.Lerp(transform.position, 
                War.Scene.StreamerManager.GetTilePosition(syncData.position), t);

            Quaternion toQ = Quaternion.Euler(
                ((float)syncData.rotateX) / rotateModifier,
                ((float)syncData.rotateY) / rotateModifier,
                ((float)syncData.rotateZ) / rotateModifier);

            Quaternion Q = Quaternion.Lerp(transform.rotation, toQ, t);

            transform.SetPositionAndRotation(newPos, Q);

            //syncT += 0.001f;
            //syncT = Mathf.Clamp(syncT, 0, 1.0f);
        }

        //tran.localRotation = Q;
        //tran.position = syncData.position;

        WheelData wd = wheels[(int)Wheels.LeftFront];

        wd = wheels[(int)Wheels.RightFront];

        rpm = syncData.rpm;
        float r = (rpm / 60.0f) * Time.deltaTime * 360.0f;
        Transform lfw_wheel = gameObject.GetComponent<Transform>().Find(wheel_lf);
        lfw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);
        if (syncData.steerAngle != 0)
        {
            lfw_wheel.localRotation = Quaternion.Euler(lfw_wheel.localRotation.x, syncData.steerAngle, lfw_wheel.localRotation.z);
        }

        Transform rfw_wheel = gameObject.GetComponent<Transform>().Find(wheel_rf);
        rfw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);
        if (syncData.steerAngle != 0)
        {
            rfw_wheel.localRotation = Quaternion.Euler(rfw_wheel.localRotation.x, syncData.steerAngle, rfw_wheel.localRotation.z);
        }

        wd = wheels[(int)Wheels.LeftBack];
        Transform lbw_wheel = gameObject.GetComponent<Transform>().Find(wheel_lb);

        r = (rpm / 60.0f) * Time.deltaTime * 360.0f;
        lbw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);

        wd = wheels[(int)Wheels.RightBack];
        Transform rbw_wheel = gameObject.GetComponent<Transform>().Find(wheel_rb);

        r = (rpm / 60.0f) * Time.deltaTime * 360.0f;
        rbw_wheel.GetChild(0).Rotate(r, 0, 0, Space.Self);


        //同步轮胎破坏？运动上不用管，只是显示隐藏轮胎皮而已，现在没有轮胎皮给你
        //所以后面资源到了再说
    }

    float rotateModifier = 10.0f;
    void updateSyncData()
    {
        syncData.position = War.Scene.StreamerManager.GetRealPosition(transform.position);

        Vector3 eular = transform.rotation.eulerAngles;

        syncData.rotateX = (short)(eular.x * rotateModifier);
        syncData.rotateY = (short)(eular.y * rotateModifier);
        syncData.rotateZ = (short)(eular.z * rotateModifier);
        syncData.rpm = (short)rpm;
        syncData.steerAngle = (short)steerAngle;

        syncData.wheelBreak = 0;
        for (int i = 0; i < 4; ++i)
        {
            if (wheels[0].breaked >= 0.9)
                syncData.wheelBreak |= (byte)((1 << i));
        }
    }

    public VehiclesSyncData getSyncData()
    {
        return syncData;
    }

    float averageSyncSpeed = 0.0f;
    bool syncDataSeted = false;

    public void setSyncData(VehiclesSyncData inData)
    {
        if(syncDataSeted == true)
        {
            float dist = (inData.position - syncData.position).magnitude;
            float t = UnityEngine.Time.deltaTime;
            if(t >= 0.001)
            {
                float curSpeed = dist / t;
                averageSyncSpeed = averageSyncSpeed * 0.5f + curSpeed * 0.5f;
            }
        }

        syncData = inData;
        syncDataSeted = true;
    }

    Vector3 lastPosition = Vector3.zero;
    float tmpMileage = 0;
    float tmpSumTime = 0;
    void getSpeed()
    {
        var currentPosition = War.Scene.StreamerManager.GetRealPosition(transform.position);

        float moveDiff = 0;
        moveDir = (currentPosition - lastPosition);
        moveDiff = moveDir.magnitude;
        tmpMileage += moveDiff;
        tmpSumTime += Time.deltaTime;

        if(tmpSumTime > 0.5f)
        {
            speed = (tmpMileage / tmpSumTime) * 3600.0f * 0.001f;
            tmpSumTime = tmpMileage = 0;
        }

        lastPosition = currentPosition;

        if (Mathf.Abs(moveDiff) > 0.00001f)
            moveDir.Normalize();
        else
            moveDir = Vector3.zero;
    }

    /// <summary>
    /// 重新计算速度。上层会用这个速度来数计算消耗，但是在移动载具时会产生一个很大的速度，
    /// 并且这个速度在0.5s内不会重新计算，这里提供这样一个接口，让上层可以在合适的时机让速度重新计算。
    /// </summary>
    public void recalcSpeed()
    {
        lastPosition = War.Scene.StreamerManager.GetRealPosition(transform.position);
        tmpMileage = 0;
        tmpSumTime = 0.5f;
        getSpeed();
    }

    bool isDoBreak = false;

    public void doBrake()
    {
        if (brakeTorque < 800.0f)
            brakeTorque = 800.0f;

        brakeTorque += (1000.0f * Time.deltaTime);

        if (brakeTorque > maxBrakeTorque)
        {
            brakeTorque = maxBrakeTorque;
        }

        isDoBreak = true;
    }

    public void releaseBreak()
    {
        brakeTorque = 0;
        isDoBreak = false;
    }

    private float wheelRadius = 1.0f;
    public void destroyWheel(Wheels whichWheel)
    {
        int idx = (int)whichWheel;
        WheelCollider wc = wheels[idx].wc;

        if(wheels[idx].breaked >= 0.9f)
        {
            wheelRadius = wc.radius;
            wheels[idx].breaked = 0.24f;
            wc.radius = RimRadius;
            wc.mass *= 0.5f;
            wc.suspensionDistance *= 0.5f;

            WheelFrictionCurve wfc = wc.forwardFriction;
            wfc.extremumSlip *= 0.5f;
            wfc.extremumValue *= 0.25f;
            wfc.asymptoteSlip *= 0.5f;
            wfc.asymptoteValue *= 0.25f;
            wc.forwardFriction = wfc;

            wfc = wc.sidewaysFriction;
            wfc.extremumValue *= 0.5f;
            wfc.asymptoteValue *= 0.5f;
            wc.sidewaysFriction = wfc;

            if (wheels[idx].tire != null)
            {
                //Destroy(wheels[idx].tire);
                wheels[idx].tire.SetActive(false);
                //wheels[idx].tire = null;
            }
            
        }

        GetComponent<Rigidbody>().AddExplosionForce(120000.0f, wc.GetComponent<Transform>().position, 20.0f);
        //GetComponent<Rigidbody>().AddExplosionForce(5000.0f, Vector3.zero, 540.0f);
    }

    public void restoreWheel(Wheels whichWheel)
    {
        int idx = (int)whichWheel;
        WheelCollider wc = wheels[idx].wc;

        //假如还没有执行过Start时会为null
        if (null == wc)
        {
            return;
        }

        if (wheels[idx].breaked < 1.0f)
        {
            wheels[idx].breaked = 1.0f;
            wc.radius = wheelRadius;
            wc.mass *= 2.0f;
            wc.suspensionDistance *= 2.0f;

            WheelFrictionCurve wfc = wc.forwardFriction;
            wfc.extremumSlip *= 2.0f;
            wfc.extremumValue *= 4.0f;
            wfc.asymptoteSlip *= 2.0f;
            wfc.asymptoteValue *= 4.0f;
            wc.forwardFriction = wfc;

            wfc = wc.sidewaysFriction;
            wfc.extremumValue *= 2.0f;
            wfc.asymptoteValue *= 2.0f;
            wc.sidewaysFriction = wfc;

            if (wheels[idx].tire != null)
            {
                wheels[idx].tire.SetActive(true);
            }
        }

    }
}
