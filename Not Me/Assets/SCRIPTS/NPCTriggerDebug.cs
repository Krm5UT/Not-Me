using UnityEngine;

/// <summary>
/// TEMPORARY DEBUG SCRIPT — attach this to your NPC alongside NPCThreatResponse.
/// It logs EVERYTHING that enters the trigger zone, no tag filtering.
/// Check the Console while running to see what Unity detects.
/// Remove this script once the issue is fixed.
/// </summary>
public class NPCTriggerDebug : MonoBehaviour
{
    void Start()
    {
        // Report all colliders on this NPC at startup
        var colliders = GetComponents<Collider>();
        if (colliders.Length == 0)
        {
            Debug.LogError($"[TriggerDebug] {name} has NO colliders at all! Add a Capsule Collider with IsTrigger = true.");
            return;
        }

        foreach (var col in colliders)
        {
            Debug.Log($"[TriggerDebug] {name} has collider: {col.GetType().Name} | IsTrigger={col.isTrigger} | Enabled={col.enabled}");
        }

        // Report Rigidbody status
        var rb = GetComponent<Rigidbody>();
        var rbParent = GetComponentInParent<Rigidbody>();
        if (rb == null && rbParent == null)
        {
            Debug.LogWarning($"[TriggerDebug] {name} has no Rigidbody on itself or parent. " +
                             "Unity requires at least one Rigidbody in a trigger pair. " +
                             "Add a Rigidbody to the NPC with IsKinematic = true.");
        }
        else
        {
            var foundRb = rb != null ? rb : rbParent;
            Debug.Log($"[TriggerDebug] Rigidbody found on: {foundRb.gameObject.name} | IsKinematic={foundRb.isKinematic}");
        }
    }

    // Catches ANYTHING entering — no tag check
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TriggerDebug] ENTERED by: '{other.name}' | tag='{other.tag}' | layer={LayerMask.LayerToName(other.gameObject.layer)}");
    }

    private void OnTriggerStay(Collider other)
    {
        // Uncomment this if OnTriggerEnter never fires but you want to check Stay events
        // Debug.Log($"[TriggerDebug] STAYING: '{other.name}' | tag='{other.tag}'");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[TriggerDebug] EXITED by: '{other.name}' | tag='{other.tag}'");
    }
}
