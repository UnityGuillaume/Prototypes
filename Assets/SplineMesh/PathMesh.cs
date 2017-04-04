using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PathMesh : MonoBehaviour
{
    public Mesh originalMesh;
    public BezierPath path;

    protected MeshFilter m_Filter;
    protected MeshRenderer m_Renderer;

    private void Reset()
    {
        Init();  
    }

    public void Init()
    {
        path = new BezierPath(Vector3.zero, Vector3.forward * 1.0f);
        //path.AddPoint(Vector3.right * 10.0f);
    }

    public void RecomputeMesh()
    {
        if (originalMesh == null)
            return;

        m_Filter = GetComponent<MeshFilter>();
        m_Renderer = GetComponent<MeshRenderer>();

        List<Vector3> points = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int>[] triangles = new List<int>[originalMesh.subMeshCount];
        for (int i = 0; i < originalMesh.subMeshCount; ++i)
        {
            triangles[i] = new List<int>();
        }

        float pathSize = path.Length();
        float floatCount = pathSize / originalMesh.bounds.size.z;
        int intCount = Mathf.CeilToInt(floatCount);

        int count = Mathf.Max(1, intCount);
        float remainer = floatCount % 1;

        float scale = 1.0f + (remainer / count);
        float pathStep = 1.0f / count;

        Vector3[] originalVertices = originalMesh.vertices;
        Vector3[] originalNormals = originalMesh.normals;
        Vector2[] originalUV = originalMesh.uv;

        for (int k = 0; k < count; ++k)
        {
            float startingDist = (pathStep * k);
            int startingPointCount = points.Count;

            for (int i = 0; i < originalVertices.Length; ++i)
            {
                float dist = (originalVertices[i].z - originalMesh.bounds.min.z) / originalMesh.bounds.size.z;

                dist = startingDist + dist * pathStep;

                Vector3 pts = path.Evaluate(dist);
                Matrix4x4 basis = path.GetBasis(dist);

                Vector3 direction = originalVertices[i];
                direction.z = 0;

                Vector3 transformed = basis.MultiplyVector(direction);

                points.Add(pts + transformed);
                normals.Add(originalNormals[i]);
                uv.Add(originalUV[i]);
            }

            for (int i = 0; i < originalMesh.subMeshCount; ++i)
            {
                int[] localTri = originalMesh.GetTriangles(i);
                int startIdx = triangles[i].Count;
                for (int j = 0; j < localTri.Length; ++j)
                {
                    triangles[i].Add(startingPointCount + localTri[j]);
                }
            }
        }

        if(m_Filter.sharedMesh == null)
        {
            m_Filter.sharedMesh = new Mesh();
        }

        m_Filter.sharedMesh.Clear();
        m_Filter.sharedMesh.SetVertices(points);
        m_Filter.sharedMesh.SetNormals(normals);
        m_Filter.sharedMesh.SetUVs(0, uv);

        m_Filter.sharedMesh.subMeshCount = originalMesh.subMeshCount;
        for(int i = 0; i < originalMesh.subMeshCount; ++i)
        {
            m_Filter.sharedMesh.SetTriangles(triangles[i], i);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PathMesh))]
public class MeshPathEditor : Editor
{
    PathMesh m_Target;
    int selectedPts = -1;
    float t = 0;

    private void OnEnable()
    {
        m_Target = target as PathMesh;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (selectedPts != -1)
        {
            m_Target.path.points[selectedPts] = EditorGUILayout.Vector3Field("Current Selected Point", m_Target.path.points[selectedPts]);
        }

        EditorGUILayout.LabelField(m_Target.path.Length().ToString());

        EditorGUI.BeginChangeCheck();
        t = EditorGUILayout.Slider(t, 0.0f, 1.0f);
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Add Point"))
        {
            Vector3 pos = m_Target.path.Evaluate(1.0f);
            Matrix4x4 mat = m_Target.path.GetBasis(1.0f);
            Vector3 forward = mat.GetColumn(2);

            m_Target.path.AddPoint(pos + forward * 2.0f);
        }


        if (EditorGUI.EndChangeCheck())
        {
            m_Target.RecomputeMesh();
        }
    }

    private void OnSceneGUI()
    {
        for (int i = 0; i < m_Target.path.curves.Count; ++i)
        {
            Handles.DrawBezier(
                m_Target.transform.TransformPoint(m_Target.path.curves[i].startPoint),
                m_Target.transform.TransformPoint(m_Target.path.curves[i].endPoint),
                m_Target.transform.TransformPoint(m_Target.path.curves[i].startControlPoint),
                 m_Target.transform.TransformPoint(m_Target.path.curves[i].endControlPoint),
                Color.white, null, 4.0f);

            Handles.color = Color.red;
            Handles.DrawLine(m_Target.transform.TransformPoint(m_Target.path.curves[i].startPoint), m_Target.transform.TransformPoint(m_Target.path.curves[i].startControlPoint));
            Handles.DrawLine(m_Target.transform.TransformPoint(m_Target.path.curves[i].endPoint), m_Target.transform.TransformPoint(m_Target.path.curves[i].endControlPoint));
        }


        for (int i = 1; i < m_Target.path.points.Count; ++i)
        {
            Vector3 worldPts = m_Target.transform.TransformPoint(m_Target.path.points[i]);

            if (selectedPts == i)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 p = Handles.PositionHandle(worldPts, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_Target, "Move Control Point");
                    m_Target.path.points[i] = m_Target.transform.InverseTransformPoint(p);
                    m_Target.RecomputeMesh();
                    Repaint();
                }
            }
            else
            {
                if(Handles.Button(worldPts, Quaternion.identity, HandleUtility.GetHandleSize(worldPts) * 0.1f, HandleUtility.GetHandleSize(worldPts) * 0.1f, Handles.SphereCap))
                {
                    selectedPts = i;
                    Repaint();
                }
            }
        }

        Handles.color = Color.green;
        Vector3 pts = m_Target.path.Evaluate(t);
        Handles.SphereCap(0, m_Target.transform.TransformPoint(pts), Quaternion.identity, 0.3f);

        Matrix4x4 basis = m_Target.path.GetBasis(t);

        Handles.color = Color.blue;
        Handles.DrawLine(m_Target.transform.TransformPoint(pts), m_Target.transform.TransformPoint(pts + new Vector3(basis.GetColumn(0).x, basis.GetColumn(0).y, basis.GetColumn(0).z)));
        Handles.DrawLine(m_Target.transform.TransformPoint(pts), m_Target.transform.TransformPoint(pts + new Vector3(basis.GetColumn(1).x, basis.GetColumn(1).y, basis.GetColumn(1).z)));
        Handles.DrawLine(m_Target.transform.TransformPoint(pts), m_Target.transform.TransformPoint(pts + new Vector3(basis.GetColumn(2).x, basis.GetColumn(2).y, basis.GetColumn(2).z)));
    }
}
#endif