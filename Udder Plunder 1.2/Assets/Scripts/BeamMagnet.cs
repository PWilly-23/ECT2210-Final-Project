using UnityEngine;

/// <summary>
/// This script applies a magnetic pull to any cow (tagged "Cow") that enters its trigger.
/// The pull force drives the cow toward the beam’s center, and the effect is reduced if the UFO moves too fast.
/// </summary>
public class BeamMagnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    [Tooltip("The force pulling cows toward the beam’s center.")]
    public float pullForce = 17f;
    [Tooltip("If the UFO's speed exceeds this threshold, the pull force is reduced, making it easier for cows to be dropped.")]
    public float dropSpeedThreshold = 16f;
    [Tooltip("The radius within which the pull force is applied (for custom logic, if needed).")]
    public float pullRadius = 3f;

    [HideInInspector]
    public float ufoSpeed = 0f;  // Set externally by the UFOController during Update.

    // Optionally, you can designate a specific pull point.
    // If left null, the beam's own transform.position is used.
    public Transform pullPoint;

    private void Start()
    {
        if (pullPoint == null)
        {
            pullPoint = transform;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Only affect objects tagged as "Cow".
        if (!other.CompareTag("Cow"))
            return;

        Rigidbody cowRb = other.attachedRigidbody;
        if (cowRb == null)
            return;

        // Calculate direction from the cow toward the pull point.
        Vector3 direction = (pullPoint.position - other.transform.position).normalized;

        // Adjust the effective pull force based on the UFO's speed.
        // When UFO speed is low, full pullForce is applied.
        // As ufoSpeed exceeds dropSpeedThreshold, the pull weakens.
        float effectivePullForce = pullForce;
        if (ufoSpeed > dropSpeedThreshold)
        {
            // Here we linearly reduce the force. When ufoSpeed is twice dropSpeedThreshold the force would be zero.
            effectivePullForce = Mathf.Lerp(pullForce, 0, (ufoSpeed - dropSpeedThreshold) / dropSpeedThreshold);
        }

        // Apply the force as an acceleration.
        cowRb.AddForce(direction * effectivePullForce, ForceMode.Acceleration);
    }
}
