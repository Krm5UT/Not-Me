using UnityEngine;

/// <summary>
/// VRObjectStop — Spawn an object, move it, and bring it to a halt
/// the moment it enters a STOP trigger. No looping, no respawn.
///
/// SETUP (4 steps):
///  1. Create an empty GameObject — call it "VRObjectStop Manager".
///  2. Attach this script to it.
///  3. Assign your car/object prefab to [objectPrefab].
///  4. Place an empty GameObject in the scene as [spawnPoint].
///  5. Create a Box Collider somewhere, tick "Is Trigger", assign it to
///     [stopTrigger]. When the moving object enters this, it freezes.
///
/// Optional: set [faceDirection] so the object auto-rotates toward [moveDirection].
/// </summary>
public class VRObjectStop : MonoBehaviour
{
    [Header("Object")]
    [Tooltip("The car, crate, drone — whatever you want to move and stop.")]
    public GameObject objectPrefab;

    [Tooltip("Where the object spawns.")]
    public Transform spawnPoint;

    [Header("Movement")]
    [Tooltip("World-space direction the object travels. Normalized automatically.")]
    public Vector3 moveDirection = Vector3.forward;

    [Tooltip("Speed in units/second.")]
    public float moveSpeed = 8f;

    [Tooltip("Rotate the object to face its travel direction on spawn.")]
    public bool faceDirection = true;

    [Header("Stop Trigger")]
    [Tooltip("A collider with 'Is Trigger' checked. Object stops on entering this.")]
    public Collider stopTrigger;

    // ── Private state ──────────────────────────────────────────────────────────
    private GameObject _activeObject;
    private bool       _moving;

    // ──────────────────────────────────────────────────────────────────────────
    void Start()
    {
        ValidateSetup();
        SpawnObject();
    }

    // ──────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!_moving || _activeObject == null) return;

        _activeObject.transform.position +=
            moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Called by StopTriggerRelay when the moving object enters the trigger
    public void OnObjectEnteredStopTrigger()
    {
        if (!_moving) return;

        _moving = false;
        Debug.Log("[VRObjectStop] Object stopped at trigger.");

        // Optional: snap exactly to the trigger centre for a clean stop
        if (stopTrigger != null)
            _activeObject.transform.position = stopTrigger.bounds.center;
    }

    // ──────────────────────────────────────────────────────────────────────────
    void SpawnObject()
    {
        _activeObject = Instantiate(objectPrefab, spawnPoint.position, spawnPoint.rotation);

        if (faceDirection && moveDirection != Vector3.zero)
            _activeObject.transform.rotation =
                Quaternion.LookRotation(moveDirection.normalized);

        // Attach the relay to the moving object so it can report trigger contact
        StopTriggerRelay relay = _activeObject.GetComponent<StopTriggerRelay>();
        if (relay == null) relay = _activeObject.AddComponent<StopTriggerRelay>();
        relay.owner     = this;
        relay.triggerID = stopTrigger;

        _moving = true;
        if (faceDirection && moveDirection != Vector3.zero)
{
    Quaternion baseRotation = Quaternion.LookRotation(moveDirection.normalized);

    // Adjust this offset until the car faces correctly:
    // 90f  = nose was pointing right (X axis)
    // -90f = nose was pointing left (-X axis)
    // 180f = nose was pointing backward (-Z axis)
    Quaternion offset = Quaternion.Euler(90f, 0f, 0f);

    _activeObject.transform.rotation = baseRotation * offset;
}
    }

    // ──────────────────────────────────────────────────────────────────────────
    void ValidateSetup()
    {
        if (objectPrefab == null) Debug.LogError("[VRObjectStop] objectPrefab is not assigned!");
        if (spawnPoint   == null) Debug.LogError("[VRObjectStop] spawnPoint is not assigned!");
        if (stopTrigger  == null) Debug.LogError("[VRObjectStop] stopTrigger is not assigned!");
    }

    // ──────────────────────────────────────────────────────────────────────────
    /// <summary>Public helper — restart from scratch if you ever need it via code.</summary>
    public void Restart()
    {
        if (_activeObject != null) Destroy(_activeObject);
        SpawnObject();
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
/// <summary>
/// Rides on the moving object and watches for collision with the stop trigger.
/// Auto-attached at runtime — you don't touch this.
/// </summary>
public class StopTriggerRelay : MonoBehaviour
{
    [HideInInspector] public VRObjectStop owner;
    [HideInInspector] public Collider     triggerID; // which trigger counts

    void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("StopTrigger"))
        owner.OnObjectEnteredStopTrigger();
}
}