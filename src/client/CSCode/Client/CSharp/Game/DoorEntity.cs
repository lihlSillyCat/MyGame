using System;
using System.Linq;
using System.Text;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using War.Scene;

namespace War.Game
{ 

    /// <summary>
    /// 门实体组件
    /// </summary>
    public class DoorEntity : MonoBehaviour
    {        
        int  m_openState = 0;  // 打开状态  0 关闭状态  1 90度打开 2 -90度打开
        int  m_formState = 0;        
        bool m_isRotation = false;
        bool m_allBreaked = false;
        Quaternion m_closeRotation;
        Quaternion m_openRotation1;  // 90 度开门旋转
        Quaternion m_openRotation2;  // -90 度开门旋转
        Quaternion m_toRotation;
        protected Scene.ObjectToMove m_ObjectToMove;
        private DoorPart[] doorParts;
        public delegate void OnColliderHandler(int doorID, int playrSid);
        public delegate void OnDestroyHandler(Vector3 pos);
        public OnColliderHandler OnPlayerEnter;
        public OnColliderHandler OnPlayerLeave;
        public OnDestroyHandler OnPartDestroy;
        

        public Vector3 position
        {
            get
            {
                return m_ObjectToMove.GetPosition();
            }
        }

        private int m_DoorID = 0;
        public int doorID
        {
            get
            {
                return m_DoorID;
            }
            set
            {
                m_DoorID = value;
                InitDoorPart(m_DoorID);
            }
        }
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            doorParts = GetComponentsInChildren<DoorPart>();
            m_ObjectToMove = GetComponent<Scene.ObjectToMove>();
        }

        private void InitDoorPart(int doorID)
        {
            for(int i = 0; i < doorParts.Length; i++)
            {
                doorParts[i].doorID = doorID;
            }
        }

        public Transform GetHitPart(int partID)
        {
            for (int i = 0; i < doorParts.Length; i++)
            {
                if(doorParts[i].partID == partID)
                {
                    return doorParts[i].transform;
                } 
            }
            return null;
        }

        private void OnEnable()
        {
            doorID = 0;
            m_openState = 0;
            m_formState = 0;
            m_isRotation = false;
            m_allBreaked = false;
        }

        private void OnDisable()
        {
            OnPlayerEnter = null;
            OnPlayerLeave = null;
        }

        public void SetPosition(float x, float y, float z)
        {
            m_ObjectToMove.SetPosition(x, y, z);
        }

        public void SetRotation(float x, float y, float z, float w)
        {
            transform.rotation = new Quaternion(x, y, z, w);
            m_closeRotation = transform.rotation;
            m_openRotation1 = m_closeRotation * Quaternion.Euler(0, 90, 0);
            m_openRotation2 = m_closeRotation * Quaternion.Euler(0, -90, 0);
        }

        public void SetFormState(int state)
        {
            if (m_formState == state)
                return;
            m_formState = state;          
        }

        public void SwitchOpenState(float x, float y, float z)
        {
            if (m_isRotation)
                return;
            
            if (m_openState == 1 || m_openState == 2)
            {
                m_toRotation = m_closeRotation;
                m_openState = 0;
                StartRotationDoor();
                return;
            }                           

            if (m_openState == 0)
            {
                Vector3 pos = new Vector3(x, y, z);
                Vector3 dir1 = pos - m_ObjectToMove.GetPosition();
                if (Vector3.Dot(dir1, transform.forward) > 0)
                {
                    m_toRotation = m_openRotation1;
                    m_openState = 1;
                }
                else
                {
                    m_toRotation = m_openRotation2;
                    m_openState = 2;
                }
                StartRotationDoor();
            }           
        }

        public int GetOpenState()
        {
            return m_openState;
        }

        public void SetOpenState(int state)
        {
            if (m_openState == state)
                return;
            if ((m_openState == 1 || m_openState == 2) && (state == 1 || state == 2))
                return;

            if (state == 1)
                m_toRotation = m_openRotation1;

            if (state == 2)
                m_toRotation = m_openRotation2;

            if (m_openState == 1 || m_openState == 2)
                m_toRotation = m_closeRotation;

            m_openState = state;
            transform.rotation = m_toRotation;       
        }

        public void SetCompleteState(int state)
        {
            DoorPart[] parts =  transform.gameObject.GetComponentsInChildren<DoorPart>();
            if (parts == null) return;

            List<int> arry = new List<int>();
            switch(state)
            {
                case 0:
                    break;
                case 1:
                    arry.Add(3);
                    break;
                case 2:
                    arry.Add(2);
                    break;
                case 3:
                    arry.Add(2);
                    arry.Add(3);
                    break;
                case 4:
                    arry.Add(1);
                    break;
                case 5:
                    arry.Add(1);
                    arry.Add(3);
                    break;
                case 6:
                    arry.Add(1);
                    arry.Add(2);
                    break;
                case 7:
                    arry.Add(1);
                    arry.Add(2);
                    arry.Add(3);
                    m_allBreaked = true;
                    break;
            }

            foreach( var part in parts)
            {
                if (arry.Contains(part.partID))
                {
                    part.gameObject.SetActive(false);
                    if (OnPartDestroy != null)
                    {
                        var move = transform.gameObject.GetComponent<ObjectToMove>().GetPosition();
                        Vector3 right = (transform.right.normalized * part.effectPos.x);
                        Vector3 up = (transform.up.normalized * part.effectPos.y);
                        OnPartDestroy(move + right + up);
                    }   
                }                    
                else
                    part.gameObject.SetActive(true);
            }

            if (m_allBreaked)
            {
                if (OnPlayerLeave != null)
                    OnPlayerLeave(doorID, 0);
            }
        }

        public void SyncOpenState(int state)
        {
            if (m_openState == state)
                return;
            if ((m_openState == 1 || m_openState == 2) && (state == 1 || state == 2))
                return;

            if (state == 1)
                m_toRotation = m_openRotation1;

            if (state == 2)
                m_toRotation = m_openRotation2;

            if (m_openState == 1 || m_openState == 2)
                m_toRotation = m_closeRotation;

            m_openState = state;
            StartRotationDoor();
        }

        private void StartRotationDoor()
        {
            StopCoroutine("RotationDoor");
            StartCoroutine("RotationDoor");
            if (m_openState == 0) // 关门
                AkSoundEngine.PostEvent("SoundEvent_Door_Close", gameObject);
            else
                AkSoundEngine.PostEvent("SoundEvent_Door_Open", gameObject);

        }

        private void OnTriggerEnter(Collider other)
        {
            if (m_allBreaked)
                return;

            if(other.gameObject.layer == LayerConfig.Player)
            {
                CharacterEntity entity = other.gameObject.GetComponent<CharacterEntity>();
                if (OnPlayerEnter != null)
                    OnPlayerEnter(doorID, entity.sid);
            }            
        }

        private void OnTriggerExit(Collider other)
        {
            if (m_allBreaked)
                return;

            if (other.gameObject.layer == LayerConfig.Player)
            {
                CharacterEntity entity = other.gameObject.GetComponent<CharacterEntity>();
                if (OnPlayerLeave != null)
                    OnPlayerLeave(doorID, entity.sid);
            }
        }

        public bool IsOperateState()
        {
            return m_isRotation;
        }

        IEnumerator RotationDoor()
        {
            m_isRotation = true;
            Quaternion startRotation = transform.rotation;            

            for (int i = 1; i <= 20; i++)
            {
                transform.rotation = Quaternion.Lerp(startRotation, m_toRotation, i / 20.0f);
                if (i == 19)
                    m_isRotation = false;
                yield return null;
            }

            m_isRotation = false;
        }

    }
}
