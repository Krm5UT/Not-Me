using UnityEngine;

/// <summary>
/// Attach to your VR Camera Rig root, OVRCameraRig, or XR Origin.
/// Automatically tags each hand/rig collider so the NPC can detect them.
///
/// Tags required in Project Settings → Tags:
///   "Player"  — camera rig / body
///   "VRHand"  — OVR hands or XR hand/controller objects
/// </summary>
public class VRRigTagger : MonoBehaviour
{
    [Header("Tag Names (must exist in Project Settings)")]
    [SerializeField] private string rigTag  = "Player";
    [SerializeField] private string handTag = "VRHand";

    [Header("Hand Transforms (assign in Inspector or leave blank for auto-find)")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    void Awake()
    {
        // Tag the rig root (camera / body collider)
        gameObject.tag = rigTag;

        // Auto-find OVR hands if not assigned
        if (leftHand  == null) leftHand  = FindHandByName("LeftHandAnchor",  "LeftHand",  "Left Controller");
        if (rightHand == null) rightHand = FindHandByName("RightHandAnchor", "RightHand", "Right Controller");

        TagHand(leftHand);
        TagHand(rightHand);
    }

    private void TagHand(Transform hand)
    {
        if (hand == null) return;
        hand.gameObject.tag = handTag;

        // Ensure the hand has a trigger collider for NPC detection
        var col = hand.GetComponent<Collider>();
        if (col == null)
        {
            var sphere = hand.gameObject.AddComponent<SphereCollider>();
            sphere.radius    = 0.06f;  // ~6 cm — tweak to match your hand mesh
            sphere.isTrigger = true;
            Debug.Log($"[VRRigTagger] Added SphereCollider trigger to {hand.name}");
        }
        else
        {
            col.isTrigger = true;
        }
    }

    private Transform FindHandByName(params string[] names)
    {
        foreach (var n in names)
        {
            var found = transform.Find(n);
            if (found != null) return found;

            // Deep search
            var deep = FindInChildren(transform, n);
            if (deep != null) return deep;
        }
        return null;
    }

    private Transform FindInChildren(Transform parent, string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(targetName)) return child;
            var result = FindInChildren(child, targetName);
            if (result != null) return result;
        }
        return null;
    }
}
