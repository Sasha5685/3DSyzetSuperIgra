using UnityEngine;
using System.Collections;

public class RearWheelDrive : MonoBehaviour 
{
    private WheelCollider[] wheels;

    public float maxAngle = 30;
    public float maxTorque = 300;
    public GameObject wheelShape;
    
    public ParticleSystem exhaustParticleSystem;
    public float minSpeedForExhaust = 5f;
    public float maxParticleEmission = 30f;
    
    private Rigidbody rb;
    private float currentSpeed;
    
    [Header("Input")]
    public float steerInput;
    public float accelerationInput;
    public float brakeInput;
    
    public void Start()
    {
        wheels = GetComponentsInChildren<WheelCollider>();
        rb = GetComponent<Rigidbody>();
        
        if (exhaustParticleSystem != null)
        {
            var emission = exhaustParticleSystem.emission;
            emission.rateOverTime = 0;
        }

        for (int i = 0; i < wheels.Length; ++i) 
        {
            var wheel = wheels[i];
            if (wheelShape != null)
            {
                var ws = GameObject.Instantiate(wheelShape);
                ws.transform.parent = wheel.transform;
            }
        }
    }

    public void Update()
    {
        // ИСПРАВЛЕНО: Используем значения из полей, а не Input.GetAxis
        float angle = maxAngle * steerInput;
        float torque = maxTorque * accelerationInput;
        float brake = brakeInput;

        foreach (WheelCollider wheel in wheels)
        {
            // a simple car where front wheels steer while rear ones drive
            if (wheel.transform.localPosition.z > 0)
                wheel.steerAngle = angle;

            if (wheel.transform.localPosition.z < 0)
            {
                // Применяем торможение
                if (brake > 0)
                {
                    wheel.brakeTorque = brake * maxTorque;
                    wheel.motorTorque = 0;
                }
                else
                {
                    wheel.brakeTorque = 0;
                    wheel.motorTorque = torque;
                }
            }

            // update visual wheels if any
            if (wheelShape) 
            {
                Quaternion q;
                Vector3 p;
                wheel.GetWorldPose(out p, out q);

                Transform shapeTransform = wheel.transform.GetChild(0);
                shapeTransform.position = p;
                shapeTransform.rotation = q;
                shapeTransform.localScale = new Vector3(1, 1, 1);
            }
        }
        
        UpdateExhaustParticles();
    }
    
    public void Move(float steering, float acceleration, bool braking)
    {
        steerInput = Mathf.Clamp(steering, -1f, 1f);
        accelerationInput = Mathf.Clamp(acceleration, -1f, 1f);
        brakeInput = braking ? 1f : 0f;
    }
    
    private void UpdateExhaustParticles()
    {
        if (exhaustParticleSystem == null) return;
        
        currentSpeed = rb.velocity.magnitude;
        var emission = exhaustParticleSystem.emission;
        
        if (currentSpeed > minSpeedForExhaust)
        {
            float speedFactor = Mathf.Clamp01((currentSpeed - minSpeedForExhaust) / minSpeedForExhaust);
            float emissionRate = speedFactor * maxParticleEmission;
            emission.rateOverTime = emissionRate;
        }
        else
        {
            emission.rateOverTime = 0;
        }
    }
}