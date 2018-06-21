using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace War.Scene
{
    /// <summary>
    /// Sets Terrain neighbours.
    /// </summary>
    public class TerrainNeighbours : MonoBehaviour
    {
        public List<Terrain> terrainsToOmit;

        //[Tooltip ("If you use Floating Point fix system drag and drop world mover prefab from your scene hierarchy.")]
        /// <summary>
        /// The world mover.
        /// </summary>
        public WorldMover worldMover;
        public List<Terrain> _terrains = new List<Terrain>();
        Dictionary<int[], Terrain> _terrainDict = null;

        [Tooltip("Debug value, it gives info about starting position offset.")]
        /// <summary>
        /// The first position for terrain tile management.
        /// </summary>
        Vector2 firstPosition;

        int sizeX = 0;
        int sizeZ = 0;

        bool firstPositonSet = false;

        /// <summary>
        /// Start this instance and creates neighbours for scene terrains
        /// </summary>
        void Start()
        {
            if (worldMover == null)
                worldMover = StreamerManager.Instance.GetComponent<WorldMover>();
            CreateNeighbours();
        }

        /// <summary>
        /// Sets the neighbours for all terrains in scenes
        /// </summary>
        public void CreateNeighbours()
        {
            List<Terrain> _terrainsNew = new List<Terrain>();
            _terrainsNew.AddRange(Terrain.activeTerrains);

            foreach (var item in terrainsToOmit)
            {
                if (_terrainsNew.Contains(item))
                {
                    _terrainsNew.Remove(item);
                }
            }

            foreach (var item in _terrains)
            {
                if (_terrainsNew.Contains(item))
                {
                    _terrainsNew.Remove(item);
                }
            }

            if (_terrainDict == null)
                _terrainDict = new Dictionary<int[], Terrain>(new War.Common.IntArrayComparer());

            Dictionary<int[], Terrain> _terrainDictNew = new Dictionary<int[], Terrain>(new War.Common.IntArrayComparer());
            Dictionary<int[], Terrain> _terrainDictRob2 = new Dictionary<int[], Terrain>(new War.Common.IntArrayComparer());

            if (_terrainsNew.Count > 0)
            {
                if (!firstPositonSet)
                {
                    firstPositonSet = true;
                    firstPosition = new Vector2(_terrainsNew[0].transform.position.x, _terrainsNew[0].transform.position.z);

                    sizeX = (int)_terrainsNew[0].terrainData.size.x;
                    sizeZ = (int)_terrainsNew[0].terrainData.size.z;
                }

                foreach (var terrain in _terrainsNew)
                {
                    _terrains.Add(terrain);

                    Vector3 pos = terrain.transform.position;

                    if (worldMover != null)
                    	pos -= worldMover.currentMove;

                    int[] posTer = new int[] {
                        (int)(Mathf.RoundToInt ((pos.x - firstPosition.x) / sizeX)),
                        (int)(Mathf.RoundToInt ((pos.z - firstPosition.y) / sizeZ))
                    };


                    if (_terrainDict.ContainsKey(posTer))
                    {
                        _terrainDict[posTer] = terrain;
                    }
                    else
                        _terrainDict.Add(posTer, terrain);

                    _terrainDictNew.Add(posTer, terrain);
                }

                foreach (var item in _terrainDictNew)
                {
                    int[] posTer = item.Key;
                    Terrain top = null;
                    Terrain left = null;
                    Terrain right = null;
                    Terrain bottom = null;

                    int[] topPos = new int[] { posTer [0], posTer [1] + 1 };
                    int[] leftPos = new int[] { posTer [0] - 1, posTer[1] };
                    int[] posRight = new int[] { posTer [0] + 1, posTer[1] };
                    int[] posBottom = new int[] {  posTer [0], posTer[1] - 1 };

                    _terrainDict.TryGetValue(topPos, out top);
                    _terrainDict.TryGetValue(leftPos, out left);
                    _terrainDict.TryGetValue(posRight, out right);
                    _terrainDict.TryGetValue(posBottom, out bottom);
                    item.Value.SetNeighbors(left, top, right, bottom);

                    item.Value.Flush();

                    if (top != null && !_terrainDictRob2.ContainsKey(topPos))
                        _terrainDictRob2.Add(topPos, top);

                    if (left != null && !_terrainDictRob2.ContainsKey(leftPos))
                        _terrainDictRob2.Add(leftPos, left);

                    if (right != null && !_terrainDictRob2.ContainsKey(posRight))
                        _terrainDictRob2.Add(posRight, right);

                    if (bottom != null && !_terrainDictRob2.ContainsKey(posBottom))
                        _terrainDictRob2.Add(posBottom, bottom);
                }

                foreach (var item in _terrainDictRob2)
                {
                    int[] posTer = item.Key;
                    Terrain top = null;
                    Terrain left = null;
                    Terrain right = null;
                    Terrain bottom = null;

                    int[] topPos = new int[] { posTer[0], posTer[1] + 1 };
                    int[] leftPos = new int[] { posTer[0] - 1, posTer[1] };
                    int[] posRight = new int[] { posTer [0] + 1, posTer[1] };
                    int[] posBottom = new int[] { posTer [0], posTer[1] - 1 };

                    _terrainDict.TryGetValue(topPos, out top);
                    _terrainDict.TryGetValue(leftPos, out left);
                    _terrainDict.TryGetValue(posRight, out right);
                    _terrainDict.TryGetValue(posBottom, out bottom);

                    item.Value.SetNeighbors(left, top, right, bottom);
                    item.Value.Flush();
                }
            }
        }
    }
}
