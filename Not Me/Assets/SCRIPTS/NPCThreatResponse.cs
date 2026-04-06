using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the NPC root GameObject.
/// Requires:
///   - An Animator with states: Idle → DrawGun → PistolIdle
///   - A Collider on this GameObject with IsTrigger = true
///   - Your VR hands/rig objects tagged "Player" or "VRHand"
///
/// No Animation Events needed — reset is handled by a coroutine timer.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Animator))]
public class NPCThreatResponse : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────
    [Header("Animator Parameters")]
    [Tooltip("Trigger parameter name in the Animator Controller")]
    [SerializeField] private string drawGunTrigger = "DrawGun";

    [Header("Draw Gun Clip")]
    [Tooltip("How long the DrawGun animation clip is in seconds. " +
             "Check this in your Animation window — shown at the top right of the timeline.")]
    [SerializeField] private float drawGunClipLength = 1.5f;

    [Header("Detection")]
    [Tooltip("Tags that count as a VR contact source")]
    [SerializeField] private string[] threatTags = { "Player", "VRHand" };

    [Tooltip("Extra seconds to wait after the clip finishes before allowing re-trigger. " +
             "0 = reset exactly when the clip ends.")]
    [SerializeField] private float extraCooldown = 0.5f;

    [Header("Optional")]
    [Tooltip("Play an audio cue when the NPC reacts")]
    [SerializeField] private AudioClip reactionClip;

    // ── Private ──────────────────────────────────────────────────
    private Animator    _animator;
    private AudioSource _audio;
    private bool        _triggered;
    private int         _drawGunHash;

    // ─────────────────────────────────────────────────────────────
    void Awake()
    {
        _animator    = GetComponent<Animator>();
        _audio       = GetComponent<AudioSource>(); // optional
        _drawGunHash = Animator.StringToHash(drawGunTrigger);

        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[NPCThreatResponse] Collider on {name} is not a trigger. Setting it now.");
            col.isTrigger = true;
        }
    }

    // ── VR Contact Detection ──────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (!IsThreat(other)) return;
        React();
    }

    // ── Core Logic ────────────────────────────────────────────────

    private bool IsThreat(Collider other)
    {
        if (_triggered) return false;

        foreach (var tag in threatTags)
            if (other.CompareTag(tag)) return true;

        return false;
    }

    private void React()
    {
        _triggered = true;

        _animator.SetTrigger(_drawGunHash);

        if (_audio != null && reactionClip != null)
            _audio.PlayOneShot(reactionClip);

        Debug.Log($"[NPCThreatResponse] {name} reacting to VR contact.");

        // Start the timer — no Animation Event needed
        StartCoroutine(ResetAfterClip());
    }

    /// <summary>
    /// Waits for the DrawGun clip to finish (plus any extra cooldown)
    /// then allows the NPC to be triggered again.
    /// </summary>
    private IEnumerator ResetAfterClip()
    {
        float waitTime = drawGunClipLength + extraCooldown;
        yield return new WaitForSeconds(waitTime);

        _triggered = false;
        Debug.Log($"[NPCThreatResponse] {name} reset after {waitTime:F1}s — ready to react again.");
    }

    // ── Editor Helper ─────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        var col = GetComponent<Collider>();
        if      (col is SphereCollider  sc) Gizmos.DrawSphere(transform.position, sc.radius);
        else if (col is BoxCollider     bc) Gizmos.DrawCube(transform.position + bc.center, bc.size);
        else if (col is CapsuleCollider cc) Gizmos.DrawWireSphere(transform.position, cc.radius);
    }
#endif
}
