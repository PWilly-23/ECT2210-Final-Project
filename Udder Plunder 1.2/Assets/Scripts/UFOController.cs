using UnityEngine;
using UnityEngine.InputSystem;

public class UFOController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float gamepadMoveSpeed = 5f;   // Speed when using gamepad (with smooth acceleration)
    public float mouseMoveSpeed = 10f;    // Immediate speed when using the mouse
    public float fixedY = 2f;
    public float accelerationTime = 0.2f; // Time to reach full speed (for gamepad input)

    [Header("Beam Settings")]
    public int beamSegments = 20;
    public float beamLength = 5f;
    public float beamAngle = 45f;
    public Color defaultBeamColor = new Color(0f, 1f, 1f, 0.5f);

    private UFOControls controls;
    private Vector2 moveInput;

    // Fields for smoothing movement (when using gamepad)
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 velocitySmooth = Vector3.zero;

    // Flag to determine if the current input comes from mouse.
    private bool usingMouse;

    // Beam GameObject and components.
    private GameObject beamObject;
    private MeshFilter beamFilter;
    private MeshRenderer beamRenderer;

    private void Awake()
    {
        controls = new UFOControls();

        // Subscribe to the move input action.
        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
            // Check whether the input came from a Mouse.
            usingMouse = ctx.control.device is Mouse;
        };
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Subscribe to the beam input.
        controls.Player.Beam.performed += ctx => ActivateBeam();
        controls.Player.Beam.canceled += ctx => DeactivateBeam();
    }

    private void OnEnable() { controls.Enable(); }
    private void OnDisable() { controls.Disable(); }

    private void Start()
    {
        // Hide and lock the cursor.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Create the beam.
        CreateBeam();
        beamObject.SetActive(false);
    }

    void Update()
    {
        // Select the movement speed based on the input device.
        float moveSpeedToUse = usingMouse ? mouseMoveSpeed : gamepadMoveSpeed;

        // Calculate the target velocity.
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeedToUse;

        // Use immediate response for mouse input; apply smooth acceleration for gamepad.
        if (usingMouse)
        {
            currentVelocity = targetVelocity;
        }
        else
        {
            currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref velocitySmooth, accelerationTime);
        }

        Vector3 delta = currentVelocity * Time.deltaTime;
        Vector3 nextPos = transform.position + delta;

        // Confine the UFO to a 25x25 plane (assuming the plane is centered at the origin).
        float halfExtent = 12.5f;
        nextPos.x = Mathf.Clamp(nextPos.x, -halfExtent, halfExtent);
        nextPos.z = Mathf.Clamp(nextPos.z, -halfExtent, halfExtent);
        nextPos.y = fixedY;

        transform.position = nextPos;
    }

    void ActivateBeam()
    {
        if (beamObject != null)
        {
            beamObject.SetActive(true);
        }
    }

    void DeactivateBeam()
    {
        if (beamObject != null)
        {
            beamObject.SetActive(false);
        }
    }

    void CreateBeam()
    {
        beamObject = new GameObject("Beam");
        beamObject.transform.SetParent(transform);
        beamObject.transform.localPosition = Vector3.zero;
        beamObject.transform.localRotation = Quaternion.identity;

        beamFilter = beamObject.AddComponent<MeshFilter>();
        beamRenderer = beamObject.AddComponent<MeshRenderer>();

        beamFilter.mesh = GenerateBeamMesh();
        beamRenderer.material = CreateBeamMaterial();
    }

    Mesh GenerateBeamMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[beamSegments + 1];
        int[] triangles = new int[beamSegments * 3];

        // The apex of the inverted cone at the UFO's origin.
        vertices[0] = Vector3.zero;
        // Calculate the radius at the base from the beam angle.
        float baseRadius = beamLength * Mathf.Tan(beamAngle * Mathf.Deg2Rad);
        for (int i = 0; i < beamSegments; i++)
        {
            float angle = ((float)i / beamSegments) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * baseRadius;
            float z = Mathf.Sin(angle) * baseRadius;
            vertices[i + 1] = new Vector3(x, -beamLength, z);
        }
        for (int i = 0; i < beamSegments; i++)
        {
            int current = i + 1;
            int next = (i + 1) % beamSegments + 1;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = next;
            triangles[i * 3 + 2] = current;
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    Material CreateBeamMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        mat.color = defaultBeamColor;
        return mat;
    }
}
