using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    public Transform groundCheck;

    private DefaultInputActions _defaultInputActions;
    private InputAction _moveAction;
    private Vector3 _input;
    private bool _isDahsing = false;

    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _dashForce = 48F;
    [SerializeField] private float _dashTime = 0.25f;

    private void Awake()
    {
        _defaultInputActions = new DefaultInputActions();
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _moveAction = _defaultInputActions.Player.Move;
        _defaultInputActions.Player.Move.Enable();

        _defaultInputActions.Player.BasicAttack.performed += OnBasicAttack;
        _defaultInputActions.Player.BasicAttack.Enable();

        _defaultInputActions.Player.Dash.performed += OnDash;
        _defaultInputActions.Player.Dash.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _defaultInputActions.Player.BasicAttack.Disable();

        _defaultInputActions.Player.BasicAttack.performed -= OnBasicAttack;
        _defaultInputActions.Player.BasicAttack.Disable();

        _defaultInputActions.Player.Dash.performed -= OnDash;
        _defaultInputActions.Player.Dash.Disable();
    }

    private void FixedUpdate()
    {
        GatherInput();
        PlayerRotation();
        Move();
       
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        _isDahsing = true;
        StartCoroutine(Dash());
    }

    private void OnBasicAttack(InputAction.CallbackContext context)
    {

    }

    private void GatherInput()
    {
        Vector2 moveDirection = _moveAction.ReadValue<Vector2>();
        _input = new Vector3(moveDirection.x, 0, moveDirection.y);
    }

    private void PlayerRotation()
    {
        if (_input == Vector3.zero || _isDahsing) return;

        var rotation = Quaternion.LookRotation(_input.ToIso(), Vector3.up);
        transform.rotation = rotation;
    }

    private void Move()
    {
        if (_isDahsing) return;

        if (_input == Vector3.zero)
        {
            _rigidBody.linearVelocity = Vector3.zero;
            return;
        }
        _rigidBody.linearVelocity = _speed * (transform.forward * _input.magnitude);
    }

    private IEnumerator Dash()
    {
        float startTime = Time.time;
        Vector3 fixedDir = transform.forward;

        while (Time.time < startTime + _dashTime)
        {
            _rigidBody.linearVelocity = _dashForce * fixedDir;
            yield return null;
        }
        _isDahsing = false;
    }
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}
