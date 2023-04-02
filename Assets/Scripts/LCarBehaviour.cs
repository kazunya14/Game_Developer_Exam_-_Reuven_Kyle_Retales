using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(LCarController))]
public class LCarBehaviour : NetworkBehaviour {
    [Header("Wheel Settings")]
    [Range(0f, 180f)]
    [Tooltip("The maximum angle the wheels can steer")]
    [SerializeField] private float maxWheelSteeringAngle = 30f;
    [Tooltip("The offset of the wheel rotation, in case the wheel model is not rotated correctly")]
    [SerializeField] private Vector3 wheelModelRotationOffset = new(0f, -90f, 0f);

    [Header("Car Body Settings")]
    [Range(0f, 90f)]
    [Tooltip("This controls how much the car body will roll")]
    [SerializeField] private float maxCarBodyRollAngle = 12f;
    [Tooltip("The offset of the car body rotation, in case the car body model is not rotated correctly")]
    [SerializeField] private Vector3 carBodyModelRotationOffset = new(0f, -90f, 0f);

    [Header("Vehicle parts")]
    [Tooltip("Put the front wheels here, the order doesn't matter")]
    [SerializeField] private GameObject[] frontWheels;
    [Tooltip("Put the wheel model objects here, the order doesn't matter")]
    [SerializeField] private GameObject[] wheelModels;
    [SerializeField] private GameObject carBodyModel;
    private float _brakeSuspensionCompressionValue;

    private LCarController _lCarController;

    private float _steeringBodyRollValue;

    private Vector3 CarBodyLocalEulerAngles {
        set => carBodyModel.transform.localEulerAngles = value;
    }

    private float CurrentSteeringAmount => _lCarController.CurrentSteeringAmount;
    private float NormalizedMagnitude => _lCarController.NormalizedMagnitude;
    private bool ItIsApplicationFocused => _lCarController.ItIsApplicationFocused;

    private void Start() {
        _lCarController = GetComponent<LCarController>();
    }

    private void Update() {
        if(!IsOwner || !ItIsApplicationFocused) return;

        CarBodyRoll();
        RotateWheels();
        SteerWheels();
    }

    private void SteerWheels() {
        var steeringAngle = CurrentSteeringAmount * maxWheelSteeringAngle * Vector3.up;

        foreach(var frontWheel in frontWheels)
            frontWheel.transform.localEulerAngles = steeringAngle + wheelModelRotationOffset;
    }

    private void RotateWheels() { }

    private void CarBodyRoll() {
        _steeringBodyRollValue = Mathf.Lerp(_steeringBodyRollValue, CurrentSteeringAmount, Time.deltaTime);

        CarBodyLocalEulerAngles = carBodyModelRotationOffset + _steeringBodyRollValue * maxCarBodyRollAngle *
            Mathf.Abs(NormalizedMagnitude) * Vector3.right;
    }
}