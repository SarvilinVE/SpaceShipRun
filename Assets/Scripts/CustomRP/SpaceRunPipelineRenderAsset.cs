using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRanderPipline
{
    [CreateAssetMenu(menuName = "Rendering/SpaceRunPipelineRenderAsset")]

    public class SpaceRunPipelineRenderAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new SpaceRunPipelineRender();
        }
    }
}
