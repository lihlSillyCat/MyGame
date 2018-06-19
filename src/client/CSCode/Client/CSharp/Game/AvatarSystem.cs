//#define TEST_AVATAR

using System.Collections.Generic;
using UnityEngine;

namespace War.Game
{
    class ModelSkeleton
    {
        private Dictionary<string, Transform> m_bones = new Dictionary<string, Transform>();

        public ModelSkeleton(GameObject model, string rootBoneName)
        {
            Transform rootBone = model.transform.Find(rootBoneName);
            if (rootBone == null)
            {
                Debug.LogErrorFormat("找不到模型骨骼根节点 {0} {1}.", model.name, rootBoneName);
                return;
            }

            Transform[] bones = rootBone.GetComponentsInChildren<Transform>();
            for (int i = 0; i < bones.Length; ++i)
            {
                var bone = bones[i];
                if (m_bones.ContainsKey(bone.name))
                {
                    Debug.LogErrorFormat("模型骨骼重名 {0} {1}.", model.name, bone.name);
                }
                else
                {
                    m_bones.Add(bone.name, bone);
                }
            }
        }

        public Transform GetBone(string name)
        {
            Transform bone;
            m_bones.TryGetValue(name, out bone);
            return bone;
        }
    }

    public class AvatarSystem : MonoBehaviour
    {
        [System.Serializable]
        public class AvatarPart
        {
            public string name = "Default";

            [HideInInspector]
            public int modelID = -1;

            [HideInInspector]
            public bool visible = false;

            [Tooltip("默认模型，按LOD排列")]
            public SkinnedMeshRenderer[] smrs = null;

            public AvatarPart(int LODLevel)
            {
                smrs = new SkinnedMeshRenderer[LODLevel];
            }

            public void SetVisible(bool v)
            {
                if (v == visible)
                    return;

                visible = v;
                for (int i = 0; i < smrs.Length; ++i)
                {
                    smrs[i].gameObject.SetActive(v);
                }
            }

            public void SetPartColor(Color color)
            {
                if (modelID == -1)
                {
                    return;
                }

                for (int i = 0; i < smrs.Length; ++i)
                {
                    smrs[i].material.color = color;
                }
            }
        }

        static readonly string[] LOD_PREFIXS = { "LOD0", "LOD1", "LOD2", "LOD3", };

        private ModelSkeleton m_modelSkeleton;
        private Dictionary<string, AvatarPart> m_avatarPartSmrs = new Dictionary<string, AvatarPart>();
        private LODGroup m_LODGroup;
        private int m_maxLODLevel = 3;

        [Tooltip("骨骼跟节点名称")]
        public string m_rootBoneName = "Main";

        public List<AvatarPart> m_defaultParts;

#if TEST_AVATAR
        public List<GameObject> m_partPrefabs;
        private void Start()
        {
            if (m_partPrefabs != null)
            {
                var e = m_partPrefabs.GetEnumerator();
                while(e.MoveNext())
                {
                    if (e.Current != null)
                    {
                        var go = GameObject.Instantiate(e.Current);
                        SetPart(go.name, 0, go);
                        GameObject.Destroy(go);
                    }
                }
                e.Dispose();
            }
        
        }
#endif

        private void Awake()
        {
            m_modelSkeleton = new ModelSkeleton(this.gameObject, m_rootBoneName);
            m_LODGroup = this.gameObject.GetComponent<LODGroup>();
            if (m_LODGroup != null)
            {
                m_maxLODLevel = m_LODGroup.GetLODs().Length;
            }
            else
            {
                m_maxLODLevel = 1;
            }

            if (m_defaultParts != null)
            {
                if (m_defaultParts.Count > 0)
                {
                    var e = m_defaultParts.GetEnumerator();
                    while (e.MoveNext())
                    {
                        CreateDefaultPart(e.Current);
                    }
                    e.Dispose();
                    RefreshLODGroup();
                }
            }
        }

        private AvatarPart GetAvatarPart(string name)
        {
            AvatarPart part;
            m_avatarPartSmrs.TryGetValue(name, out part);
            return part;
        }

        private void CreateDefaultPart(AvatarPart defaultPart)
        {
            if (defaultPart.smrs.Length < m_maxLODLevel)
            {
                SkinnedMeshRenderer[] newSmrs = new SkinnedMeshRenderer[m_maxLODLevel];
                for (int i = 0; i < m_maxLODLevel; ++i)
                {
                    if (i < defaultPart.smrs.Length)
                    {
                        newSmrs[i] = defaultPart.smrs[i];
                    }
                    else
                    {
                        GameObject go = new GameObject();
                        go.name = string.Format("{0}_LOD{1}", defaultPart.name, i);
                        go.transform.position = Vector3.zero;
                        go.transform.rotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;
                        go.transform.SetParent(this.gameObject.transform, false);
                        newSmrs[i] = go.AddComponent<SkinnedMeshRenderer>();
                    }
                }
                defaultPart.smrs = newSmrs;
            }
            defaultPart.modelID = -1;
            m_avatarPartSmrs.Add(defaultPart.name, defaultPart);
        }

        private AvatarPart CreatePart(string name)
        {
            AvatarPart part = new AvatarPart(m_maxLODLevel);
            for (int i = 0; i < m_maxLODLevel; ++i)
            {
                GameObject go = new GameObject();
                go.name = string.Format("{0}_LOD{1}", name, i);
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.transform.SetParent(this.gameObject.transform, false);
                part.smrs[i] = go.AddComponent<SkinnedMeshRenderer>();
            }
            m_avatarPartSmrs.Add(name, part);
            return part;
        }

        private Transform[] CollectBones(SkinnedMeshRenderer srcSmr)
        {
            List<Transform> bones = new List<Transform>();
            Transform[] allUsedBones = srcSmr.bones;
            for (int i = 0; i < allUsedBones.Length; ++i)
            {
                Transform bone = m_modelSkeleton.GetBone(allUsedBones[i].name);
                if (bone != null)
                {
                    bones.Add(bone);
                }
                else
                {
                    Debug.LogErrorFormat("在主模型找不到部件所需要的骨骼 {0}", allUsedBones[i].name);
                }
            }
            return bones.ToArray();
        }

        private Transform FindPartRootBone(SkinnedMeshRenderer srcSmr)
        {
            if (srcSmr.rootBone != null)
            {
                Transform bone = m_modelSkeleton.GetBone(srcSmr.rootBone.name);
                return bone;
            }
            return null;
        }

        private SkinnedMeshRenderer FindLODSmr(SkinnedMeshRenderer[] srcSmrs, int LODLevel)
        {
            if (LODLevel >= srcSmrs.Length)
            {
                return null;
            }

            for (int i = 0; i < srcSmrs.Length; ++i)
            {
                SkinnedMeshRenderer smr = srcSmrs[i];
                if (smr.name.LastIndexOf(LOD_PREFIXS[LODLevel], System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return smr;
                }
            }

            return null;
        }

        private void RefreshLODGroup()
        {
            if (m_LODGroup == null)
            {
                return;
            }

            LOD[] L = m_LODGroup.GetLODs();
            for(int i = 0; i < L.Length; ++i)
            {
                L[i].renderers = new Renderer[m_avatarPartSmrs.Count];
            }
            int idx = 0;
            var e = m_avatarPartSmrs.GetEnumerator();
            while(e.MoveNext())
            {
                AvatarPart part = e.Current.Value;
                for (int i = 0; i < L.Length; ++i)
                {
                    L[i].renderers[idx] = part.smrs[i];
                }
                ++idx;
            }
            m_LODGroup.SetLODs(L);
            e.Dispose();
        }

        public void SetPart(string name, int modelID, GameObject partModel)
        {
            SkinnedMeshRenderer[] srcSmrs = partModel.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (srcSmrs.Length == 0)
            {
                return;
            }

            bool refreshLODGroup = false;
            var part = GetAvatarPart(name);
            if (part == null)
            {
                part = CreatePart(name);
                refreshLODGroup = true;
            }

            //if (part.modelID == modelID)
            //{
            //    part.SetVisible(true);
            //    partModel.transform.SetParent(this.gameObject.transform, false);
            //    partModel.SetActive(false);
            //    return;
            //}

            part.modelID = modelID;

            int currLODLevel = 0;
            for (; currLODLevel < m_maxLODLevel; ++currLODLevel)
            {
                SkinnedMeshRenderer srcSmr = FindLODSmr(srcSmrs, currLODLevel);
                if (srcSmr == null)
                {
                    break;
                }
                SkinnedMeshRenderer targetSmr = part.smrs[currLODLevel];
                targetSmr.sharedMaterial = srcSmr.sharedMaterial;
                targetSmr.sharedMesh = srcSmr.sharedMesh;
                targetSmr.bones = CollectBones(srcSmr);
                targetSmr.rootBone = FindPartRootBone(srcSmr);
                targetSmr.receiveShadows = srcSmr.receiveShadows;
                targetSmr.shadowCastingMode = srcSmr.shadowCastingMode;
            }

            if (currLODLevel < m_maxLODLevel)
            {
                SkinnedMeshRenderer fill = null;
                if (currLODLevel == 0)
                {
                    fill = srcSmrs[0];
                }
                else
                {
                    fill = part.smrs[currLODLevel - 1];
                }
                Transform[] bones = CollectBones(fill);
                Transform rootBone = FindPartRootBone(fill);
                for (; currLODLevel < m_maxLODLevel; ++currLODLevel)
                {
                    SkinnedMeshRenderer targetSmr = part.smrs[currLODLevel];
                    targetSmr.sharedMaterial = fill.sharedMaterial;
                    targetSmr.sharedMesh = fill.sharedMesh;
                    targetSmr.bones = bones;
                    targetSmr.rootBone = rootBone;
                    targetSmr.receiveShadows = fill.receiveShadows;
                    targetSmr.shadowCastingMode = fill.shadowCastingMode;
                }
            }

            if (refreshLODGroup)
            {
                RefreshLODGroup();
            }

            part.SetVisible(true);
            partModel.transform.SetParent(this.gameObject.transform, false);
            partModel.SetActive(false);
        }

        public void ShowPart(string name)
        {
            var part = GetAvatarPart(name);
            if (part != null)
            {
                if (part.modelID == -1)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("无效的avatar模型");
#endif
                    return;
                }
                part.SetVisible(true);
            }
        }

        public void HidePart(string name)
        {
            var part = GetAvatarPart(name);
            if (part != null)
            {
                if (part.modelID == -1)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("无效的avatar模型");
#endif
                    return;
                }
                part.SetVisible(false);
            }
        }

        public void ClearPart(string name)
        {
            var part = GetAvatarPart(name);
            if (part != null)
            {
                part.SetVisible(false);
                part.modelID = -1;
            }
        }

        public void SetPartColor(string name, Color color)
        {
            var part = GetAvatarPart(name);
            if (part != null)
            {
                part.SetPartColor(color);
            }
        }

        public GameObject[] GetPartGameObjects(string name)
        {
            var part = GetAvatarPart(name);
            if (part != null)
            {
                GameObject[] gos = new GameObject[part.smrs.Length];
                for (int i = 0; i < gos.Length; ++i)
                {
                    gos[i] = part.smrs[i].gameObject;
                }
                return gos;
            }
            return null;
        }
    }

}
