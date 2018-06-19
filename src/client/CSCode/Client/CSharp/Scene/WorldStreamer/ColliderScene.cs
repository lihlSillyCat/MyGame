using UnityEngine;
using System.Collections;

namespace War.Scene
{
    /// <summary>
    /// Collider scene.
    /// </summary>
    public class ColliderScene : MonoBehaviour
    {
        /// <summary>
        /// The name of the scene.
        /// </summary>
        public string sceneName;

        /// <summary>
        /// Start this instance adds to world mover and searches for collider streamer prefab.
        /// </summary>
        void Start()
        {
            ColliderStreamerManager.Instance.AddColliderScene(this);
        }
    }
}
