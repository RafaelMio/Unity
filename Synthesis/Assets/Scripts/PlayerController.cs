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
    private bool _canDash = true;
    private bool _isAttacking = false;
    private Animator _animator;
    private string _currentAnimation = "";

    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private Collider _currentWeaponCollider;
    [SerializeField] private float _speed = 12f;
    [SerializeField] private float _dashForce = 48F;
    [SerializeField] private float _dashTime = 0.25f;
    [SerializeField] private float _dashCooldown = 1f;

    private void Awake()
    {
        _defaultInputActions = new DefaultInputActions();
        _rigidBody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _animator.SetBool("AttackStart", _isAttacking);
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
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f && _animator.GetCurrentAnimatorStateInfo(0).IsName("SwordAttack1"))
        {
            _animator.SetBool("SwordAttack1", false);
            _animator.SetBool("AttackStart", false);
            DisableWeaponCollider();
        }
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (_isDahsing || !_canDash) return;
        StartCoroutine(Dash());
    }

    private void OnBasicAttack(InputAction.CallbackContext context)
    {
        Debug.Log("TAPER");
        EnableWeaponCollider();
        _isAttacking = true;
        _animator.SetBool("AttackStart", _isAttacking);
        _animator.SetBool("SwordAttack1", true);
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
            _animator.SetFloat("RunningSpeed", 0);
            _rigidBody.linearVelocity = Vector3.zero;
            return;
        }
        _animator.SetFloat("RunningSpeed", 1);
        _rigidBody.linearVelocity = _speed * (transform.forward * _input.magnitude);
    }

    private void EnableWeaponCollider()
    {
        _currentWeaponCollider.enabled = true;
    }

    private void DisableWeaponCollider()
    {
        _currentWeaponCollider.enabled = false;
    }

    private IEnumerator Dash()
    {
        float startTime = Time.time;
        _isDahsing = true;
        _canDash = false;
        _rigidBody.linearVelocity = _dashForce * this.transform.forward;
        yield return new WaitForSeconds(_dashTime);
        _isDahsing = false;
        yield return new WaitForSeconds(_dashCooldown);
        _canDash = true;
    }
}

public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}
