using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    const string bufferName = "Lighting";
    
    // static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColor");
    // static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
    
    const int maxDirLightCount = 4;
    
    static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
    
    static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];
    
    CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };

    CullingResults cullingResults;
    
    public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        // SetupDirectionalLight();
        SetupLights();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (var i = 0; i < visibleLights.Length; ++i)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= maxDirLightCount)
                {
                    break;
                }
            }
        }
        
        buffer.SetGlobalInt(dirLightCountId, dirLightCount);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }

    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        // Light light = RenderSettings.sun;
        // buffer.SetGlobalColor(dirLightColorId, light.color.linear * light.intensity);
        // buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
        dirLightColors[index] = visibleLight.finalColor;
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }
}
