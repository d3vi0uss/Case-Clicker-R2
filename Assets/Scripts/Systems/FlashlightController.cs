using System.Collections;
using UnityEngine;

/// <summary>
/// Flashlight battery and toggle logic with low-cost flicker behavior at low battery.
/// Uses no real-time shadows for mobile performance.
/// </summary>
public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light flashlight;
    [SerializeField] private TouchInputManager inputManager;

    [Header("Battery")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainPerSecond = 2.5f;
    [SerializeField] private float lowBatteryThreshold = 25f;

    [Header("Low Battery Flicker")]
    [SerializeField] private float flickerIntervalMin = 0.08f;
    [SerializeField] private float flickerIntervalMax = 0.22f;
    [SerializeField] private float flickerIntensityMin = 0.35f;

    private float defaultIntensity;
    private float battery;
    private bool isOn = true;
    private Coroutine flickerRoutine;

    public float BatteryNormalized => maxBattery <= 0f ? 0f : battery / maxBattery;
    public bool IsOn => isOn;

    private void Awake()
    {
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<Light>();
        }

        if (inputManager == null)
        {
            inputManager = FindObjectOfType<TouchInputManager>();
        }

        if (flashlight != null)
        {
            flashlight.shadows = LightShadows.None;
            defaultIntensity = flashlight.intensity;
        }

        battery = maxBattery;
    }

    private void Update()
    {
        if (inputManager != null && inputManager.FlashlightTogglePressed)
        {
            Toggle();
        }

        if (!isOn || flashlight == null)
        {
            return;
        }

        battery = Mathf.Max(0f, battery - drainPerSecond * Time.deltaTime);

        if (battery <= 0f)
        {
            SetState(false);
            return;
        }

        bool lowBattery = battery <= lowBatteryThreshold;

        if (lowBattery && flickerRoutine == null)
        {
            flickerRoutine = StartCoroutine(LowBatteryFlicker());
        }
        else if (!lowBattery && flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
            flashlight.intensity = defaultIntensity;
        }
    }

    public void Toggle()
    {
        if (battery <= 0f) return;
        SetState(!isOn);
    }

    public void SetState(bool enabledState)
    {
        isOn = enabledState;
        if (flashlight != null)
        {
            flashlight.enabled = enabledState;
            if (enabledState)
            {
                flashlight.intensity = defaultIntensity;
            }
        }

        if (!enabledState && flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }
    }

    private IEnumerator LowBatteryFlicker()
    {
        while (true)
        {
            float t = Mathf.InverseLerp(lowBatteryThreshold, 0f, battery);
            float minIntensity = Mathf.Lerp(flickerIntensityMin, 0.1f, t);
            flashlight.intensity = Random.Range(minIntensity, defaultIntensity);
            yield return new WaitForSeconds(Random.Range(flickerIntervalMin, flickerIntervalMax));
        }
    }
}
