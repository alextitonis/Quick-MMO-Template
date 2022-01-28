using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager getInstance;
    void Awake() { getInstance = this; }

    [SerializeField] string VerticalAxis = "Vertical";
    [SerializeField] string HorizontalAxis = "Horizontal";
    [SerializeField] KeyCode JumpKey = KeyCode.B;
    [SerializeField] KeyCode RollKey = KeyCode.Space;

    [HideInInspector] public float Vertical { get; private set; }
    [HideInInspector] public float Horizontal { get; private set; }
    [HideInInspector] public float VerticalRaw { get; private set; }
    [HideInInspector] public float HorizontalRaw { get; private set; }
    [HideInInspector] public bool Jump { get; private set; }
    [HideInInspector] public bool Roll { get; private set; }

    void Update()
    {
        Vertical = Input.GetAxis(VerticalAxis);
        Horizontal = Input.GetAxis(HorizontalAxis);
        VerticalRaw = Input.GetAxis(VerticalAxis);
        HorizontalRaw = Input.GetAxis(HorizontalAxis);
        Jump = Input.GetKeyDown(JumpKey);
        Roll = Input.GetKeyDown(RollKey);
    }
}