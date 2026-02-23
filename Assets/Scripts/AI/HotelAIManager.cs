using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central event router for hotel horror events.
/// Tracks world stress metrics and triggers modular events selected from registered behaviors.
/// </summary>
public class HotelAIManager : MonoBehaviour
{
    [Serializable]
    public class AIBehavior
    {
        public string id;
        [Range(0f, 1f)] public float baseWeight = 1f;
        public UnityEngine.Events.UnityEvent onTriggered;
    }

    [Header("Tracked State")]
    [SerializeField] private float darknessTime;
    [SerializeField] private float idleTime;
    [SerializeField] private float boilerNeglectTime;

    [Header("Events")]
    [SerializeField] private List<AIBehavior> behaviors = new List<AIBehavior>();
    [SerializeField] private float decisionInterval = 10f;
    [SerializeField] private float minimumStressToTrigger = 0.2f;

    public float DarknessTime => darknessTime;
    public float IdleTime => idleTime;
    public float BoilerNeglectTime => boilerNeglectTime;

    public event Action<string> OnBehaviorTriggered;

    private void Start()
    {
        StartCoroutine(DecisionLoop());
    }

    public void AddDarknessTime(float delta) => darknessTime += Mathf.Max(0f, delta);
    public void AddIdleTime(float delta) => idleTime += Mathf.Max(0f, delta);
    public void AddBoilerNeglectTime(float delta) => boilerNeglectTime += Mathf.Max(0f, delta);

    public void RegisterBehavior(AIBehavior behavior)
    {
        if (behavior != null && !behaviors.Contains(behavior))
        {
            behaviors.Add(behavior);
        }
    }

    private IEnumerator DecisionLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(decisionInterval);
        while (true)
        {
            yield return wait;
            TryTriggerBehavior();
        }
    }

    private void TryTriggerBehavior()
    {
        if (behaviors.Count == 0)
        {
            return;
        }

        float stress = CalculateStress();
        if (stress < minimumStressToTrigger)
        {
            return;
        }

        // Weighted random selection to make the system easy to extend with new events.
        float total = 0f;
        for (int i = 0; i < behaviors.Count; i++)
        {
            total += Mathf.Max(0.01f, behaviors[i].baseWeight * stress);
        }

        float pick = UnityEngine.Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < behaviors.Count; i++)
        {
            AIBehavior b = behaviors[i];
            cumulative += Mathf.Max(0.01f, b.baseWeight * stress);
            if (pick <= cumulative)
            {
                b.onTriggered?.Invoke();
                OnBehaviorTriggered?.Invoke(b.id);
                break;
            }
        }
    }

    private float CalculateStress()
    {
        // Normalize tracked values into a 0-1 stress score.
        float darknessStress = Mathf.Clamp01(darknessTime / 60f);
        float idleStress = Mathf.Clamp01(idleTime / 45f);
        float boilerStress = Mathf.Clamp01(boilerNeglectTime / 90f);

        return Mathf.Clamp01((darknessStress * 0.4f) + (idleStress * 0.25f) + (boilerStress * 0.35f));
    }
}
