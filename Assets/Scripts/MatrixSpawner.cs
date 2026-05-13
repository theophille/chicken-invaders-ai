using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatrixSpawner : MonoBehaviour
{
    public static MatrixSpawner Instance { get; private set; }

    [Header("Grid")]
    [SerializeField] private GameObject chickenPrefab;
    [SerializeField] private int rows = 2;
    [SerializeField] private int columns = 2;
    [SerializeField] private float gap = 1.5f;
    [SerializeField] private float zDepth = 10f;

    [Header("Row Movement")]
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Egg Shooting")]
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private float minDelay = 2f;
    [SerializeField] private float maxDelay = 4f;
    [SerializeField] private float xRange = 2f;

    [Header("References")]
    [SerializeField] private Transform racket; // ship transform

    [Header("Attacker")]
    [SerializeField] private float attackerInterval = 10f;

    private List<List<GameObject>> _rows = new List<List<GameObject>>();
    private List<float> _rowOriginX = new List<float>();
    private List<GameObject> _chickens = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CenterOnCamera();
        SpawnMatrix();
        StartCoroutine(EggRoutine());
        // StartCoroutine(AttackerRoutine()); // keep disabled while training basic behavior
    }

    void CenterOnCamera()
    {
        Camera cam = Camera.main;
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, zDepth);
        transform.position = cam.ScreenToWorldPoint(screenCenter);
    }

    void SpawnMatrix()
    {
        if (chickenPrefab == null) return;

        float totalWidth = (columns - 1) * gap;
        float totalHeight = (rows - 1) * gap;
        float originX = transform.position.x - totalWidth / 2f;
        float originY = transform.position.y + totalHeight / 2f;

        for (int row = 0; row < rows; row++)
        {
            List<GameObject> rowList = new List<GameObject>();
            float direction = (row % 2 == 0) ? 1f : -1f;
            float rowBaseX = originX;
            float homeY = originY - row * gap;

            for (int col = 0; col < columns; col++)
            {
                Vector3 spawnPos = new Vector3(
                    originX + col * gap,
                    homeY,
                    transform.position.z
                );

                GameObject chickenGO = Instantiate(chickenPrefab, spawnPos, Quaternion.identity);
                chickenGO.transform.parent = transform;
                chickenGO.name = $"Chicken_{row}_{col}";

                Chicken chicken = chickenGO.GetComponent<Chicken>();
                if (chicken != null)
                {
                    chicken.Init(col, gap, rowBaseX, direction,
                        moveDistance, moveSpeed, homeY, racket);
                }

                rowList.Add(chickenGO);
                _chickens.Add(chickenGO);
            }

            _rows.Add(rowList);
            _rowOriginX.Add(transform.position.x);
        }
    }

    public void ResetWave()
    {
        foreach (var c in _chickens)
        {
            if (c != null) Destroy(c);
        }
        _chickens.Clear();
        _rows.Clear();
        _rowOriginX.Clear();

        StopAllCoroutines();

        SpawnMatrix();
        StartCoroutine(EggRoutine());
        // StartCoroutine(AttackerRoutine()); // re-enable later for kamikaze
    }

    public void NotifyChickenDead()
    {
        _chickens.RemoveAll(c => c == null);

        if (_chickens.Count == 0)
        {
            Debug.Log("[MatrixSpawner] All chickens dead — wave cleared!");

            if (ShipAgent.Instance != null)
            {
                ShipAgent.Instance.AddCustomReward(+5f); // bonus for clearing the wave
                Debug.Log($"[ShipAgent] Wave cleared! EpisodeReward={ShipAgent.Instance.CurrentEpisodeReward:F2}");
                ShipAgent.Instance.EndEpisode();
            }
        }
    }

    public GameObject GetClosestChicken(Vector3 position)
    {
        float bestDist = float.MaxValue;
        GameObject best = null;

        foreach (var c in _chickens)
        {
            if (c == null) continue;
            float d = Vector3.SqrMagnitude(c.transform.position - position);
            if (d < bestDist)
            {
                bestDist = d;
                best = c;
            }
        }

        return best;
    }

    IEnumerator EggRoutine()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            _chickens.RemoveAll(c => c == null);
            if (_chickens.Count == 0) yield break;
            if (racket == null) yield break;

            List<GameObject> candidates = _chickens.FindAll(c =>
                c != null &&
                c.transform.position.x > racket.position.x - xRange &&
                c.transform.position.x < racket.position.x + xRange
            );

            if (candidates.Count == 0) continue;

            GameObject picked = candidates[Random.Range(0, candidates.Count)];
            if (eggPrefab != null)
                Instantiate(eggPrefab, picked.transform.position, Quaternion.identity);
        }
    }

    IEnumerator AttackerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackerInterval);

            _chickens.RemoveAll(c => c == null);
            if (_chickens.Count == 0) yield break;

            GameObject chosen = _chickens[Random.Range(0, _chickens.Count)];
            Chicken chicken = chosen != null ? chosen.GetComponent<Chicken>() : null;
            if (chicken != null)
                chicken.BecomeAttacker();
        }
    }
}