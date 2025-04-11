using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Maximum rotation speed in degrees per second.")]
    public float rotationSpeed = 30f;
    [Tooltip("Time in seconds to reach full rotation speed and decelerate to zero.")]
    public float rotationSmoothTime = 0.2f;

    private UFOControls controls;
    private float rotateInput = 0f;

    // These fields store the current angular speed and are used for smoothing.
    private float currentAngularSpeed = 0f;
    private float angularSpeedVelocity = 0f;

    private void Awake()
    {
        controls = new UFOControls();

        // Subscribe to the Rotate action.
        controls.Player.Rotate.performed += ctx => rotateInput = ctx.ReadValue<float>();
        controls.Player.Rotate.canceled += ctx => rotateInput = 0f;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // Calculate the target angular speed based on input.
        float targetAngularSpeed = rotateInput * rotationSpeed;
        // Smoothly transition the current angular speed toward the target angular speed.
        currentAngularSpeed = Mathf.SmoothDamp(currentAngularSpeed, targetAngularSpeed, ref angularSpeedVelocity, rotationSmoothTime);

        // Apply rotation around the Y-axis.
        float angleDelta = currentAngularSpeed * Time.deltaTime;
        transform.Rotate(0f, angleDelta, 0f);
    }
}
