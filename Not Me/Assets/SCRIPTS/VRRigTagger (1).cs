using UnityEngine;

/// <summary>
/// Attach to your [BuildingBlock] Camera Rig or OVRCameraRig root.
///
/// Assign in Inspector:
///   Left Hand  → LeftHandAnchor  (under TrackingSpace)
///   Right Hand → RightHandAnchor (under TrackingSpace)
///
/// Optionally assign OVR Hand Data Sources if using hand tracking:
///   Left Hand Data  → OVRLeftHandDataSource  (under OVRHands)
///   Right Hand Data → OVRRightHandDataSource (under OVRHands)
///
/// Tags required in Project Settings → Tags:
///   "Player"  — camera rig / body
///   "VRHand"  — hand anchors and OVR hand objects
/// </summary>
public class VRRigTagger : MonoBehaviour
{
    [Header("Tag Names (must exist in Project Settings)")]
    [SerializeField] private string rigTag  = "Player";
    [SerializeField] private string handTag = "VRHand";

    [Header("Hand Anchors (TrackingSpace children)")]
    [Tooltip("Drag LeftHandAnchor from TrackingSpace here")]
    [SerializeField] private Transform leftHand;
    [Tooltip("Drag RightHandAnchor from TrackingSpace here")]
    [SerializeField] private Transform rightHand;

    [Header("OVR Hand Data Sources (optional - for hand tracking mode)")]
    [Tooltip("Drag OVRLeftHandDataSource from OVRHands here")]
    [SerializeField] private Transform leftHandData;
    [Tooltip("Drag OVRRightHandDataSource from OVRHands here")]
    [SerializeField] private Transform rightHandData;

    void Awake()
    {
        // Tag the rig root so the headset/body can also trigger NPCs
        gameObject.tag = rigTag;

        // Tag and add colliders to all hand sources
        SetupHand(leftHand,      sphereRadius: 0.06f);
        SetupHand(rightHand,     sphereRadius: 0.06f);
        SetupHand(leftHandData,  sphereRadius: 0.08f);
        SetupHand(rightHandData, sphereRadius: 0.08f);
    }

    private void SetupHand(Transform hand, float sphereRadius)
    {
        if (hand == null) return;

        // Apply tag
        hand.gameObject.tag = handTag;

        // Only add a collider if one doesn't already exist
        var col = hand.GetComponent<Collider>();
        if (col == null)
        {
            var sphere      = hand.gameObject.AddComponent<SphereCollider>();
            sphere.radius    = sphereRadius;
            sphere.isTrigger = true;
            Debug.Log($"[VRRigTagger] Added SphereCollider (r={sphereRadius}) trigger to {hand.name}");
        }
        else
        {
            // Make sure existing collider is a trigger
            col.isTrigger = true;
            Debug.Log($"[VRRigTagger] Set existing collider on {hand.name} to IsTrigger.");
        }
    }
}
