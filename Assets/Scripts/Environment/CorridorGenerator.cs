using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a corridor path from pre-made low-poly segment prefabs.
/// Uses a small object pool and limited segment count for mobile performance.
/// </summary>
public class CorridorGenerator : MonoBehaviour
{
    [SerializeField] private List<GameObject> corridorPrefabs = new List<GameObject>();
    [SerializeField] private int segmentCount = 12;
    [SerializeField] private float segmentLength = 8f;
    [SerializeField] private Transform corridorRoot;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();
    private readonly List<GameObject> activeSegments = new List<GameObject>();

    private void Start()
    {
        if (corridorRoot == null)
        {
            corridorRoot = transform;
        }

        PreWarmPool(segmentCount);
        Generate();
    }

    public void Generate()
    {
        ClearActive();

        Vector3 currentPos = corridorRoot.position;
        Quaternion currentRot = corridorRoot.rotation;

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segment = GetSegment();
            segment.transform.SetPositionAndRotation(currentPos, currentRot);
            segment.SetActive(true);
            activeSegments.Add(segment);

            currentPos += currentRot * Vector3.forward * segmentLength;

            // Occasional left/right turns for variation.
            if (Random.value < 0.2f)
            {
                currentRot *= Quaternion.Euler(0f, Random.value > 0.5f ? 90f : -90f, 0f);
            }
        }
    }

    private void PreWarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(PickPrefab(), corridorRoot);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    private GameObject GetSegment()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }

        GameObject go = Instantiate(PickPrefab(), corridorRoot);
        go.SetActive(false);
        return go;
    }

    private void ReturnSegment(GameObject segment)
    {
        segment.SetActive(false);
        pool.Enqueue(segment);
    }

    private void ClearActive()
    {
        for (int i = 0; i < activeSegments.Count; i++)
        {
            ReturnSegment(activeSegments[i]);
        }
        activeSegments.Clear();
    }

    private GameObject PickPrefab()
    {
        if (corridorPrefabs.Count == 0)
        {
            Debug.LogError("CorridorGenerator has no segment prefabs assigned.");
            return new GameObject("FallbackCorridorSegment");
        }

        int idx = Random.Range(0, corridorPrefabs.Count);
        return corridorPrefabs[idx];
    }
}
