using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class ShipAgent : Agent
{
    public static ShipAgent Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float minX = -7f;
    [SerializeField] private float maxX = 7f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootCooldown = 0.2f;

    public int EpisodeCount { get; private set; } = 0;
    public float CurrentEpisodeReward { get; private set; } = 0f;

    private float shootTimer = 0f;
    private int decisionCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        shootTimer -= Time.deltaTime;
    }

    public override void OnEpisodeBegin()
    {
        EpisodeCount++;
        CurrentEpisodeReward = 0f;
        decisionCount = 0;

        Debug.Log($"[ShipAgent] Episode {EpisodeCount} started.");

        float centerX = (minX + maxX) * 0.5f;
        Vector3 pos = transform.position;
        pos.x = centerX;
        transform.position = pos;

        shootTimer = 0f;

        if (MatrixSpawner.Instance != null)
        {
            MatrixSpawner.Instance.ResetWave();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float normalizedX = Mathf.InverseLerp(minX, maxX, transform.position.x);
        normalizedX = normalizedX * 2f - 1f;
        sensor.AddObservation(normalizedX);

        Vector2 nearestChicken = Vector2.zero;
        float foundChicken = 0f;

        if (MatrixSpawner.Instance != null)
        {
            GameObject closest = MatrixSpawner.Instance.GetClosestChicken(transform.position);
            if (closest != null)
            {
                Vector3 diff = closest.transform.position - transform.position;
                nearestChicken = new Vector2(diff.x, diff.y);
                foundChicken = 1f;
            }
        }

        sensor.AddObservation(nearestChicken.x);
        sensor.AddObservation(nearestChicken.y);
        sensor.AddObservation(foundChicken);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        decisionCount++;

        int action = actions.DiscreteActions[0];

        if (action == 1) Move(-1f);
        else if (action == 2) Move(1f);
        else if (action == 3) Shoot();

        float stepPenalty = -0.0005f;
        AddReward(stepPenalty);
        CurrentEpisodeReward += stepPenalty;

        if (decisionCount % 100 == 0)
        {
            Debug.Log($"[ShipAgent] Step {decisionCount}, action={action}, episodeReward={CurrentEpisodeReward:F2}");
        }
    }

    private void Move(float direction)
    {
        Vector3 pos = transform.position;
        pos.x += direction * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;
        if (shootTimer > 0f) return;

        Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        shootTimer = shootCooldown;
    }

    public void AddCustomReward(float value)
    {
        AddReward(value);
        CurrentEpisodeReward += value;
    }

    // Keyboard control for testing
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discrete = actionsOut.DiscreteActions;
        discrete[0] = 0;

        float h = Input.GetAxisRaw("Horizontal");
        bool fire = Input.GetKey(KeyCode.Space);

        if (h < 0f) discrete[0] = 1;
        else if (h > 0f) discrete[0] = 2;

        if (fire) discrete[0] = 3;
    }
}