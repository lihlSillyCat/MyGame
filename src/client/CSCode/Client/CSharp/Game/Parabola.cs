using UnityEngine;
using System.Collections;

public class Parabola : MonoBehaviour
{
    private LineRenderer line = null;

    // Use this for initialization
    void Start()
    {
        line = this.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (line != null && line.positionCount >= 2 && this.gameObject.activeSelf)
        {
            Material mat = line.material;
            Vector3 startPos = line.transform.TransformPoint(line.GetPosition(0));
            startPos.y = 0.0f;
            Vector3 endPos = line.transform.TransformPoint(line.GetPosition(line.positionCount - 1));
            endPos.y = 0.0f;

            Vector3 center = (endPos + startPos) * 0.5f;
            Vector3 dir = endPos - startPos;
            float len = dir.magnitude * 0.5f;
            dir.Normalize();

            float width = line.startWidth * 0.5f;

            mat.SetVector("_Center", center);
            mat.SetVector("_Direction", dir);
            mat.SetFloat("_HalfLength", len);
            mat.SetFloat("_HalfWidth", width);
        }
    }
}
