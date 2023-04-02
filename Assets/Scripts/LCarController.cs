using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class LCarController : NetworkBehaviour {
    private enum GearDirection { Forward, Backward }
    
#region SERIALIZED_FIELDS

    [Header("Engine")]
    [Tooltip("This controls how fast the car can go")]
    [SerializeField] private float maxVelocity = 20f;
    [Tooltip("This controls how long it takes to reach max velocity when moving forward")]
    [SerializeField] private float positiveAcceleration = 100f;
    [Tooltip("This controls how long it takes to reach max velocity when moving backward")]
    [SerializeField] private float negativeAcceleration = 50f;
    [Tooltip("This controls how fast the car can turn")]
    [SerializeField] private float steeringSpeed = 100f;
    [Tooltip("This controls the minimum speed the car needs to be moving to be able to move backward")]
    [SerializeField] private float breakToReverseThreshold = 0.1f;
    [Tooltip("This controls how fast the car will stop when moving forward")]
    [SerializeField] private float brakeForce = 10f;

#endregion

#region PRIVATE_FIELDS

    private GearDirection _gearDirection;

    private Rigidbody _rigidbody;
    private Collider _collider;
    private CinemachineVirtualCamera _virtualCamera;

#endregion

#region PUBLIC_PROPERTIES

    public float CurrentGasPedalAmount { get; private set; }
    public float CurrentSteeringAmount { get; private set; }
    public bool ItIsApplicationFocused { get; private set; }
    public bool IsGameStarted { get; private set; }

    public float NormalizedMagnitude => Mathf.InverseLerp(0f, maxVelocity, Mathf.Abs(Magnitude));

#endregion

#region PRIVATE_PROPERTIES

    private float Magnitude => _rigidbody.velocity.magnitude;
    private Vector3 ForwardDirection => transform.forward;
    private Vector3 BackwardDirection => -transform.forward;
    private bool ItIsOwner => IsOwner;

    private Vector3 Velocity {
        get => _rigidbody.velocity;
        set => _rigidbody.velocity = value;
    }

#endregion

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    private void Start() {
        if(!IsOwner) return;

        ItIsApplicationFocused = true;
        InitializeCamera();
        SubscribeToEvents();
    }
    
    private void SubscribeToEvents() {
        GameEvents.OnGameStart += OnGameStart;
        GameEvents.OnPlayerSpawn += OnPlayerSpawn;
    }
    
    private void UnsubscribeFromEvents() {
        GameEvents.OnGameStart -= OnGameStart;
        GameEvents.OnPlayerSpawn -= OnPlayerSpawn;
    }

    private void FixedUpdate() {
        if(!ItIsOwner || !ItIsApplicationFocused || !IsGameStarted) return;

        HandleAcceleration(CurrentGasPedalAmount);
        HandleSteering(CurrentSteeringAmount);
    }

#region LOGIC

    public void HandleMovement(InputAction.CallbackContext context) {
        var input = context.ReadValue<Vector2>();
        CurrentGasPedalAmount = input.y;
        CurrentSteeringAmount = input.x;
    }

    private void HandleAcceleration(float currentGasPedalAmount) {
        switch(currentGasPedalAmount) {
            case > 0:
                Accelerate(currentGasPedalAmount);
                _gearDirection = GearDirection.Forward;
                break;

            case < 0 when ItIsReversible():
                _gearDirection = GearDirection.Backward;
                Accelerate(Math.Abs(currentGasPedalAmount));
                break;

            case < 0 when !ItIsReversible():
                Brake();
                break;
        }
    }

    private void HandleSteering(float currentSteeringAmount) {
        if(currentSteeringAmount == 0) return;

        Steer(currentSteeringAmount);
    }

    private void Accelerate(float currentGasPedalAmount) {
        // Because the car is already moving at max speed
        if(Magnitude > maxVelocity) return;

        // Because the car can move forward or backward
        var acceleration = _gearDirection == GearDirection.Forward ? positiveAcceleration : negativeAcceleration;
        var forceDirection = _gearDirection == GearDirection.Forward ? ForwardDirection : BackwardDirection;

        var force = currentGasPedalAmount * acceleration;

        Velocity += forceDirection * (force * Time.fixedDeltaTime);
    }

    private void Brake() {
        // Because the car is not moving
        if(Magnitude < 0 && _gearDirection == GearDirection.Backward) return;

        Velocity -= ForwardDirection * (brakeForce * Time.fixedDeltaTime);
    }

    private void Steer(float currentSteeringAmount) {
        var steeringAmount = currentSteeringAmount * Time.fixedDeltaTime * steeringSpeed;

        if(_gearDirection == GearDirection.Backward)
            // Because direction is inverted when moving backwards
            steeringAmount = -steeringAmount;

        transform.Rotate(steeringAmount * NormalizedMagnitude * Vector3.up);
    }

#endregion

#region METHODS_AND_HELPERS

    private void InitializeCamera() {
        _virtualCamera.Follow = transform;
        _virtualCamera.LookAt = transform;
    }
    
    private bool ItIsReversible() {
        return Magnitude <= breakToReverseThreshold || _gearDirection == GearDirection.Backward;
    }

#endregion

#region EVENTS

    private void OnApplicationFocus(bool hasFocus) {
        ItIsApplicationFocused = hasFocus;
    }

    private void OnGameStart() {
        IsGameStarted = true;
    }

    private void OnPlayerSpawn(Transform spawnPoint, ulong clientID) {
        if(clientID != OwnerClientId) return;
        
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }

    public override void OnDestroy() {
        base.OnDestroy();

        UnsubscribeFromEvents();
    }

#endregion
}