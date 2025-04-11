using UnityEngine;
using UnityEngine.InputSystem;

public class UFOController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float gamepadMoveSpeed = 5f;    // Speed when using gamepad (with smooth acceleration)
    public float mouseMoveSpeed = 10f;     // Immediate speed when using the mouse
    public float fixedY = 2f;              // Fixed height of the UFO
    public float accelerationTime = 0.2f;  // Smoothing time for gamepad input

    [Header("Circular Island Settings")]
    [Tooltip("Radius of the circular island (in world units).")]
    public float islandRadius = 15f;
    [Tooltip("Center of the circular island in world space.")]
    public Vector3 islandCenter = Vector3.zero;

    [Header("Beam Settings")]
    public int beamSegments = 20;
    public float beamLength = 5f;
    public float beamAngle = 45f;
    public Color defaultBeamColor = new Color(0f, 1f, 1f, 0.5f);

    private UFOControls controls;
    private Vector2 moveInput;

    // For smoothing movement (when using gamepad)
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 velocitySmooth = Vector3.zero;

    // Flag to determine if input is coming from a Mouse.
    private bool usingMouse;

    // Beam GameObject and its components.
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
        // Determine the movement speed based on the input device.
        float moveSpeedToUse = usingMouse ? mouseMoveSpeed : gamepadMoveSpeed;
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeedToUse;

        // For mouse input, apply immediate movement; for gamepad, smooth acceleration/deceleration.
        if (usingMouse)
        {
            currentVelocity = targetVelocity;
        }
        else
        {
            currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref velocitySmooth, accelerationTime);
        }

        // Calculate the tentative new position.
        Vector3 delta = currentVelocity * Time.deltaTime;
        Vector3 nextPos = transform.position + delta;
        nextPos.y = fixedY;

        // Clamp the UFO's horizontal (X, Z) position to within a circle of islandRadius around islandCenter.
        Vector3 offset = nextPos - islandCenter;       // Offset from the island center.
        Vector2 offsetXZ = new Vector2(offset.x, offset.z); // Project to XZ plane.
        if (offsetXZ.magnitude > islandRadius)
        {
            offsetXZ = offsetXZ.normalized * islandRadius;
            nextPos.x = islandCenter.x + offsetXZ.x;
            nextPos.z = islandCenter.z + offsetXZ.y;
        }

        transform.position = nextPos;

        // Update the BeamMagnet's ufoSpeed if the beam is active.
        if (beamObject != null && beamObject.activeSelf)
        {
            BeamMagnet magnet = beamObject.GetComponent<BeamMagnet>();
            if (magnet != null)
            {
                magnet.ufoSpeed = currentVelocity.magnitude;
            }
        }
    }

    void ActivateBeam()
    {
        if (beamObject != null)
            beamObject.SetActive(true);
    }

    void DeactivateBeam()
    {
        if (beamObject != null)
            beamObject.SetActive(false);
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

        MeshCollider meshCollider = beamObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = beamFilter.mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = true;

        beamObject.AddComponent<BeamMagnet>();
    }

    Mesh GenerateBeamMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[beamSegments + 1];
        int[] triangles = new int[beamSegments * 3];

        vertices[0] = Vector3.zero;
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
