using UnityEngine;
using UnityEngine.Rendering;

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

    [SerializeField]
    private float maxSteeringAngle = 30.0f;

    [SerializeField] private float wheelBase;

    [SerializeField] private float trackWidth;

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

        float crtSteeringAngle = Mathf.Abs(inputVec.x) * maxSteeringAngle * Mathf.Deg2Rad;

        float insideDegrees = Mathf.Atan2((2.0f * wheelBase * Mathf.Sin(crtSteeringAngle)), (2.0f * wheelBase * Mathf.Cos(crtSteeringAngle) - trackWidth * Mathf.Sin(crtSteeringAngle))) * Mathf.Rad2Deg;
        float outsideDegrees = Mathf.Atan2((2.0f * wheelBase * Mathf.Sin(crtSteeringAngle)), (2.0f * wheelBase * Mathf.Cos(crtSteeringAngle) + trackWidth * Mathf.Sin(crtSteeringAngle))) * Mathf.Rad2Deg;

        if (inputVec.x < 0)
        { 
            wheelFL.transform.localRotation = Quaternion.Euler(wheelFL.transform.localRotation.x, -insideDegrees, wheelFL.transform.localRotation.z);
            wheelFR.transform.localRotation = Quaternion.Euler(wheelFR.transform.localRotation.x, -outsideDegrees, wheelFR.transform.localRotation.z);

        } 
        else if (inputVec.x > 0) 
        {
            wheelFL.transform.localRotation = Quaternion.Euler(wheelFL.transform.localRotation.x, outsideDegrees, wheelFL.transform.localRotation.z);
            wheelFR.transform.localRotation = Quaternion.Euler(wheelFR.transform.localRotation.x, insideDegrees, wheelFR.transform.localRotation.z);
        }
        else
        {
            wheelFL.transform.localRotation = Quaternion.Euler(wheelFL.transform.localRotation.x, 0.0f, wheelFL.transform.localRotation.z);
            wheelFR.transform.localRotation = Quaternion.Euler(wheelFR.transform.localRotation.x, 0.0f, wheelFR.transform.localRotation.z);
        }
    }
}
