using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [SerializeField]
    private WheelComponent wheelFL;

    [SerializeField]
    private WheelComponent wheelFR;

    [SerializeField]
    private WheelComponent wheelRL;

    [SerializeField]
    private WheelComponent wheelRR;

    [SerializeField]
    private float torque;

    private MainInput input;


    


    private void Awake()
    {
        input = new MainInput();
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }


    private void Update()
    {
        Vector2 inputVec = input.Vehicle.BaseMovement.ReadValue<Vector2>();
       
        
        wheelRL.DriveTorque = torque * inputVec.y;
        wheelRR.DriveTorque = torque * inputVec.y;
    }
}
