using System.Collections;
using UnityEngine;

/// <summary>
/// Lightweight mirror rendering for mobile with low-res RenderTexture and optional glitch/delay effects.
/// </summary>
public class MirrorController : MonoBehaviour
{
    [SerializeField] private Camera mirrorCamera;
    [SerializeField] private Renderer mirrorRenderer;
    [SerializeField] private int textureSize = 256;
    [SerializeField] private float refreshRate = 15f;

    [Header("Events")]
    [SerializeField] private bool delayedReflectionEnabled = true;
    [SerializeField] private float reflectionDelay = 0.15f;
    [SerializeField] private float glitchChance = 0.08f;

    private RenderTexture mirrorTexture;

    private void Awake()
    {
        SetupTexture();
        StartCoroutine(RefreshLoop());
    }

    private void SetupTexture()
    {
        mirrorTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32)
        {
            useMipMap = false,
            autoGenerateMips = false
        };

        if (mirrorCamera != null)
        {
            mirrorCamera.targetTexture = mirrorTexture;
            mirrorCamera.enabled = false; // Manual render only.
        }

        if (mirrorRenderer != null)
        {
            mirrorRenderer.material.mainTexture = mirrorTexture;
        }
    }

    private IEnumerator RefreshLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(1f / Mathf.Max(1f, refreshRate));

        while (true)
        {
            if (mirrorCamera != null)
            {
                if (delayedReflectionEnabled)
                {
                    yield return new WaitForSeconds(reflectionDelay);
                }

                if (Random.value < glitchChance)
                {
                    // Skip one frame to create a subtle glitch effect.
                    yield return wait;
                    continue;
                }

                mirrorCamera.Render();
            }

            yield return wait;
        }
    }

    private void OnDestroy()
    {
        if (mirrorTexture != null)
        {
            mirrorTexture.Release();
            Destroy(mirrorTexture);
        }
    }
}
