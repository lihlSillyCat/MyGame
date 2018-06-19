using UnityEngine;
using System.Collections;
using System;

namespace War.Scene
{
	/// <summary>
	/// Scene split data
	/// </summary>
	[Serializable]
	public class SceneSplit
	{
		/// <summary>
		/// The position x.
		/// </summary>
		public int posX = 0; 
		/// <summary>
		/// The position y.
		/// </summary>
		public int posY = 0; 
		/// <summary>
		/// The position z.
		/// </summary>
		public int posZ = 0; 
		/// <summary>
		/// The name of the scene.
		/// </summary>
		public string sceneName;
        /// <summary>
        /// The name of the assets bundle.
        /// </summary>
        public string assetBundleName;
		/// <summary>
		/// The scene Game object.
		/// </summary>
		public GameObject sceneGo;
		/// <summary>
		/// Was scene loaded.
		/// </summary>
		public bool loaded;
		/// <summary>
		/// The position X looping move.
		/// </summary>
		public float posXLimitMove = 0; 
		/// <summary>
		/// The x deload looping.
		/// </summary>
		public int xDeloadLimit = 0; 
		/// <summary>
		/// The position Y looping move.
		/// </summary>
		public float posYLimitMove = 0;
		/// <summary>
		/// The y deload looping.
		/// </summary>
		public int yDeloadLimit = 0; 
		/// <summary>
		/// The position Z looping move.
		/// </summary>
		public float posZLimitMove = 0; 
		/// <summary>
		/// The z deload looping.
		/// </summary>
		public int zDeloadLimit = 0;

	
	}
}
