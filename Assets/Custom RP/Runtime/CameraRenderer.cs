using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;

    private const string bufferName = "Render Camera";

    private CommandBuffer buffer = new CommandBuffer() {name = bufferName};

    private CullingResults cullingResults;

    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");

    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing)
    {
        this.context = context;
        this.camera = camera;
        
        PrepareBuffer();
        PrepareForSceneWindow();

        // 视锥体剔除
        if (!Cull())
        {
            return;
        }
        
        Setup();
        // 绘制视锥体内可见的物体
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        // 绘制不支持的Shader
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }

    private void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        /*
         * 对于 Unity 2022，我对渲染流程做了一个修改：现在默认情况下，每一帧都会清除颜色缓冲区（color buffer），除非开发者明确指定不要清除。
            原因解释：
                避免 NaN 和 Infinity 值引起的混合问题： 渲染目标（render target，也就是颜色缓冲区）可能会包含一些特殊的值，比如“非数字”（Not-a-Number, NaN）和“无穷大”（Infinity）。 
                这些值在混合（blending）过程中会导致不正确的计算结果，从而产生视觉上的瑕疵（artifacts）。
                方便调试： 如果没有清除颜色缓冲区，帧调试器（frame debugger）可能会显示一些随机的、历史遗留的数据。 这会让人难以判断当前的渲染状态，给调试带来困难。 
                清除颜色缓冲区可以确保帧调试器显示的是当前帧的真实数据，更容易进行问题定位。
            总结：
                为了避免渲染错误和提高调试效率，Unity 2022 现在默认清除颜色缓冲区。 开发者可以通过特定设置来关闭这个默认行为，但建议保持启用，除非有明确的理由需要保留上一帧的颜色数据。
            更简洁的版本：
                Unity 2022 默认清除颜色缓冲区，避免 NaN/Infinity 值导致的混合问题，并确保帧调试器显示正确数据，方便调试。 除非明确指定，否则默认启用。
         */
        // buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, Color.clear);
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags <= CameraClearFlags.Color, 
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        // 1. 绘制不透明物体
        var sortingSettings = new SortingSettings(camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
        };
        drawingSettings.SetShaderPassName(1, litShaderTagId);
        
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        
        // 2. 绘制天空盒
        context.DrawSkybox(camera);

        // 3. 绘制透明物体
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    private void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    private bool Cull()
    {
        if (camera.TryGetCullingParameters(out var p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }

        return false;
    }
}
