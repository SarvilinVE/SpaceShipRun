using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

partial class CameraRenderer
{
    private ScriptableRenderContext _contex;
    private Camera _camera;
    private CullingResults _cullingResults;
    Lighting lighting = new Lighting();

    private readonly CommandBuffer _commandBuffer;

    private static readonly List<ShaderTagId> drawingShaderTagIds = new List<ShaderTagId>
    {
        new ShaderTagId("SRPDefaultUnlit"),
        new ShaderTagId("UniversalForward"),
    };

    public CameraRenderer()
    {
        _commandBuffer = new CommandBuffer();
    }
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _camera = camera;
        _contex = context;
        _commandBuffer.name = _camera.name;

        DrawUI();

        if (!Cull(out var parameters))
        {
            return;
        }

        Settings(parameters);
        lighting.Setup(context, _cullingResults);
        DrawVisible();
        DrawUnsupportedShader();
        DrawGizmos();
        
        Submit();
    }

    private DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTags, SortingCriteria sortingCriteria, 
        out SortingSettings sortingSettings)
    {
        sortingSettings = new SortingSettings(_camera)
        {
            criteria = sortingCriteria
        };

        var drawingSettings = new DrawingSettings(shaderTags[0], sortingSettings);

        for(var i = 1; i < shaderTags.Count; i++)
        {
            drawingSettings.SetShaderPassName(i, shaderTags[i]);
        }

        return drawingSettings;
    }

    private bool Cull(out ScriptableCullingParameters parameters)
    {
        return _camera.TryGetCullingParameters(out parameters);
    }

    private void Submit()
    {
        _commandBuffer.EndSample(_camera.name);
        ExecuteCommandBuffer();
        _contex.Submit();
    }

    private void Settings(ScriptableCullingParameters parameters)
    {
        _cullingResults = _contex.Cull(ref parameters);
        _contex.SetupCameraProperties(_camera);
        _commandBuffer.ClearRenderTarget(true, true, Color.clear);
        _commandBuffer.BeginSample(_camera.name);
        ExecuteCommandBuffer();
    }

    private void ExecuteCommandBuffer()
    {
        _contex.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }

    private void DrawVisible()
    {
        var drawingSettings = CreateDrawingSettings(drawingShaderTagIds, SortingCriteria.CommonOpaque, out var sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);

        _contex.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);

        _contex.DrawSkybox(_camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;

        _contex.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }
}

partial class CameraRenderer
{
    partial void DrawGizmos()
    {
        if (!Handles.ShouldRenderGizmos())
        {
            return;
        }
        _contex.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
        _contex.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
    }

    partial void DrawUI()
    {
        //_contex.DrawUIOverlay(_camera);
        if (_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }

    partial void DrawUnsupportedShader();
    

#if UNITY_EDITOR
    private static readonly ShaderTagId[] _legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    private static Material _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
    partial void DrawGizmos();
    partial void DrawUI();

    partial void DrawUnsupportedShader()
    {
        var drawingSettings = new DrawingSettings(_legacyShaderTagIds[0], new SortingSettings(_camera))
        {
            overrideMaterial = _errorMaterial,
        };

        for(var i = 1; i<_legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
        }

        var filteringSettings = FilteringSettings.defaultValue;

        _contex.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }

#endif
}
