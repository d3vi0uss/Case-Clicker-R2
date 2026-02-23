using System.Collections;
using UnityEngine;

/// <summary>
/// Boiler maintenance loop. If ignored too long, player penalties and AI pressure increase.
/// Attach this to an interactable boiler console object.
/// </summary>
public class BoilerSystem : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private HotelAIManager aiManager;
    [SerializeField] private MobilePlayerController playerController;

    [Header("Maintenance")]
    [SerializeField] private float repairWindowSeconds = 120f;
    [SerializeField] private float repairDuration = 3f;
    [SerializeField] private float speedPenaltyMultiplier = 0.8f;

    [Header("Optional Hooks")]
    [SerializeField] private UnityEngine.Events.UnityEvent onRepairStarted;
    [SerializeField] private UnityEngine.Events.UnityEvent onRepairFinished;
    [SerializeField] private UnityEngine.Events.UnityEvent onNeglectState;

    private float timer;
    private bool isBroken;
    private bool isRepairing;

    private void Start()
    {
        timer = repairWindowSeconds;
    }

    private void Update()
    {
        if (isRepairing)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isBroken = true;
            timer = 0f;
            playerController?.SetMovementSpeedMultiplier(speedPenaltyMultiplier);
            aiManager?.AddBoilerNeglectTime(Time.deltaTime);
            onNeglectState?.Invoke();
        }
    }

    public void Interact(GameObject interactor)
    {
        if (!isBroken || isRepairing)
        {
            return;
        }

        StartCoroutine(RepairRoutine());
    }

    private IEnumerator RepairRoutine()
    {
        isRepairing = true;
        onRepairStarted?.Invoke();
        yield return new WaitForSeconds(repairDuration);

        isBroken = false;
        timer = repairWindowSeconds;
        playerController?.SetMovementSpeedMultiplier(1f);
        isRepairing = false;
        onRepairFinished?.Invoke();
    }
}
