using UnityEngine;

public class WheelComponent : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private Rigidbody vehicleRB;
    [SerializeField] private Transform wheelMesh;
    [SerializeField] private float mass = 10.0f, radius = 0.5f;
    [SerializeField] private LayerMask drivableLayers;

    public float DriveTorque { get => driveTorque; set => driveTorque = value; }
    public float BrakeTorque { get => brakeTorque; set => brakeTorque = value; }

    [Header("Grip")]
    [SerializeField] private float gripFactor = 0.4f; // TODO: Lookup table

    [Header("Suspension")]
    [SerializeField] private float suspensionRestingDist = 0.5f;
    [SerializeField] private float strength = 10000.0f, damping = 300.0f;

    [Header("Debug")]
    [SerializeField] private Color debugWheelColor = Color.green;
    [SerializeField] private Color perpendicularForceColor = Color.red;
    [SerializeField] private bool drawForces;

    private float driveTorque, brakeTorque;
    private bool grounded = false;
    private float groundDist;
    private float crtTorque;
    private Vector3 perpForce;

    private void Update()
    {
        CheckWheelGrounded();
    }

    private void CheckWheelGrounded()
    {
        grounded = Physics.Raycast(transform.position, -vehicleRB.transform.up, out RaycastHit hit, radius, drivableLayers);
        groundDist = hit.distance;
    }

    private void FixedUpdate()
    {
        Vector3 tireWorldVelocity = vehicleRB.GetPointVelocity(transform.position);
        float forwardVelocity = Vector3.Dot(transform.forward, tireWorldVelocity);

        float distanceTraveled = forwardVelocity * Time.fixedDeltaTime;
        float angularDistance = distanceTraveled / (2.0f * Mathf.PI * radius);
        wheelMesh.transform.Rotate(angularDistance * 360.0f, 0.0f, 0.0f);

        if (!grounded) { return; }

        CalcSuspensionForce();
        CalcPerpForce();
        CalcAccelForce();
        CalcBrakeForce();
    }

    private void CalcSuspensionForce()
    {
        Vector3 springDir = transform.up;
        Vector3 tireWorldVelocity = vehicleRB.GetPointVelocity(transform.position);
        float offset = suspensionRestingDist - groundDist;
        float velocity = Vector3.Dot(springDir, tireWorldVelocity);
        float force = (offset * strength) - (velocity * damping);
        vehicleRB.AddForceAtPosition(springDir * force, transform.position);
        Debug.Log(groundDist);

    }

    private void CalcPerpForce()
    {
        Vector3 wheelRight = transform.right;
        Vector3 tireWorldVelocity = vehicleRB.GetPointVelocity(transform.position);
        float steeringVelocity = Vector3.Dot(wheelRight, tireWorldVelocity);
        float desiredVelocityChange = -steeringVelocity * gripFactor;
        float desiredAccel = desiredVelocityChange / Time.fixedDeltaTime;
        perpForce = desiredAccel * mass * wheelRight;
        vehicleRB.AddForceAtPosition(perpForce, transform.position);
    }

    private void CalcAccelForce()
    {
        Vector3 accelDir = transform.forward;
        vehicleRB.AddForceAtPosition(accelDir * driveTorque, transform.position);
    }

    private void CalcBrakeForce()
    {
        Vector3 tireWorldVelocity = vehicleRB.GetPointVelocity(transform.position);
        Vector3 brakeForceDir = -Vector3.Project(tireWorldVelocity, transform.forward.normalized).normalized;
        vehicleRB.AddForceAtPosition(brakeForceDir * brakeTorque, transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = debugWheelColor;
        Vector3 startPoint = transform.position + transform.right * radius;
        Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one);
        int res = 32;

        for (int i = 0; i <= res; i++)
        {
            float angle = i * 2.0f * Mathf.PI / res;
            Vector3 localEndPoint = new Vector3(0.0f, Mathf.Sin(angle), Mathf.Cos(angle)) * radius;
            Vector3 endPoint = rotMatrix.MultiplyPoint3x4(localEndPoint) + transform.position;
            Gizmos.DrawLine(startPoint, endPoint);
            startPoint = endPoint;
        }

        if (drawForces)
        {
            Gizmos.color = perpendicularForceColor;
            Gizmos.DrawLine(transform.position, transform.position + perpForce);
        }
    }
}
