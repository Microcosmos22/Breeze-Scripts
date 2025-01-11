using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CloudCamera : MonoBehaviour
{
    public Material cloudMaterial; // Assign your cloud material in the Inspector

    void Start()
    {
        // Ensure the main camera has depth texture enabled for depth information
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (cloudMaterial != null)
        {
            // Render the clouds to the destination using the assigned cloud material
            Graphics.Blit(source, destination, cloudMaterial);
        }
        else
        {
            // Fallback if cloudMaterial is not assigned
            Graphics.Blit(source, destination);
        }
    }
}
