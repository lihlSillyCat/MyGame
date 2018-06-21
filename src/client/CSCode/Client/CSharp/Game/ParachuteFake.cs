using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using War.Scene;

namespace War.Game
{
    public class ParachuteFake : Parachute
    {     
        private bool StopMove = false;    
        private Vector3 m_ShadowPosition;
        private int m_DetectTimes = 0;
        private Vector3 m_LastPostion;

        private void Awake()
        {
            OnAwake();
            m_ShadowPosition = Vector3.zero;
            m_LastPostion = Vector3.zero;
            m_DetectTimes = 0;
            TerrainAltitude = -1000;
        }
        
        private void DetectTerrainHeight()
        {
            Vector3 curPostion = m_Transform.position;
            curPostion.y = 0;
            m_LastPostion.y = 0;
            float distance = Vector3.Distance(curPostion, m_LastPostion);
            if (m_DetectTimes == 0)
            {                
                if(distance > 1 || TerrainAltitude == -1000)
                {
                    CheckTerrainHeight(curPostion.y + 50);
                }

                m_DetectTimes++;
                if (m_DetectTimes > 15)
                    m_DetectTimes = 0;
            }
        }

        private void FixedUpdate()
        {   
            if (!StopMove)
                UpdateMove();

            DetectTerrainHeight();

            switch(State)
            {
                case ChuteState.Skydive:
                    break;

                case ChuteState.Deployed:
                    {                        
                        if (CanLanding())
                        {
                            PrepareLand();
                        }
                    }
                    break;

                case ChuteState.Landing:
                    if (CanLaned())
                    {
                        StopMove = true;
                    }
                    break;

                case ChuteState.Landed:                  
                    return;
            }

            m_lastAltitude = m_Transform.position.y;
            m_LastPostion = m_Transform.position;
        }       

        private void Update()
        {
            JoystickAdaptor.Update();

            switch (State)
            {
                case ChuteState.Skydive:
                    SkyDirveAnimatorControl();
                    break;
                case ChuteState.Deployed:     
                    DeployedAnimatorControl();
                    break;
            }

            if (m_ShadowPosition != Vector3.zero)
            {
                UpdateShadow();
            }
        }

        public void SyncShadow(Vector3 pos, float rotate, Vector3 velocity, Vector2 joystick)
        {
            switch (State)
            {
                case ChuteState.Skydive:
                    {
                        m_ShadowPosition = pos;
                        Vector3 eular = transform.eulerAngles;
                        eular.y = rotate;
                        transform.eulerAngles = eular;

                        Velocity = velocity;
                        this.JoystickAdaptor.TargetAxis = joystick;
                    }
                    break;

                case ChuteState.Deployed:
                case ChuteState.Landing:
                case ChuteState.Landed:
                    {
                        m_ShadowPosition = pos;

                        Vector3 eular = m_ChuteGameObject.transform.eulerAngles;
                        eular.y = rotate;
                        m_ChuteGameObject.transform.eulerAngles = eular;

                        Velocity = velocity;
                        this.JoystickAdaptor.TargetAxis = joystick;
                    }
                    break;
            }
        }

        private void UpdateShadow()
        {
            switch (State)
            {
                case ChuteState.Skydive:
                    {
                        //Vector3 p = m_ObjectToMove.GetPosition();
                        //Vector3 pos = Vector3.Lerp(p, m_ShadowPosition, Time.deltaTime);
                        m_ObjectToMove.SetPosition(m_ShadowPosition);
                    }
                    break;

                case ChuteState.Deployed:
                case ChuteState.Landing:
                case ChuteState.Landed:
                    {
                        //Vector3 p = m_ChuteGameObject.GetComponent<ObjectToMove>().GetPosition();
                        //Vector3 pos = Vector3.Lerp(p, m_ShadowPosition, Time.deltaTime);
                        m_ChuteGameObject.GetComponent<ObjectToMove>().SetPosition(m_ShadowPosition);
                    }
                    break;
            }
        }
      
    }   
}