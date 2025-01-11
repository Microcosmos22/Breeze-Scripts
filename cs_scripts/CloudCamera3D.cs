﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TrueClouds
{
    class CloudCamera3D : CloudCamera
    {
        private void Start()
        {
            Camera cloudCamera = GetComponent<Camera>();

            if (cloudCamera != null)
            {
                // Set the depth to render after other cameras (e.g., terrain camera)
                cloudCamera.depth = 1;

                // Set the clear flags to Depth Only to preserve depth buffer from other cameras
                cloudCamera.clearFlags = CameraClearFlags.Depth;

                // Ensure the culling mask includes only the Clouds layer
                cloudCamera.cullingMask = LayerMask.GetMask("Clouds");
            }
            else
            {
                Debug.LogError("No Camera component found on this GameObject!");
            }
        }

        [ImageEffectOpaque]
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderClouds(source, destination);
        }
    }
}