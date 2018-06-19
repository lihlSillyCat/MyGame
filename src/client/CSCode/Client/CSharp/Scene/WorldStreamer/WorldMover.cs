using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace War.Scene
{/// <summary>
 /// World mover - movesw the world when moving out of chosen range.
 /// </summary>
    public class WorldMover : MonoBehaviour
    {
        /// <summary>
        /// The WORLD MOVER TAG.
        /// </summary>
        public static string WORLDMOVERTAG = "WorldMover";

        [Tooltip("Frequency distance of world position restarting, distance in is grid elements.")]
        /// <summary>
        /// The x tile range based on main streamer.
        /// </summary>
        public float xTileRange = 2;

        [Tooltip("Frequency distance of world position restarting, distance in is grid elements.")]
        /// <summary>
        /// The y tile range  based on main streamer.
        /// </summary>
        public float yTileRange = 2;

        [Tooltip("Frequency distance of world position restarting, distance in is grid elements.")]
        /// <summary>
        /// The z tile range  based on main streamer.
        /// </summary>
        public float zTileRange = 2;

        [HideInInspector]
        /// <summary>
        /// The x current tile move.
        /// </summary>
        public float xCurrentTile = 0;

        [HideInInspector]
        /// <summary>
        /// The y current tile move.
        /// </summary>
        public float yCurrentTile = 0;

        [HideInInspector]
        /// <summary>
        /// The z current tile move.
        /// </summary>
        public float zCurrentTile = 0;


        [Tooltip("Drag and drop here, your _Streamer_Major prefab from scene hierarchy.")]
        /// <summary>
        /// The streamer main for checking range.
        /// </summary>
        public Streamer streamerMajor;


        [Tooltip("Drag and drop here, your all _Streamer_Minors prefabs from scene hierarchy.")]
        /// <summary>
        /// The additional streamers to move tiles.
        /// </summary>
        public Streamer[] streamerMinors;

        [Tooltip("Differences between real  and restarted player position. Useful in AI and network communications.")]
        /// <summary>
        /// The current move vector.
        /// </summary>
        public Vector3 currentMove = Vector3.zero;

        /// <summary>
        /// The objects to move with tiles.
        /// </summary>
        [HideInInspector]
        public HashSet<Transform>
            objectsToMove = new HashSet<Transform>();

        [Tooltip("Debug value used for client-server communication it's position without floating point fix and looping")]
        /// <summary>
        /// Debug value used for client-server communication it's position without floating point fix and looping
        /// </summary>
        public Vector3 playerPositionMovedLooped;

        private Vector3 worldSize;

        /// <summary>
        /// Start this instance and sets main streamer field for world mover.
        /// </summary>
        public void Start()
        {
            streamerMajor.worldMover = this;
            List<Streamer> streamersTemp = new List<Streamer>();
            streamersTemp.AddRange(streamerMinors);
            streamersTemp.Remove(streamerMajor);
            streamerMinors = streamersTemp.ToArray();

            worldSize = new Vector3(streamerMajor.sceneCollection.xSize * (streamerMajor.sceneCollection.xLimitsy - streamerMajor.sceneCollection.xLimitsx + 1),
                streamerMajor.sceneCollection.ySize * (streamerMajor.sceneCollection.yLimitsy - streamerMajor.sceneCollection.yLimitsx + 1),
                streamerMajor.sceneCollection.zSize * (streamerMajor.sceneCollection.zLimitsy - streamerMajor.sceneCollection.zLimitsx + 1));
        }

        public void Update()
        {
            if (streamerMajor.player != null)
            {
                playerPositionMovedLooped = streamerMajor.player.position - currentMove;
            }

            if (streamerMajor.looping)
            {

                //Debug.Log (playerPositionMovedLooped.z + " " + Mathf.Abs (streamerMajor.sceneCollection.zSize * streamerMajor.sceneCollection.zLimitsx) + " " + worldSize.z);

                playerPositionMovedLooped = new Vector3(worldSize.x != 0 ? modf((playerPositionMovedLooped.x + Mathf.Abs(streamerMajor.sceneCollection.xSize * streamerMajor.sceneCollection.xLimitsx)), worldSize.x) + streamerMajor.sceneCollection.xSize * streamerMajor.sceneCollection.xLimitsx : playerPositionMovedLooped.x,
                    worldSize.y != 0 ? modf((playerPositionMovedLooped.y + Mathf.Abs(streamerMajor.sceneCollection.ySize * streamerMajor.sceneCollection.yLimitsx)), worldSize.y) + streamerMajor.sceneCollection.ySize * streamerMajor.sceneCollection.yLimitsx : playerPositionMovedLooped.y,
                    worldSize.z != 0 ? modf((playerPositionMovedLooped.z + Mathf.Abs(streamerMajor.sceneCollection.zSize * streamerMajor.sceneCollection.zLimitsx)), worldSize.z) + streamerMajor.sceneCollection.zSize * streamerMajor.sceneCollection.zLimitsx : playerPositionMovedLooped.z);


            }
        }

        /// <summary>
        /// Checks the mover distance.
        /// </summary>
        /// <param name="xPosCurrent">X position current in tiles.</param>
        /// <param name="yPosCurrent">Y position current in tiles.</param>
        /// <param name="zPosCurrent">Z position current in tiles.</param>
        public void CheckMoverDistance(int xPosCurrent, int yPosCurrent, int zPosCurrent)
        {

            if (Mathf.Abs(xPosCurrent - xCurrentTile) > xTileRange || Mathf.Abs(yPosCurrent - yCurrentTile) > yTileRange || Mathf.Abs(zPosCurrent - zCurrentTile) > zTileRange)
            {

                MoveWorld(xPosCurrent, yPosCurrent, zPosCurrent);

            }
        }

        /// <summary>
        /// Moves the world.
        /// </summary>
        /// <param name="xPosCurrent">X position current in tiles.</param>
        /// <param name="yPosCurrent">Y position current in tiles.</param>
        /// <param name="zPosCurrent">Z position current in tiles.</param>
        void MoveWorld(int xPosCurrent, int yPosCurrent, int zPosCurrent)
        {

            Vector3 moveVector = new Vector3((xPosCurrent - xCurrentTile) * streamerMajor.sceneCollection.xSize, (yPosCurrent - yCurrentTile) * streamerMajor.sceneCollection.ySize, (zPosCurrent - zCurrentTile) * streamerMajor.sceneCollection.zSize);

            currentMove -= moveVector;

            foreach (var item in streamerMajor.loadedScenes)
            {
                if (item.loaded && item.sceneGo != null)
                    item.sceneGo.transform.position -= moveVector;
            }

            foreach (var item in objectsToMove)
            {
                if (item != null && item.parent == null)
                {
                    item.position -= moveVector;
                }
            }

            xCurrentTile = xPosCurrent;
            yCurrentTile = yPosCurrent;
            zCurrentTile = zPosCurrent;

            streamerMajor.currentMove = currentMove;

            foreach (var item in streamerMinors)
            {

                item.currentMove = currentMove;

                foreach (var scene in item.loadedScenes)
                {
                    if (scene.loaded && scene.sceneGo != null)
                        scene.sceneGo.transform.position -= moveVector;
                }
            }

        }

        /// <summary>
        /// Moves the object.
        /// </summary>
        /// <param name="objectTransform">Object transform.</param>
        public void MoveObject(Transform objectTransform)
        {
            objectTransform.position += currentMove;
        }

        /// <summary>
        /// Adds the object to move.
        /// </summary>
        /// <param name="objectToMove">Object to move.</param>
        public void AddObjectToMove(Transform objectToMove)
        {
            transform.position += currentMove;
            objectsToMove.Add(objectToMove);
        }

        public void RemoveObjectToMove(Transform objectToMove)
        {
            objectsToMove.Remove(objectToMove);
        }

        public bool HasObjectToMove(Transform objectToMove)
        {
            return objectsToMove.Contains(objectToMove);
        }

        float modf(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}