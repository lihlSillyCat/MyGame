using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace War.Scene
{
	public class ColliderStreamerManager : MonoBehaviour
	{
		[Tooltip ("Object that will start loading process after it hits the collider.")]
		/// <summary>
		/// The player transform.
		/// </summary>
		public Transform player;
		

        public static ColliderStreamerManager Instance = null;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        [HideInInspector]
		/// <summary>
		/// The collider streamers.
		/// </summary>
		public List<ColliderStreamer> colliderStreamers;
	
		/// <summary>
		/// Adds the collider streamer.
		/// </summary>
		/// <param name="colliderStreamer">Collider streamer.</param>
		public void AddColliderStreamer (ColliderStreamer colliderStreamer)
		{
			colliderStreamers.Add (colliderStreamer);
		}
	
		/// <summary>
		/// Adds the collider scene.
		/// </summary>
		/// <param name="colliderScene">Collider scene.</param>
		public void AddColliderScene (ColliderScene colliderScene)
		{
            for (int i = 0; i < colliderStreamers.Count; ++i)
            {
                ColliderStreamer item = colliderStreamers[i];
                if (item != null && item.sceneName == colliderScene.sceneName)
                {
                    item.SetSceneGameObject(colliderScene.gameObject);
                    break;
                }
            }
		}
	}
}
