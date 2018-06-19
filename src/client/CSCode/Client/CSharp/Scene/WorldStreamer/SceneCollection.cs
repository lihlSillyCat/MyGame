using UnityEngine;
using System.Collections;
using System;

namespace War.Scene
{
	/// <summary>
	/// Contains information about scenes in collection
	/// </summary>
	[Serializable]
	public class SceneCollection : MonoBehaviour
	{
		/// <summary>
		/// The strem object name prefix.
		/// </summary>
		public  string prefixName = "stream";
	
		/// <summary>
		/// The scene name prefix.
		/// </summary>
		public string prefixScene = "Scene";
	
		/// <summary>
		/// The path of scenes.
		/// </summary>
		public string path = "Assets/WorldStreamer/SplitScenes/";
	
		/// <summary>
		/// The names of the scenes in collection.
		/// </summary>
		public string[] names;
	
		/// <summary>
		/// The is split by x.
		/// </summary>
		public bool xSplitIs = true;
		/// <summary>
		/// The is split by y.
		/// </summary>
		public bool ySplitIs = false;
		/// <summary>
		/// The is split by z.
		/// </summary>
		public bool zSplitIs = true;
	
		/// <summary>
		/// The size of the tile in x.
		/// </summary>
		public int xSize = 10;
		/// <summary>
		/// The size of the  tile in y.
		/// </summary>
		public int ySize = 10;
		/// <summary>
		/// The size of the  tile in z.
		/// </summary>
		public int zSize = 10;
	
		/// <summary>
		/// The x axis limits.
		/// </summary>
		public int xLimitsx = int.MaxValue;
		public int xLimitsy = int.MinValue;
	
	
		/// <summary>
		/// The y axis limits.
		/// </summary>
		public int yLimitsx = int.MaxValue;
		/// <summary>
		/// The y axis limits.
		/// </summary>
		public int yLimitsy = int.MinValue;
	
		/// <summary>
		/// The z axis limits.
		/// </summary>
		public int zLimitsx = int.MaxValue;
		/// <summary>
		/// The z axis limits.
		/// </summary>
		public int zLimitsy = int.MinValue;
	
		/// <summary>
		/// The collapsed for scene collection editor.
		/// </summary>
		[HideInInspector]
		public  bool collapsed = true;
	
		/// <summary>
		/// The layer number for scene collection editor.
		/// </summary>
		[HideInInspector]
		public  int layerNumber = 0;
	
		/// <summary>
		/// The color.
		/// </summary>
		public  Color color = Color.red;
	
	}
}

