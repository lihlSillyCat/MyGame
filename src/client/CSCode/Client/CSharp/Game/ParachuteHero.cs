using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using War.Scene;

namespace War.Game
{
    public class ParachuteHero : Parachute
    {   
        public System.Action OnCanDeploy;
        public System.Action OnForceDeploy;
        public System.Action OnLanded;

        public delegate void ParachuteUpdateEventHandler(float terrainAltitude, float height, float downSpeed);
        public ParachuteUpdateEventHandler OnUpdate;

        public delegate void OnSkyDirveChangeHandler(float dir);
        public OnSkyDirveChangeHandler OnSkyDirveEvent;

        private bool OnCanDeployCalled = false;
        private bool OnForceDeployCalled = false;
        private bool OnLanedCalled = false;
        private bool OnDeployCalled = false;
        private bool StopMove = false;

        private Quaternion m_TargetRotate;   

        private void Awake()
        {
            OnAwake();
            OnSkyDirveEvent = null;
            OnCanDeployCalled = false;
            OnForceDeployCalled = false;
            OnLanedCalled = false;
            OnDeployCalled = false;
            TerrainAltitude = -1000;
        }
                
        private void FixedUpdate()
        {
            CheckTerrainHeight(transform.position.y + 50);
            CheckFowardBlock();
            if (!StopMove)
                UpdateMove();

            float altitude = m_Transform.position.y;

            switch(State)
            {
                case ChuteState.Skydive:
                    if (OnCanDeployCalled == false && CanDeployChute())
                    {
                        if (OnCanDeploy != null)
                        {
                            OnCanDeploy.Invoke();
                        }
                        OnCanDeployCalled = true;
                    }

                    if (OnForceDeployCalled == false && CanForceDeployChute())
                    {
                        if (OnForceDeploy != null)
                        {
                            OnForceDeploy.Invoke();
                            StopMove = true;
                        }
                        
                        OnForceDeployCalled = true;
                    }
                    break;

                case ChuteState.Deployed:
                    {
                        if (OnDeployCalled == false)
                        {
                            if (StreamerManager.Instance)
                                StreamerManager.Instance.SetPlayer(m_ChuteGameObject.transform);
                            OnDeployCalled = true;
                        }
                        
                        StopMove = false;
                        if (CanLanding())
                            PrepareLand();
                    }
                    break;

                case ChuteState.Landing:
                    if (CanLaned())
                    {
                        Landed();                       
                    }
                    break;

                case ChuteState.Landed:
                    if (OnLanded != null && OnLanedCalled == false)
                    {
                        OnLanded.Invoke();
                        if (StreamerManager.Instance)
                            StreamerManager.Instance.SetPlayer(transform);
                        OnCanDeployCalled = true;
                    }
                    return;
            }

            m_lastAltitude = m_Transform.position.y;
        }

       
        private void Update()
        {
            JoystickAdaptor.Update();

            switch (State)
            {
                case ChuteState.Skydive:
                    transform.rotation = Quaternion.Slerp(transform.rotation, m_TargetRotate, 0.03f);
                    float forward = SkyDirveAnimatorControl();
                    if (OnSkyDirveEvent != null)
                        OnSkyDirveEvent(forward);

                    break;
                case ChuteState.Deployed:
                    DeployedAnimatorControl();
                    break;
            }

            if (OnUpdate != null)
            {
                OnUpdate(TerrainAltitude, m_ObjectToMove.GetPosition().y, Velocity.y);
            }
        }


        public void TurnToCamera()
        {
            Vector3 f = Camera.main.transform.forward;
            f.y = 0;
            m_TargetRotate = Quaternion.FromToRotation(Vector3.forward, f.normalized);
        }

        public Vector3 GetSyncPosition()
        {
            switch (State)
            {
                case ChuteState.Skydive:
                    {
                        Vector3 pos = m_ObjectToMove.GetPosition();
                        return pos;
                    }

                case ChuteState.Deployed:
                case ChuteState.Landing:
                case ChuteState.Landed:
                    {
                        Vector3 pos = m_ChuteGameObject.GetComponent<ObjectToMove>().GetPosition();
                        return pos;
                    }
            }
            return Vector3.zero;
        }

        public float GetSyncRotate()
        {
            switch (State)
            {
                case ChuteState.Skydive:
                    {
                        return transform.rotation.eulerAngles.y;
                    }

                case ChuteState.Deployed:
                    {
                        return m_ChuteGameObject.transform.rotation.eulerAngles.y;

                    }
            }
            return 0;
        }
    }   
}