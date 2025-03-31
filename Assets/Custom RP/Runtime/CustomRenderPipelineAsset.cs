using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    
    [SerializeField]
    bool useDynamicBatching = false, useGPUInstancing = false, useSRPBatcher = true;
    
    [SerializeField]
    ShadowSettings shadows = new ShadowSettings();
    
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(useDynamicBatching, useGPUInstancing, useSRPBatcher, shadows);
    }
}
