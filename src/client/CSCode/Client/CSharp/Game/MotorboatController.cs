using UnityEngine;
using System.Collections;
using System;

public struct BoatSyncData
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

public delegate string BoatCollisionProcess(Collision collision);

public class MotorboatController : MonoBehaviour {

    [SerializeField]
    public bool enableTest = false;

    public float steerAngle = 0;
    [SerializeField]
    private float maxEngineOut = 1000;

    private bool isFlameout = false;

    public float maxSteerAngle = 30.0f;
    public float throttle = 0.0f;

    public float speed = 0;
    public float waterLine = 0.5f; //吃水深度


    [SerializeField]
    public Vector3 massCenter;// = new Vector3(0, -0.12f, 0);

    [SerializeField]
    public bool enablePhySimulation = true;

    [SerializeField]
    public Texture2D destroyedTexture;

    private Texture2D replaceTexture = null;

    public float rotateTile = 0.25f;

    public Vector3[] floatPoints = new Vector3[4] { new Vector3(-0.5f,-0.5f,0.5f),
         new Vector3(0.5f,-0.5f,0.5f), new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(0.5f,-0.5f,-0.5f) };
    float[]   inWaterDepths = new float[4];
    Vector3[] floatPointsWorld = new Vector3[4];

    public BoatCollisionProcess collisionCallBack = null;

    /// <summary>
    /// 碰撞退出时的回调，上层应用会用到这个事件来做一些逻辑处理，比如撞人的时候，
    /// 在这个时机主动的同步一次人物的数据w
    /// </summary>
    public BoatCollisionProcess collisionExitCallBack = null; 

    private BoatSyncData syncData;

    Transform tran = null;
    Transform propellerTran = null;
    Transform rudderTran = null;

    Vector3 moveDir = Vector3.zero;

    int waterLayerInt;
 
    bool destroyed = false;
    private Texture2D oriTexture = null;

    public bool useInterplationSync = true;

    public bool usePhysic = true;

    public Vector3 PropellerPosition = -Vector3.forward;

    public GameObject SpeedEffect = null;
    public GameObject MovingEffect = null;

    public float SpeedEffectOffset = 0.35f;
    public float MovingEffectOffset = -0.2f;
    public float SpeedEffectSpeed = 20.0f;

    public float MaxEngineOut
    {
        get
        {
            return maxEngineOut;
        }

        set
        {
            maxEngineOut = value;
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

        Transform ct = tran.Find("tank_ship_CT");
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

    public void destroyBoat(bool addExplosion)
    {
        destroyed = true;
        Flameout = true;

        applyTexture(destroyedTexture, true);
        
        if (addExplosion == true)
        {
            float expForce = UnityEngine.Random.Range(140000.0f, 250000.0f);
            float offsetX = UnityEngine.Random.Range(-0.2f, 0.25f);
            float offsetZ = UnityEngine.Random.Range(0.1f, 0.3f);
            GetComponent<Rigidbody>().AddExplosionForce(expForce, tran.position + (new Vector3(offsetX, 
                PropellerPosition.y, offsetZ)), 30.0f);
        }
    }

    public void restoreBoat()
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
    }

    private void startPhySimulation()
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        //gameObject.GetComponent<Rigidbody>().WakeUp();
    }

    private void stopPhySimulation()
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        //gameObject.GetComponent<Rigidbody>().Sleep();
    }

    private void OnDisable()
    {
        isFlameout = false;
        maxSteerAngle = 30.0f;
        throttle = 0.0f;
        speed = 0;

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

        restoreBoat();

        collisionCallBack = null;
        collisionExitCallBack = null;

        for (int i = 0; i < floatPoints.Length; ++i)
        {
            inWaterDepths[i] = 0f;
        }
    }

    // Use this for initializations
    public float Mass = 10;
    Rigidbody rigidBody = null;
void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        lastPosition = War.Scene.StreamerManager.GetRealPosition(transform.position);

        rigidBody.centerOfMass = massCenter;

        tran = GetComponent<Transform>();
        if (tran.childCount >= 3)
        {
            this.propellerTran = tran.GetChild(1);
            this.rudderTran = tran.GetChild(2);
        }
          
        if(replaceTexture != null)
            applyTexture(replaceTexture, false);

        waterLayerInt = LayerMask.NameToLayer("Water");
    }


    float lastInputV = 0;
    void processInput()
    {
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


        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (destroyed == false)
                destroyBoat(true);
            else
                OnDisable();
        }


        if(usePhysic)
        {

        }

        throttle = v;
        lastInputV = v;
    }

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
            updateSyncData();
        }
    }

    private void FixedUpdate()
    {
        if (enablePhySimulation == true)
        {
            processMotionPhysics();
        }
        updateEffects();
    }

    private void LateUpdate()
    {

    }
    public float RotateSpeed = 10.0f;
    void processMotionPhysics()
    {
        for(int i = 0; i < floatPoints.Length; ++i)
        {
            if(inWaterDepths[i] > waterLine)
            {
                float floatForce = inWaterDepths[i] * Mass;
                floatForce = Mathf.Min(floatForce, 3.0f);

                //将逻辑坐标转成真实Unity坐标
                Vector3 v3Float = War.Scene.StreamerManager.GetTilePosition(floatPointsWorld[i]);

                rigidBody.AddForceAtPosition(-Physics.gravity * floatForce * 0.25f, v3Float, ForceMode.Acceleration);
            }
        }

        if (inWater > 0.0f)
        {
            //if(inWater > waterLine)
            //{
            //    float floatForce = inWater *  Mass;          
            //    floatForce = Mathf.Min(floatForce, 3.0f);
            //    rigidBody.AddForce(-Physics.gravity * floatForce, ForceMode.Acceleration);
            //}


            float curOut = this.throttle * maxEngineOut;
            Vector3 dir = (tran.forward);
            dir.y = 0;

            dir.Normalize();


            rigidBody.AddForce(dir * curOut, ForceMode.Force);

            if (this.steerAngle != 0)
            {
                rigidBody.AddRelativeTorque(Vector3.up * this.steerAngle * RotateSpeed, ForceMode.Force);
                rigidBody.AddRelativeTorque(Vector3.back * this.steerAngle * rotateTile * RotateSpeed, ForceMode.Force);
            }

            //处理螺旋桨和方向舵
            if(propellerTran != null)
            {
                float r = -this.throttle * Time.deltaTime * 1000.0f ;
                propellerTran.GetChild(0).Rotate(0, 0, r, Space.Self);
            }
            if(rudderTran != null)
            {
                rudderTran.localRotation = Quaternion.Euler(rudderTran.localRotation.x, -steerAngle, rudderTran.localRotation.z);
            }

        }

    }
    
    void updateEffects()
    {
        if (MovingEffect != null)
        {
            if (speed > 5.0f && inWater > 0.1f)
            {
                if(MovingEffect.transform.parent == null)
                {
                    MovingEffect.transform.SetParent(tran);
                    
                }
                MovingEffect.SetActive(true);
                MovingEffect.transform.localPosition = (Vector3.zero) + new Vector3(0, 0, MovingEffectOffset);
                MovingEffect.transform.localRotation = Quaternion.identity;

            }
            else
            {
                MovingEffect.SetActive(false);
            }
        }

        if (SpeedEffect != null)
        {
            if (speed > SpeedEffectSpeed && inWater > 0.1f)
            {
                if (SpeedEffect.transform.parent == null)
                {
                    SpeedEffect.transform.SetParent(tran);
                }
                SpeedEffect.SetActive(true);
                SpeedEffect.transform.localPosition = (Vector3.zero) + new Vector3(0, 0, SpeedEffectOffset); ;
                SpeedEffect.transform.localRotation = Quaternion.identity;
            }
            else
            {
                SpeedEffect.SetActive(false);
            }
        }

    }

     // 碰撞开始
    void OnCollisionEnter(Collision collision)
    {
        if (collisionCallBack != null)
            collisionCallBack(collision);

    }

    private void OnCollisionStay(Collision collision)
    {

        if (collision.gameObject.layer != waterLayerInt)
        {

        }
    }

    private float inWater = 0;
    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.layer == waterLayerInt)
        {
            BoxCollider waterBox = collision.gameObject.GetComponent<BoxCollider>();
            float waterLevel =  waterBox.center.y + waterBox.size.y * 0.5f +
                collision.gameObject.GetComponent<Transform>().position.y;

            Vector3 cpos = tran.localToWorldMatrix.MultiplyPoint(PropellerPosition);

            inWater = waterLevel - (cpos.y);

            for (int i = 0; i < floatPoints.Length; ++i)
            {
                Vector3 posTmp = tran.localToWorldMatrix.MultiplyPoint(floatPoints[i]);

                //转成逻辑坐标，不然逻辑世界转换的时候，会翻船
                floatPointsWorld[i] = War.Scene.StreamerManager.GetRealPosition(posTmp);

                inWaterDepths[i] = waterLevel - (floatPointsWorld[i].y);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == waterLayerInt)
        {
            inWater = 0;
            for (int i = 0; i < floatPoints.Length; ++i)
            {
                inWaterDepths[i] = 0f;
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
        }

       
    }

    float rotateModifier = 10.0f;
    void updateSyncData()
    {
        syncData.position = War.Scene.StreamerManager.GetRealPosition(transform.position);

        Vector3 eular = transform.rotation.eulerAngles;

        syncData.rotateX = (short)(eular.x * rotateModifier);
        syncData.rotateY = (short)(eular.y * rotateModifier);
        syncData.rotateZ = (short)(eular.z * rotateModifier);
        syncData.steerAngle = (short)steerAngle;
    }

    public BoatSyncData getSyncData()
    {
        return syncData;
    }

    float averageSyncSpeed = 0.0f;
    bool syncDataSeted = false;

    public void setSyncData(BoatSyncData inData)
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
}
