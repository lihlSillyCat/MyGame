using UnityEngine;
using System.Collections;

/// <summary>
/// GPS定位状态
/// </summary>
public enum GPS_STATE 
{
    NONE = 1,                //不需要处理
    READY,                   //需要定位了
    LOCATING,              //正在定位
    SUC,                      //定位成功
    FAIL,                     //定位失败
    TIMEOUT,               //定位超时
    NOT_ENABLE_BY_USER, //设备没有开启定位
}

public class GPS : MonoBehaviour
{

    /// <summary>
    /// 定位状态
    /// </summary>
    private GPS_STATE m_state = GPS_STATE.NONE;

    /// <summary>
    /// 经纬度
    /// </summary>
    private Vector2 m_v2LatLng = new Vector2();

    /// <summary>
    /// 获取经纬度
    /// </summary>
    /// <returns></returns>
    public Vector2 GetLatLng()
    {
        return m_v2LatLng;
    }

    /// <summary>
    /// 启动定位
    /// </summary>
    public void StartGPS()
    {
        m_state = GPS_STATE.READY;
    }

    /// <summary>
    /// 是否定位完成
    /// </summary>
    /// <returns></returns>
    public bool IsFinished()
    {
        return m_state == GPS_STATE.NOT_ENABLE_BY_USER || m_state == GPS_STATE.FAIL
            || m_state == GPS_STATE.SUC || m_state == GPS_STATE.TIMEOUT;
    }

    /// <summary>
    /// 是否定位成功
    /// </summary>
    /// <returns></returns>
    public bool IsSuc()
    {
        return m_state == GPS_STATE.SUC;
    }

    // Use this for initialization  
    void Start()
    {
        if(m_state == GPS_STATE.READY)
        {
            m_state = GPS_STATE.LOCATING;
            StartCoroutine(StartGPSService());
        }
    }

    /// <summary>
    /// 停止定位
    /// </summary>
    void StopGPS()
    {
        m_state = GPS_STATE.NONE;
        Input.location.Stop();
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        string gps_info = "N:" + m_v2LatLng.x + " E:" + m_v2LatLng.y;
        GUI.skin.label.fontSize = 28;
        GUI.Label(new Rect(20, 20, 600, 200), gps_info);
    }
#endif

    IEnumerator StartGPSService()
    {
        // Input.location 用于访问设备的位置属性（手持设备）, 静态的LocationService位置  
        // LocationService.isEnabledByUser 用户设置里的定位服务是否启用  
        if (!Input.location.isEnabledByUser)
        {
            m_state = GPS_STATE.NOT_ENABLE_BY_USER;
            yield break;
        }

        // LocationService.Start() 启动位置服务的更新,最后一个位置坐标会被使用  
        Input.location.Start(100.0f, 100.0f);

        int maxWait = 5;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            // 暂停协同程序的执行(1秒)  
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        //超时
        if (maxWait < 1)
        {
            m_state = GPS_STATE.TIMEOUT;
            yield break;
        }

        //失败
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            m_state = GPS_STATE.FAIL;
            yield break;
        }
        else
        {
            m_v2LatLng.x = Input.location.lastData.latitude;
            m_v2LatLng.y = Input.location.lastData.longitude;
            m_state = GPS_STATE.SUC;
            yield break;
        }
    }
}