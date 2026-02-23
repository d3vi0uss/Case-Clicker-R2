using System.Collections;
using UnityEngine;

/// <summary>
/// Hidden sanity value manager.
/// Decreases sanity in darkness or prolonged idle states and applies lightweight sensory effects.
/// </summary>
public class SanitySystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioSource breathingSource;
    [SerializeField] private FlashlightController flashlightController;
    [SerializeField] private MobilePlayerController playerController;

    [Header("Sanity")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float darknessDrainPerSecond = 2f;
    [SerializeField] private float idleDrainPerSecond = 1.5f;
    [SerializeField] private float idleThresholdSeconds = 8f;

    [Header("Visual Effect")]
    [SerializeField] private float baseFov = 70f;
    [SerializeField] private float maxFovOffset = 4f;
    [SerializeField] private float fovWaveSpeed = 1.2f;

    [Header("Occasional Light Disturbance")]
    [SerializeField] private float disturbanceInterval = 7f;
    [SerializeField] private float disturbanceChance = 0.25f;

    private float sanity;
    private float idleTimer;

    public float Sanity => sanity;
    public float SanityNormalized => maxSanity <= 0f ? 0f : sanity / maxSanity;

    private void Start()
    {
        sanity = maxSanity;
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = baseFov;
        }

        StartCoroutine(DisturbanceLoop());
    }

    private void Update()
    {
        float delta = 0f;

        bool isDark = flashlightController != null && !flashlightController.IsOn;
        if (isDark)
        {
            delta -= darknessDrainPerSecond * Time.deltaTime;
        }

        bool isIdle = playerController != null && playerController.CurrentSpeedNormalized < 0.05f;
        if (isIdle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > idleThresholdSeconds)
            {
                delta -= idleDrainPerSecond * Time.deltaTime;
            }
        }
        else
        {
            idleTimer = 0f;
        }

        sanity = Mathf.Clamp(sanity + delta, 0f, maxSanity);

        ApplyLightweightEffects();
    }

    private void ApplyLightweightEffects()
    {
        float stress = 1f - SanityNormalized;

        if (playerCamera != null)
        {
            float wave = Mathf.Sin(Time.time * fovWaveSpeed) * maxFovOffset * stress;
            float targetFov = baseFov + wave;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, Time.deltaTime * 3f);
        }

        if (breathingSource != null)
        {
            breathingSource.volume = Mathf.Lerp(0f, 0.45f, stress);
            if (!breathingSource.isPlaying && breathingSource.volume > 0.01f)
            {
                breathingSource.Play();
            }
            else if (breathingSource.isPlaying && breathingSource.volume < 0.01f)
            {
                breathingSource.Stop();
            }
        }
    }

    private IEnumerator DisturbanceLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(disturbanceInterval);
        while (true)
        {
            yield return wait;

            if (flashlightController == null || !flashlightController.IsOn)
            {
                continue;
            }

            float stress = 1f - SanityNormalized;
            if (Random.value < disturbanceChance * stress)
            {
                flashlightController.SetState(false);
                yield return new WaitForSeconds(0.12f);
                flashlightController.SetState(true);
            }
        }
    }
}
