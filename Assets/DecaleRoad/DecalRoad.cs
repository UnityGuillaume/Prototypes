using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class DecalRoad : MonoBehaviour
{
    static public CommandBuffer commandBuffer { get { if (s_CommandBuffer == null) s_CommandBuffer = new CommandBuffer(); return s_CommandBuffer; } }

    static protected CommandBuffer s_CommandBuffer = null;
    static List<DecalRoad> s_DecalRoads = new List<DecalRoad>();

    protected MeshRenderer m_Renderer;
    protected MeshFilter m_Filter;

    void Awake()
    {
        if(s_CommandBuffer == null)
            s_CommandBuffer = new CommandBuffer();
    }

    private void OnEnable()
    {
        s_DecalRoads.Add(this);
        m_Renderer = GetComponent<MeshRenderer>();
        m_Renderer.enabled = false;
        m_Filter = GetComponent<MeshFilter>();
        UpdateCommand();
    }

    private void OnDisable()
    {
        s_DecalRoads.Remove(this);
        UpdateCommand();
    }

    private void Update()
    {

    }

    static public void UpdateCommand()
    {
        s_CommandBuffer.Clear();

        var normalsID = Shader.PropertyToID("_NormalsCopy");
        s_CommandBuffer.GetTemporaryRT(normalsID, -1, -1);
        s_CommandBuffer.Blit(BuiltinRenderTextureType.GBuffer2, normalsID);

        RenderTargetIdentifier[] mrt = { BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.GBuffer2 };
        s_CommandBuffer.SetRenderTarget(mrt, BuiltinRenderTextureType.CameraTarget);

        for(int i = 0; i < s_DecalRoads.Count; ++i)
        {
            s_CommandBuffer.DrawMesh(s_DecalRoads[i].m_Filter.sharedMesh, s_DecalRoads[i].transform.localToWorldMatrix, s_DecalRoads[i].m_Renderer.sharedMaterial);
        }

        s_CommandBuffer.ReleaseTemporaryRT(normalsID);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DecalRoad))]
public class DecalRoadEditor : Editor
{
    DecalRoad m_Decal;

    private void OnEnable()
    {
        m_Decal = target as DecalRoad;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck() || m_Decal.transform.hasChanged)
        {
            DecalRoad.UpdateCommand();
            SceneView.RepaintAll();
        }
    }
}
#endif