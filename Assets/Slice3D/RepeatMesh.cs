using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RepeatMesh : MonoBehaviour
{
    public Mesh originalMesh;

    public float xPlaneLeft = 1.0f;
    public float xPlaneRight = 1.0f;
    public float yPlaneUp = 1.0f;
    public float yPlaneDown = 1.0f;
    public float zPlaneFront = 1.0f;
    public float zPlaneBack = 1.0f;

    public Vector3 scale = Vector3.one;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void RecomputeMesh()
    {
        if (originalMesh == null)
            return;

        MeshRenderer rend = GetComponent<MeshRenderer>();
        MeshFilter filter = GetComponent<MeshFilter>();


        Plane xLeft = new Plane(Vector3.right, xPlaneLeft);
        Plane xRight = new Plane(Vector3.left, xPlaneRight);
        Plane yUp = new Plane(Vector3.down, yPlaneUp);
        Plane yDown = new Plane(Vector3.up, yPlaneDown);
        Plane zFront = new Plane(Vector3.back, zPlaneFront);
        Plane zBack = new Plane(Vector3.forward, zPlaneBack);

        int xCount = Mathf.Max(1, Mathf.FloorToInt(scale.x));

        float strecthValue = (scale.x - xCount);

        List<Vector3> resultPts = new List<Vector3>();
        List<Vector2> resultUVs = new List<Vector2>();
        List<Vector3> resultNormals = new List<Vector3>();
        List<Color32> resultColor = new List<Color32>();

        for (int i = 0; i < originalMesh.vertices.Length; i++)
        {
            Vector3 vert = originalMesh.vertices[i];

            Vector3 resultingVert = vert;
            Vector3 resultingUV = originalMesh.uv[i];

            Color col = Color.white;

            Vector3 bitangent = Vector3.Cross(originalMesh.tangents[i], originalMesh.normals[i]);

            //x planes
            bool left = xLeft.GetSide(vert);
            bool right = xRight.GetSide(vert);

            //Debug.Log(vert + "xLeft plane : " + xLeft.normal + " -- " + xLeft.distance + " L : " + left + " R : " + right);

            if (left && right)
            {//if inside the 2 plane, scale
                resultingVert.x *= scale.x;

                resultingUV.x *= scale.x * Vector3.Dot(bitangent, Vector3.right);
                resultingUV.y *= scale.x * Vector3.Dot(originalMesh.tangents[i], Vector3.right);
            }
            else if(!left)
            {//outside, just push vertex 
                float vertDist = vert.x - xLeft.distance * -Mathf.Sign(xLeft.normal.x);
                resultingVert.x = -Mathf.Sign(xLeft.normal.x) * xLeft.distance * scale.x + vertDist;
            }
            else
            {
                float vertDist = vert.x - xRight.distance * -Mathf.Sign(xRight.normal.x);
                resultingVert.x = -Mathf.Sign(xRight.normal.x) * xRight.distance * scale.x + vertDist;
            }

            //y planes
            bool up = yUp.GetSide(vert);
            bool down = yDown.GetSide(vert);
            if (up && down)
            {//if inside the 2 plane, scale
                resultingVert.y *= scale.y;
                //resultingUV.x *= scale.y * Vector3.Dot(originalMesh.tangents[i], Vector3.up);
                //resultingUV.y *= scale.y * Vector3.Dot(bitangent, Vector3.up);
            }
            else if (!up)
            {//outside, just push vertex 
                float vertDist = vert.y - yUp.distance * -Mathf.Sign(yUp.normal.y);
                resultingVert.y = -Mathf.Sign(yUp.normal.y) * yUp.distance * scale.y + vertDist;
            }
            else
            {
                float vertDist = vert.y - yDown.distance * -Mathf.Sign(yDown.normal.y);
                resultingVert.y = -Mathf.Sign(yDown.normal.y) * yDown.distance * scale.y + vertDist;
            }

            //z planes
            bool front = zFront.GetSide(vert);
            bool back = zBack.GetSide(vert);
            if (front && back)
            {//if inside the 2 plane, scale
                resultingVert.z *= scale.z;
                //resultingUV.x *= scale.z * Vector3.Dot(originalMesh.tangents[i], Vector3.forward);
                //resultingUV.y *= scale.z * Vector3.Dot(bitangent, Vector3.forward);
            }
            else if (!front)
            {//outside, just push vertex 
                float vertDist = vert.z - zFront.distance * -Mathf.Sign(zFront.normal.z);
                resultingVert.z = -Mathf.Sign(zFront.normal.z) * zFront.distance * scale.z + vertDist;
                col = Color.blue;
            }
            else
            {
                float vertDist = vert.z - zBack.distance * -Mathf.Sign(zBack.normal.z);
                resultingVert.z = -Mathf.Sign(zBack.normal.z) * zBack.distance * scale.z + vertDist;
                col = Color.green;
            }

            resultPts.Add(resultingVert);
            resultUVs.Add(resultingUV);
            resultNormals.Add(originalMesh.normals[i]);
            resultColor.Add(col);
        }

        if(filter.sharedMesh == null)
        {
            filter.sharedMesh = new Mesh();
        }

        filter.sharedMesh.Clear();
        filter.sharedMesh.SetVertices(resultPts);
        filter.sharedMesh.SetNormals(resultNormals);
        filter.sharedMesh.SetUVs(0, resultUVs);
        filter.sharedMesh.SetColors(resultColor);

        filter.sharedMesh.subMeshCount = originalMesh.subMeshCount;
        for (int i = 0; i < originalMesh.subMeshCount; ++i)
        {
            filter.sharedMesh.SetTriangles(originalMesh.GetTriangles(i), i);
        }

        filter.sharedMesh.RecalculateBounds();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RepeatMesh))]
public class RepeatMeshEditor : Editor
{
    RepeatMesh m_Target;

    private void OnEnable()
    {
        m_Target = target as RepeatMesh;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        m_Target.originalMesh = EditorGUILayout.ObjectField("Mesh", m_Target.originalMesh, typeof(Mesh), false) as Mesh;
        m_Target.scale = EditorGUILayout.Vector3Field("Scale : ", m_Target.scale);

        EditorGUILayout.LabelField("X Plane");
        EditorGUILayout.BeginHorizontal();
        m_Target.xPlaneLeft = EditorGUILayout.FloatField("Left", m_Target.xPlaneLeft);
        m_Target.xPlaneRight = EditorGUILayout.FloatField("Right", m_Target.xPlaneRight);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Y Plane");
        EditorGUILayout.BeginHorizontal();
        m_Target.yPlaneUp = EditorGUILayout.FloatField("Up", m_Target.yPlaneUp);
        m_Target.yPlaneDown = EditorGUILayout.FloatField("Down", m_Target.yPlaneDown);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Z Plane");
        EditorGUILayout.BeginHorizontal();
        m_Target.zPlaneFront = EditorGUILayout.FloatField("Forward", m_Target.zPlaneFront);
        m_Target.zPlaneBack = EditorGUILayout.FloatField("Back", m_Target.zPlaneBack);
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
            m_Target.RecomputeMesh();
        }
    }

    private void OnSceneGUI()
    {
        Color transparent = new Color(0, 0, 0, 0);
        Vector3[] xLeftVertices =
        {
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.up * m_Target.yPlaneUp + Vector3.forward * m_Target.zPlaneFront + Vector3.left * m_Target.xPlaneLeft, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.down * m_Target.yPlaneDown + Vector3.forward * m_Target.zPlaneFront + Vector3.left * m_Target.xPlaneLeft, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.down * m_Target.yPlaneDown + Vector3.back * m_Target.zPlaneBack + Vector3.left * m_Target.xPlaneLeft, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.up * m_Target.yPlaneUp + Vector3.back * m_Target.zPlaneBack + Vector3.left * m_Target.xPlaneLeft, m_Target.scale))
        };

        Handles.DrawSolidRectangleWithOutline(xLeftVertices, transparent, Color.red);

        Vector3[] xRightVertices =
       {
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.up * m_Target.yPlaneUp + Vector3.forward * m_Target.zPlaneFront + Vector3.right * m_Target.xPlaneRight, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.down * m_Target.yPlaneDown + Vector3.forward * m_Target.zPlaneFront + Vector3.right * m_Target.xPlaneRight, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.down * m_Target.yPlaneDown + Vector3.back * m_Target.zPlaneBack + Vector3.right * m_Target.xPlaneRight, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.up * m_Target.yPlaneUp + Vector3.back * m_Target.zPlaneBack + Vector3.right * m_Target.xPlaneRight, m_Target.scale))
        };

        Handles.DrawSolidRectangleWithOutline(xRightVertices, transparent, Color.red);

        Vector3[] yUpVertices =
        {
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.right * m_Target.xPlaneRight + Vector3.forward * m_Target.zPlaneFront + Vector3.up * m_Target.yPlaneUp, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.left * m_Target.xPlaneLeft + Vector3.forward * m_Target.zPlaneFront + Vector3.up * m_Target.yPlaneUp, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.left * m_Target.xPlaneLeft + Vector3.back * m_Target.zPlaneBack + Vector3.up * m_Target.yPlaneUp, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.right * m_Target.xPlaneRight + Vector3.back * m_Target.zPlaneBack + Vector3.up * m_Target.yPlaneUp, m_Target.scale))
        };

        Handles.DrawSolidRectangleWithOutline(yUpVertices, transparent, Color.green);

        Vector3[] yDownVertices =
        {
             m_Target.transform.TransformPoint(Vector3.Scale(Vector3.right * m_Target.xPlaneRight + Vector3.forward * m_Target.zPlaneFront + Vector3.down * m_Target.yPlaneDown, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.left * m_Target.xPlaneLeft + Vector3.forward * m_Target.zPlaneFront + Vector3.down * m_Target.yPlaneDown, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.left * m_Target.xPlaneLeft + Vector3.back * m_Target.zPlaneBack + Vector3.down * m_Target.yPlaneDown, m_Target.scale)),
            m_Target.transform.TransformPoint(Vector3.Scale(Vector3.right * m_Target.xPlaneRight + Vector3.back * m_Target.zPlaneBack + Vector3.down * m_Target.yPlaneDown, m_Target.scale))
        };

        Handles.DrawSolidRectangleWithOutline(yDownVertices, transparent, Color.green);
    }
}
#endif