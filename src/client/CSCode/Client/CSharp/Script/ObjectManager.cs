using System.Collections.Generic;

namespace War.Script
{
    public class ObjectManager
    {
        public static ObjectManager Instance = null;

        protected HashSet<BaseObject> m_Objects;
        protected HashSet<BaseObject> m_ObjectsToAdd;
        protected HashSet<BaseObject> m_ObjectsToRemove;

        public ObjectManager()
        {
            m_Objects = new HashSet<BaseObject>();
            m_ObjectsToAdd = new HashSet<BaseObject>();
            m_ObjectsToRemove = new HashSet<BaseObject>();

            Instance = this;
        }

        public void OnObjectCreate(BaseObject obj)
        {
            m_ObjectsToAdd.Add(obj);
        }

        public void OnObjectDestroy(BaseObject obj)
        {
            m_ObjectsToRemove.Add(obj);
        }

        protected void UpdateObjectList()
        {
            foreach (BaseObject obj in m_ObjectsToRemove)
            {
                if (m_ObjectsToAdd.Contains(obj))
                {
                    m_ObjectsToAdd.Remove(obj);
                }

                /*
                else
                {
                    m_Objects.Remove(obj);
                }*/

                m_Objects.Remove(obj);
            }
            m_ObjectsToRemove.Clear();

            foreach (BaseObject obj in m_ObjectsToAdd)
            {
                m_Objects.Add(obj);
            }
            m_ObjectsToAdd.Clear();
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            UpdateObjectList();

            foreach (BaseObject obj in m_Objects)
            {
                obj.FixedUpdate(fixedDeltaTime);
            }
        }

        public void Update(float deltaTime)
        {
            UpdateObjectList();

            foreach (BaseObject obj in m_Objects)
            {
                obj.Update(deltaTime);
            }
        }

        public void LateUpdate()
        {
            UpdateObjectList();

            foreach (BaseObject obj in m_Objects)
            {
                obj.LateUpdate();
            }
        }

        public void Destroy()
        {
            UpdateObjectList();

            foreach (BaseObject obj in m_Objects)
            {
                obj.Dispose();
            }
            m_Objects.Clear();
            m_Objects = null;

            m_ObjectsToAdd = null;
            m_ObjectsToRemove = null;

            Instance = null;
        }
    }
}