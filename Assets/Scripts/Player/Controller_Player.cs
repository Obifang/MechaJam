using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Controller_Player : MonoBehaviour
{
    public float MovementSpeed = 1.0f;
    public float GroundCheckDistance = 2.0f;
    public float WallCheckDistance = 2.0f;
    public float RotationSpeedForModel = 1.0f;
    public GameObject Model;

    private Vector2 _moveDirection;
    private Vector3 _previousPosition;
    private Rigidbody _rb;
    private Collider _collider;
    private float Velocity;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (Manager_Input.Instance.RotateRight.IsPressed()) {
            RotateCharacterRight();
        }

        if (Manager_Input.Instance.RotateLeft.IsPressed()) {
            RotateCharacterLeft();
        }
    }

    void FaceTowardsCameraForwardDirection()
    {
        var cameraDir = Camera.main.transform.forward;
        transform.forward = Vector3.ProjectOnPlane(cameraDir, Vector3.up);
    }

    private void HandleMovement()
    {
        _moveDirection = Manager_Input.Instance.Move.ReadValue<Vector2>().normalized;

        if (_rb.velocity.y > 0) {
            _moveDirection *= 0.5f;
        }

        if (_moveDirection == Vector2.zero) {
            //_animator.SetFloat("Velocity", 0);
            return;
        }

        var speed = MovementSpeed * Time.deltaTime;
        var direction = transform.TransformDirection(new Vector3(_moveDirection.x, 0, _moveDirection.y) * speed);


        if (!CheckMovementDirection(direction.normalized)) {
            _previousPosition = transform.position;
            transform.position += direction;
        }

        Velocity = Vector3.Distance(transform.position, _previousPosition);
        //_animator.SetFloat("Velocity", Velocity);
        //_animator.SetFloat("Speed", 1 + MovementSpeed * _combatStates.CurrentStats.MovementSpeedMultiplier * _playerController.Power.CurrentPower * Velocity);
    }

    private bool CheckMovementDirection(Vector3 direction)
    {
        var groundLayer = 1 << LayerMask.NameToLayer("Ground");
        var boundSize = _collider.bounds.size.x;
        var halfWidth = boundSize * 0.5f;


        var center = _collider.bounds.center;
        var left = new Vector3(center.x - halfWidth, center.y, center.z);
        var right = new Vector3(center.x + halfWidth, center.y, center.z);

        var leftHit = Physics.Raycast(left, direction, boundSize * WallCheckDistance, groundLayer);
        var rightHit = Physics.Raycast(right, direction, boundSize * WallCheckDistance, groundLayer);
        var centerHit = Physics.Raycast(center, direction, boundSize * WallCheckDistance, groundLayer);
        Debug.DrawRay(left, direction, Color.green);
        Debug.DrawRay(right, direction, Color.green);
        Debug.DrawRay(center, direction, Color.green);

        return (centerHit || leftHit || rightHit);
    }

    public void RotateCharacterRight()
    {
        transform.Rotate(new Vector3(0, Time.deltaTime * RotationSpeedForModel, 0));
        Debug.Log("Rotating");
    }

    public void RotateCharacterLeft()
    {
        transform.Rotate(new Vector3(0, -Time.deltaTime * RotationSpeedForModel, 0));
    }

    private bool IsGrounded()
    {
        var groundLayer = 1 << LayerMask.NameToLayer("Ground");
        var halfWidth = _collider.bounds.size.x * 0.5f;

        var center = _collider.bounds.center;
        var left = new Vector3(center.x - halfWidth, center.y, center.z);
        var right = new Vector3(center.x + halfWidth, center.y, center.z);

        var leftHit = Physics.Raycast(left, -transform.up, GroundCheckDistance, groundLayer);
        var rightHit = Physics.Raycast(right, -transform.up, GroundCheckDistance, groundLayer);
        var centerHit = Physics.Raycast(center, -transform.up, GroundCheckDistance, groundLayer);

        Debug.DrawLine(left, new Vector3(left.x, left.y - GroundCheckDistance, left.z), Color.red);
        Debug.DrawLine(center, new Vector3(center.x, center.y - GroundCheckDistance, center.z), Color.red);
        Debug.DrawLine(right, new Vector3(right.x, right.y - GroundCheckDistance, right.z), Color.red);

        return (leftHit && rightHit && centerHit);
    }
}
