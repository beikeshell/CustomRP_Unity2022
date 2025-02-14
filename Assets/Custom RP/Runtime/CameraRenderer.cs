using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

public class CameraRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;
        
        Setup();
        DrawVisibleGeometry();
        Submit();
    }

    private void Setup()
    {
        context.SetupCameraProperties(camera);
    }

    private void DrawVisibleGeometry()
    {
        context.DrawSkybox(camera);
    }

    private void Submit()
    {
        context.Submit();
    }
}
