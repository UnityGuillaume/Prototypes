using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SpecialRendering : MonoBehaviour
{
    protected Camera m_Camera;

    private void OnEnable()
    {
        m_Camera = GetComponent<Camera>();
        m_Camera.AddCommandBuffer(CameraEvent.BeforeLighting, DecalRoad.commandBuffer);
    }

    private void OnDisable()
    {
        m_Camera.RemoveCommandBuffer(CameraEvent.BeforeLighting, DecalRoad.commandBuffer);
    }
}
