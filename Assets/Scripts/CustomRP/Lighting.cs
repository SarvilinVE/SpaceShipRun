using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class Lighting
{
	private CullingResults _cullingResults;
	private const string bufferName = "Lighting";

	private static int
		dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
		dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");

	private CommandBuffer buffer = new CommandBuffer
	{
		name = bufferName
	};

	public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
	{
		_cullingResults = cullingResults;
		buffer.BeginSample(bufferName);
        SetupDirectionalLight();
        //SetupLights();
        buffer.EndSample(bufferName);
		context.ExecuteCommandBuffer(buffer);
		buffer.Clear();
	}

	private void SetupLights()
	{
		NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
	}
	public void SetupDirectionalLight()
	{
		Light light = RenderSettings.sun;
		buffer.SetGlobalVector(dirLightColorId, light.color.linear * light.intensity);
		buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
	}
}