using Cinemachine;
using QFSW.QC;
using RK.Retales.Utility;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class LCarController : NetworkBehaviour {
    // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
    private NetworkVariable<bool> _isGameStarted = new NetworkVariable<bool>(false, EveryoneCanRead);

    // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
    private NetworkVariable<int> _numberOfPlayers = new NetworkVariable<int>(0, EveryoneCanRead);

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        _quantumConsole = FindObjectOfType<QuantumConsole>();
    }

    private void Start() {
        if(!IsOwner) return;

        ItIsApplicationFocused = true;
        InitializeCamera();
    }

    private void Update() {
        if(!IsServer) return;

        _numberOfPlayers.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }

    private void FixedUpdate() {
        if(!IsOwner || !ItIsApplicationFocused || !ItIsGameStarted) return;

        HandleAcceleration(CurrentGasPedalAmount);
        HandleSteering(CurrentSteeringAmount);
    }

    private void InitializeCamera() {
        _virtualCamera.Follow = transform;
        _virtualCamera.LookAt = transform;
    }

    private void InitializePlayer(int playerNumber) {
        if(!IsOwner || playerNumber <= 0 || _playerNumberAssigned) return;

        _playerNumber = playerNumber;
        _playerNumberAssigned = true;

        name = $"Player {_playerNumber}";
    }

    private void MoveToStartingPosition() {
        if(name != $"Player {_playerNumber}") return;
        _gameManager??= FindObjectOfType<GameManager>();
        
        _spawnPoint = SpawnPointHandler(_playerNumber);
        
        gameObject.SetActive(false); // To avoid any weird physics
        transform.position = _spawnPoint.transform.position;
        transform.rotation = _spawnPoint.transform.rotation;
        gameObject.SetActive(true);
    }

    private Transform SpawnPointHandler(int playerNumber) {
        return _gameManager.SpawnPointHandler(playerNumber - 1);
    }

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
    private int _playerNumber;
    private bool _playerNumberAssigned;
    
    private Transform _spawnPoint;
    private Rigidbody _rigidbody;
    private CinemachineVirtualCamera _virtualCamera;
    private GameManager _gameManager;
    private QuantumConsole _quantumConsole;

#endregion

#region PROPERTIES

    // Read-write properties
    public float CurrentGasPedalAmount { get; private set; }
    public float CurrentSteeringAmount { get; private set; }
    public bool ItIsApplicationFocused { get; private set; }
    public bool ItIsGameStarted { get; private set; }

    private Vector3 Velocity {
        get => _rigidbody.velocity;
        set => _rigidbody.velocity = value;
    }

    // Read-only properties
    public float NormalizedMagnitude => Mathf.InverseLerp(0f, maxVelocity, Mathf.Abs(Magnitude));
    private float Magnitude => _rigidbody.velocity.magnitude;
    private Vector3 ForwardDirection => transform.forward;
    private Vector3 BackwardDirection => -transform.forward;
    private static NetworkVariableReadPermission EveryoneCanRead => NetworkVariableReadPermission.Everyone;

    private bool ItIsReversible =>
        Magnitude <= breakToReverseThreshold || _gearDirection == GearDirection.Backward;

#endregion

#region LOGIC
    
    // TODO: Implement WheelCollider Movement

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

            case < 0 when ItIsReversible:
                _gearDirection = GearDirection.Backward;
                Accelerate(Mathf.Abs(currentGasPedalAmount));
                break;

            case < 0 when !ItIsReversible:
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
        
        

        transform.Rotate(steeringAmount * NormalizedMagnitude * transform.up);
    }

#endregion

#region EVENTS

    public override void OnNetworkSpawn() {
        _isGameStarted.OnValueChanged += OnGameStartTriggered;
        _numberOfPlayers.OnValueChanged += OnNumberOfPlayersChanged;
    }

    private void OnNumberOfPlayersChanged(int oldValue, int newValue) {
        InitializePlayer(newValue);
        MoveToStartingPosition();
        GameEvents.InvokeOnPlayerJoined(newValue);
        
        LogHandler.StaticLog($"{name}: Player count has changed from {oldValue} to {newValue}", Color.blue, this);
    }

    private void OnGameStartTriggered(bool oldValue, bool newValue) {
        ItIsGameStarted = true;
        GameEvents.InvokeOnGameStart();

        LogHandler.StaticLog($"{name}: Game started: {newValue}", Color.blue, this);
    }

    private void UnsubscribeFromEvents() {
        _numberOfPlayers.OnValueChanged -= OnNumberOfPlayersChanged;
        _isGameStarted.OnValueChanged -= OnGameStartTriggered;
    }

    public override void OnDestroy() {
        base.OnDestroy();

        UnsubscribeFromEvents();
    }
    private void OnApplicationFocus(bool hasFocus) {
        ItIsApplicationFocused = hasFocus;
    }

    public void OnToggleConsole() {
        _quantumConsole.Toggle();
    }

    public void StartGame() {
        if(!IsServer) return;

        _isGameStarted.Value = true;
        
        LogHandler.StaticLog($"{name}: Host has started the game", Color.green, this);
    }
    
    #endregion
    
}