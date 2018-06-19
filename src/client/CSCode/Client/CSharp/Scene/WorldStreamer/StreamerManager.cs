using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace War.Scene
{
    /// <summary>
    /// 
    /// </summary>
    public class StreamerManager : MonoBehaviour
    {
        [Tooltip("List of streamers objects that should affect loading screen. Drag and drop here all your streamer objects from scene hierarchy which should be used in loading screen.")]
        /// <summary>
        /// The collider streamers.
        /// </summary>
        public Streamer[] streamers;

        public event Action SceneLoaded;
        public event Action<double> LoadingProgressChanged;

        [SerializeField]
        protected WorldMover m_WorldMover;
        
        public static StreamerManager Instance = null;

        void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        void Update()
        {
            if (streamers.Length > 0)
            {
                bool initialized = true;
                float progress = 0;

                for (int i = 0; i < streamers.Length; ++i)
                {
                    Streamer item = streamers[i];
                    progress += item.LoadingProgress / (float)streamers.Length;
                    initialized = initialized && item.initialized;
                }

                if (LoadingProgressChanged != null)
                { 
                    LoadingProgressChanged.Invoke(progress);
                }

                if (initialized)
                {
                    if (progress >= 1)
                    {
                        if (SceneLoaded != null)
                            SceneLoaded.Invoke();

                        this.enabled = false;
                    }
                }
            }
        }

        public void SetPlayer(Transform player)
        {
            for (int i = 0; i < streamers.Length; ++i)
            {
                streamers[i].player = player;
            }

            ColliderStreamerManager manager = ColliderStreamerManager.Instance;
            if (manager != null)
            {
                manager.player = player;
            }
        }

        public void AddObjectToMove(Transform objectToMove)
        {
            m_WorldMover.AddObjectToMove(objectToMove);
        }

        public void RemoveObjectToMove(Transform objectToMove)
        {
            m_WorldMover.RemoveObjectToMove(objectToMove);
        }

        public bool HasObjectToMove(Transform objectToMove)
        {
            return m_WorldMover.HasObjectToMove(objectToMove);
        }
        
        public static float GetPlayerDistance(Vector3 position /*tile position*/)
        {
            if (Instance != null)
            {
                if (Instance.streamers.Length >= 1)
                {
                    var player = Instance.streamers[0].player;
                    if (player != null)
                    {
                        return (player.position - position).magnitude;
                    }
                }
            }

            return 0f;
        }

        public static float GetPlayerDistanceSqr(Vector3 position /*tile position*/)
        {
            if (Instance != null)
            {
                if (Instance.streamers.Length >= 1)
                {
                    var player = Instance.streamers[0].player;
                    if (player != null)
                    {
                        return (player.position - position).sqrMagnitude;
                    }
                }
            }

            return 0f;
        }

        public static Vector3 GetTilePosition(float x, float y, float z)
        {
            if (Instance != null)
            {
                return new Vector3(x, y, z) + Instance.m_WorldMover.currentMove;
            }

            return new Vector3(x, y, z);
        }

        public static Vector3 GetTilePosition(Vector3 pos)
        {
            if (Instance != null)
            {
                return  pos + Instance.m_WorldMover.currentMove;
            }
            return pos;
        }

        public static Vector3 GetRealPosition(float x, float y, float z)
        {
            if (Instance != null)
            {
                return new Vector3(x, y, z) - Instance.m_WorldMover.currentMove;
            }

            return new Vector3(x, y, z);
        }

        public static Vector3 GetRealPosition(Vector3 pos)
        {
            if (Instance != null)
            {
                return pos - Instance.m_WorldMover.currentMove;
            }

            return pos;
        }

        public static Vector3 GetCurrentMove()
        {
            if (Instance != null)
            {
                return Instance.m_WorldMover.currentMove;
            }
            return Vector3.zero;
        }
    }
}
