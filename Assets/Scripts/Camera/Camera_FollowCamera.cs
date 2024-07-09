using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class Camera_FollowCamera : MonoBehaviour
{

    // The Object we want to look at
    [Header("General Settings")]
    public Transform lookAt;
    public Transform Parent;

    [Header("Follow Settings")]
    public bool ClampYDistance = true;
    public float MinYDistance = 25f;
    public float MaxYDistance = 25f;
    public bool ClampXDistance = true;
    public float MinXDistance = -25f;
    public float MaxXDistance = 25f;
    public bool InvertX;
    public bool InvertY;
    private float _locationX;
    private float _locationY;

    private Vector3 movingTo;
    private Vector3 _lookingAt;

    private void Start()
    {
        // This may not work if window is switched
        Setup();
    }

    public void Setup()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        //UpdateInversionSettings();
    }

    public void UpdateInversionSettings()
    {
        var invertions = Manager_Input.Instance.LoadUserInversion();
        if (invertions.Item1 == 0) {
            InvertX = false;
        } else if (invertions.Item1 == 1) {
            InvertX = true;
        }
        if (invertions.Item2 == 0) {
            InvertY = false;
        } else if (invertions.Item2 == 1) {
            InvertY = true;
        }
    }

    private void Update()
    {
        HandleMouseInput();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        
        LateUpdateFollow();
    }

    private void LateUpdateFollow()
    {
        var forward = Parent.forward.normalized;
        // Create Direction and Rotation values for the camera to use
        Vector3 direction = new Vector3(forward.x, forward.y + DistanceAboveCharacter, forward.z + Distance);
        Quaternion rotation = Quaternion.Euler(_locationY, _locationX, 0f);

        // Sets the position of the camera based on the lookAt objects position
        movingTo = transform.position + rotation * direction;
        // Makes camera look at the lookAt object's position
        lookAt.position = movingTo;
        transform.localRotation = rotation;//Quaternion.LookRotation(_lookingAt);
    }

    public void HandleMouseInput()
    {
        //*0.1f used to correct delta value to pre GetAxis Speeds
        var lookAxis = Manager_Input.Instance.Look.ReadValue<Vector2>() * 0.1f;

        if (InvertX) {
            _locationX -= lookAxis.x; //Input.GetAxis("Mouse X");
        } else {
            _locationX += lookAxis.x;
        }
        //_locationX += lookAxis.x; //Input.GetAxis("Mouse X");

        if (_locationX > 360) {
            _locationX = -360;
        } else if (_locationX < -360) {
            _locationX = 360;
        }

        if (InvertY) {
            _locationY += lookAxis.y; //Input.GetAxis("Mouse X");
        } else {
            _locationY -= lookAxis.y;
        }

        if (_locationY > 360) {
            _locationY = -360;
        } else if (_locationY < -360) {
            _locationY = 360;
        }

        //_locationY -= lookAxis.y;//Input.GetAxis("Mouse Y");
        if (ClampYDistance)
            _locationY = Mathf.Clamp(_locationY, MinYDistance, MaxYDistance);
        if (ClampXDistance)
            _locationX = Mathf.Clamp(_locationX, MinXDistance, MaxXDistance);
    }
}
