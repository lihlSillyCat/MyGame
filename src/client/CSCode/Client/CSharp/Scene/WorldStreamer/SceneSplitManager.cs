using UnityEngine;
using System.Collections;
using System.Linq;
using War.Base;

namespace War.Scene
{
	/// <summary>
	/// Scene split manager, finds streamer and adds scene.
	/// </summary>
	public class SceneSplitManager : MonoBehaviour
	{
		/// <summary>
		/// The name of the scene.
		/// </summary>
		public string sceneName;
	
		/// <summary>
		/// The gizmos color.
		/// </summary>
		public  Color color;
	
		/// <summary>
		/// The split position.
		/// </summary>
		[HideInInspector]
		public Vector3 position;
	
		/// <summary>
		/// The size of split.
		/// </summary>
		[HideInInspector]
		public Vector3 size = new Vector3 (10, 10, 10);

        public int streamIndex = -1;
	
		/// <summary>
		/// Start this instance, finds streamer and adds scene.
		/// </summary>
		void Start ()
		{
            if (StreamerManager.Instance != null)
			    AddToStreamer ();
            else
            {
                // null means main scene unloaded
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(this.gameObject.scene.name);
                //War.Base.AssetLoader.UnloadAssetBundle(item.assetBundleName);
            }
		}


        void AddToStreamer()
        {
            if (streamIndex >= 0 && streamIndex < StreamerManager.Instance.streamers.Length)
            {
                StreamerManager.Instance.streamers[streamIndex].AddSceneGO(sceneName, this.gameObject);
            }
            else
            {
                Streamer[] streamers = StreamerManager.Instance.streamers;
                for (int i = 0; i < streamers.Length; ++i)
                {
                    Streamer streamer = streamers[i];
                    if (streamer != null)
                    {
                        string[] names = streamer.sceneCollection.names;
                        for (int j = 0; j < names.Length; ++j)
                        {
                            string name = names[j];
                            //if (name.Replace(".unity", "").Equals(sceneName))
                            if (name.StartsWith(sceneName) && name[sceneName.Length] == '.')
                            {
                                streamer.AddSceneGO(sceneName, this.gameObject);
                            }
                        }
                    }
                }
            }
        }
	
		void  OnDrawGizmosSelected ()
		{
			// Display the explosion radius when selected
			Gizmos.color = color;
			Gizmos.DrawWireCube (position + size * 0.5f, size);
		}
	}
}
