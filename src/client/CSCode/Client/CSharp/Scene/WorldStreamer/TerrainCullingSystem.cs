using UnityEngine;
using System.Collections;

namespace War.Scene
{
	/// <summary>
	/// Terrain culling system.
	/// </summary>
	public class TerrainCullingSystem : MonoBehaviour
	{
		[Tooltip ("Max view distance is referred from camera to terrain center point")]
		/// <summary>
		/// The bounding distance - Max view distance is referred from camera to terrain center point.
		/// </summary>
		public float renderingDistance = 10000;
	
	
		float sphereSize = 0.5f;
	
		Terrain terrain;
	
		CullingGroup group;
		BoundingSphere[] spheres = new BoundingSphere[1000];
	
		Vector3 offsetVector;
		Vector3 offsetVectorUp;
		Camera mainCamera;
		int heightSphereNumber;
	
		[HideInInspector]
		public bool disableTrees = false;
	
		/// <summary>
		/// Start this instance, generates culling group and finds terrain.
		/// </summary>
		void Start ()
		{
			terrain = GetComponent<Terrain> ();
			if (terrain != null) {
	
				if (terrain.terrainData.size.x > terrain.terrainData.size.z)
					sphereSize = terrain.terrainData.size.x * 0.75f;
				else
					sphereSize = terrain.terrainData.size.z * 0.75f;
	
				offsetVector = new Vector3 (terrain.terrainData.size.x, 0, terrain.terrainData.size.z) * 0.5f;
	
				group = new CullingGroup ();
				group.targetCamera = Camera.main;
	
				heightSphereNumber = 2 * (int)(terrain.terrainData.size.y / (float)sphereSize);
				heightSphereNumber = Mathf.Max(1, heightSphereNumber);
				offsetVectorUp = new Vector3 (0, sphereSize * 0.5f, 0);
	
				for (int i = 0; i < heightSphereNumber; i++)
                {
					spheres [i] = new BoundingSphere (transform.position + offsetVector + i * offsetVectorUp, sphereSize);
				}
			
				group.SetBoundingSpheres (spheres);
				group.SetBoundingSphereCount (heightSphereNumber);
	
				group.onStateChanged = StateChangedMethod;
	
				group.SetBoundingDistances (new float[]{ renderingDistance });
	
				mainCamera = Camera.main;
				group.SetDistanceReferencePoint (Camera.main.transform);
	
	
				Invoke ("CheckVisibility", 0.1f);
			} else
				Debug.LogError ("TerrainCullingSystem: no terrain on game object " + gameObject.name);
		}
	
		void OnDrawGizmosSelected ()
		{
			Gizmos.color = Color.red;
		
			for (int i = 0; i < heightSphereNumber; i++)
            {
				Gizmos.DrawWireSphere (transform.position + offsetVector + i * offsetVectorUp, sphereSize);
			}
		}
	
		/// <summary>
		/// Checks visibility by hand
		/// </summary>
		void CheckVisibility ()
		{
			bool visible = false;
			for (int i = 0; i < heightSphereNumber; i++)
            {
				if (group.IsVisible (i))
                {
					visible = true;
					break;
				}
			}
			
			if (!visible)
            {
				terrain.drawHeightmap = false;
				if (disableTrees)
					terrain.drawTreesAndFoliage = false;
			}
	
		}
	
		/// <summary>
		/// sets object possition, and checks camera change;
		/// </summary>
		public void Update ()
		{
			
			for (int i = 0; i < heightSphereNumber; i++)
            {
				spheres [i] = new BoundingSphere (transform.position + offsetVector + i * offsetVectorUp, sphereSize);
			}
	
			if (mainCamera != Camera.main)
            {
				mainCamera = Camera.main;
				group.SetDistanceReferencePoint (Camera.main.transform);
			}
	
		}
	
		/// <summary>
		/// Event on cilling group change
		/// </summary>
		/// <param name="evt">Evt.</param>
		private void StateChangedMethod (CullingGroupEvent evt)
		{
			bool visible = false;
			for (int i = 0; i < heightSphereNumber; i++)
            {
				if (group.IsVisible (i))
                {
					visible = true;
					break;
				}
			}
	
			if (visible)
            {
				terrain.drawHeightmap = true;
				if (disableTrees)
					terrain.drawTreesAndFoliage = true;
			}
            else
            {
				terrain.drawHeightmap = false;
				if (disableTrees)
					terrain.drawTreesAndFoliage = false;
			}
		}
	
		/// <summary>
		/// Raises the disable event.
		/// </summary>
		void OnDisable ()
		{
			if (group != null)
            {
				group.Dispose ();
				group = null;
			}
		}
	}
}
