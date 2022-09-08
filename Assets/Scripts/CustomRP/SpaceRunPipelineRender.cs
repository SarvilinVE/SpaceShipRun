using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomRanderPipline
{
    public class SpaceRunPipelineRender : RenderPipeline
    {
        private CameraRenderer _cameraRender;
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            _cameraRender = new CameraRenderer();
            CamerasRender(context, cameras);
        }

        private void CamerasRender(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach(var camera in cameras)
            {
                _cameraRender.Render(context, camera);
            }
        }
    }
}
