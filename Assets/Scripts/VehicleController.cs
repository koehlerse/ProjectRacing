using System;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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

    [SerializeField] private float maxSteeringAngle = 30.0f;
    [SerializeField] private float wheelBase;
    [SerializeField] private float trackWidth;
    [SerializeField] private Transform centerOfMass;

    [Header("Engine & Transmission")]
    [SerializeField] private float brakeForce = 3.0f;
    [SerializeField] private float maxRPM = 6000.0f;
    [SerializeField] private float idleRPM = 1000.0f;
    [SerializeField] private int currentGear = 1;
    [SerializeField] private float[] gearRatios = { 2.66f, 1.78f, 1.3f, 1.0f, 0.74f, 0.5f };
    [SerializeField] private float differentialRatio = 3.42f;
    [SerializeField] private float transmissionEfficiency = 0.7f;
    [SerializeField] private AnimationCurve torqueLookupTable = null;

    [Header("Resistance")]
    [SerializeField] private float coefficientOfFriction = 0.3f;
    [SerializeField] private float frontalAreaOfCar = 2.2f;
    [SerializeField] private float densityOfAir = 1.29f;

    [Header("Debug")]
    [SerializeField] private TMP_Text rpmField = null;
    [SerializeField] private TMP_Text speedField = null;
    
    private MainInput input;
    private Rigidbody rb;
    private float rpm;
    private float throttleInput;
    private float brakeInput;
    private float steeringInput;
    private float driveTorque;
    private float vMax;

    private void Awake()
    {
        input = new MainInput();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        Application.targetFrameRate = 30;
        rb.centerOfMass = centerOfMass.position;

        vMax = (maxRPM * (1.0f / gearRatios[gearRatios.Length - 1]) * (1.0f / differentialRatio)) * wheelFL.Radius * ((2.0f * Mathf.PI) / 60.0f);
        Debug.Log(vMax);
    }


    private void Update()
    {
        Input();
        Steering();
        ApplyDrive();
    }

    private void FixedUpdate()
    {
        ApplyResistance();
        wheelRL.DriveTorque = driveTorque / 2.0f;
        wheelRR.DriveTorque = driveTorque / 2.0f;
    }

    private void Input()
    {
        Vector2 inputVec = input.Vehicle.BaseMovement.ReadValue<Vector2>();
        steeringInput = inputVec.x;

        if (inputVec.y > 0.0f)
        {
            throttleInput = inputVec.y;
        }
        else if (inputVec.y < 0.0f)
        {
            brakeInput = Mathf.Abs(inputVec.y);
        }
        else
        {
            throttleInput = 0.0f;
            brakeInput = 0.0f;
        }
    }
    private void Steering()
    {
        float crtSteeringAngle = Mathf.Abs(steeringInput) * maxSteeringAngle * Mathf.Deg2Rad;

        float insideDegrees = Mathf.Atan2((2.0f * wheelBase * Mathf.Sin(crtSteeringAngle)), (2.0f * wheelBase * Mathf.Cos(crtSteeringAngle) - trackWidth * Mathf.Sin(crtSteeringAngle))) * Mathf.Rad2Deg;
        float outsideDegrees = Mathf.Atan2((2.0f * wheelBase * Mathf.Sin(crtSteeringAngle)), (2.0f * wheelBase * Mathf.Cos(crtSteeringAngle) + trackWidth * Mathf.Sin(crtSteeringAngle))) * Mathf.Rad2Deg;

        if (steeringInput < 0)
        {
            wheelFL.transform.localRotation = Quaternion.Euler(wheelFL.transform.localRotation.x, -insideDegrees, wheelFL.transform.localRotation.z);
            wheelFR.transform.localRotation = Quaternion.Euler(wheelFR.transform.localRotation.x, -outsideDegrees, wheelFR.transform.localRotation.z);

        }
        else if (steeringInput > 0)
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
    private void ApplyDrive()
    {
        //Geschwindigkeit des Rades berechnen

        float wheelSpeed =  (wheelRL.RPM + wheelRR.RPM) / 2 * 60.0f;
        rpm = Mathf.Clamp(wheelSpeed * gearRatios[currentGear] * differentialRatio, idleRPM, maxRPM); 
        rpmField.text = "RPM: " + rpm.ToString();
        speedField.text = "Km/h: " + rb.linearVelocity.magnitude * 3.6f;
        //Gangwechsel
        
        if (rpm >= maxRPM && currentGear < gearRatios.Length - 1)
        {
            currentGear++;
            Debug.Log(currentGear);
        }
        else if (rpm < idleRPM * 1.5f && currentGear > 0)
        {
            currentGear--;
        }

        //Motordrehmoment

        float maxTorque = torqueLookupTable.Evaluate(rpm);
        float engineTorque = throttleInput * maxTorque;
        driveTorque = (engineTorque * gearRatios[currentGear] * differentialRatio * transmissionEfficiency);

        //Bremsen

        if (brakeInput > 0)
        {
            wheelFL.BrakeTorque = brakeInput * brakeForce;
            wheelFR.BrakeTorque = brakeInput * brakeForce;
            wheelRL.BrakeTorque = brakeInput * brakeForce;
            wheelRR.BrakeTorque = brakeInput * brakeForce;
        } 
        else
        {
            wheelFL.BrakeTorque = 0.0f;
            wheelFR.BrakeTorque = 0.0f;
            wheelRL.BrakeTorque = 0.0f;
            wheelRR.BrakeTorque = 0.0f;
        }
    }

    private void ApplyResistance()
    {
        //Luftwiderstand berechnen
        
        float speed = rb.linearVelocity.magnitude;
        float dragForceMagnitude = driveTorque / Mathf.Pow(vMax, 2.0f) * Mathf.Pow(speed, 2.0f);

        Vector3 dragForce = -dragForceMagnitude * rb.linearVelocity.normalized;
        rb.AddForce(dragForce, ForceMode.Force);
    }
}