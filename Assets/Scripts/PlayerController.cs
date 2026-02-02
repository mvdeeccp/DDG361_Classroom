using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _stepHeight = 0.5f;
    [SerializeField] private float _stepDisableDuration = 0.5f;
    private float _stepDisableTimer;
    [SerializeField] private float _heightError;

    [Header("Jump")]
    [SerializeField] private float _jumpForce = 15f;
    [SerializeField] private float _jumpBufferTime = 0.15f;
    [SerializeField] private float _coyoteTime = 0.15f;
    private bool _jumpQueued;
    private float _jumpBufferCounter;
    private float _coyoteCounter;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D _rb;
    private RaycastHit2D hit;

    private Vector2 _correctionVelocityLastFrame;
    private Vector2 _velocity;
    private float _rayLength;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = _jumpBufferTime;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        RemoveCorrectionVelocity();

        ToggleGrounded();

        Movement();

        StepUp();

        UpdateCoyoteTime();

        Jump();

        if (_stepDisableTimer > 0)
        {
            _stepDisableTimer -= Time.fixedDeltaTime;
        }
    }

    private void SetVelocity(Vector2 v)
    {
        _velocity = v;
        _rb.velocity = v;
    }

    private void Movement()
    {
        float moveX = Input.GetAxis("Horizontal");

        if (IsGrounded())
        {
            SetVelocity(new Vector2(moveX * _speed, 0f));
        }
        else
        {
            SetVelocity(new Vector2(moveX * _speed, _rb.velocity.y));
        }
    }

    private void UpdateCoyoteTime()
    {
        if (IsGrounded())
            _coyoteCounter = _coyoteTime;
        else
            _coyoteCounter -= Time.fixedDeltaTime;
    }

    private void Jump()
    {
        if (_jumpBufferCounter > 0f && _coyoteCounter > 0f)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, 0f);
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

            _jumpBufferCounter = 0f;
            _coyoteCounter = 0f;

            _stepDisableTimer = _stepDisableDuration;
            _rayLength = _stepHeight;
        }

        _jumpQueued = false;
    }

    private void RemoveCorrectionVelocity()
    {
        _velocity = _rb.velocity;
        _velocity -= _correctionVelocityLastFrame;

        SetVelocity(_velocity);
        _correctionVelocityLastFrame = Vector2.zero;
    }

    private void StepUp()
    {
        if (!IsGrounded() || _stepDisableTimer > 0f)
        {
            _rb.gravityScale = 3f;
            return;
        }

        _rb.gravityScale = 0f;

        float heightError = _stepHeight - hit.distance;
        float targetVelocity = heightError / Time.fixedDeltaTime;

        _heightError = heightError;

        _correctionVelocityLastFrame = new Vector2(0, targetVelocity);
        SetVelocity(_velocity + _correctionVelocityLastFrame);
    }

    private void ToggleGrounded()
    {
        if (IsGrounded())
        {
            _rayLength = _stepHeight * 2f;
        }
        else
        {
            _rayLength = _stepHeight;
        }
    }

    private bool IsGrounded()
    {
        hit = Physics2D.Raycast(transform.position, Vector2.down, _rayLength, groundLayer);
        return hit.collider != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 position = transform.position;
        Gizmos.DrawLine(position, position + Vector3.down * _rayLength);
    }
}