using UnityEngine;
using System.Collections;

public class RearWheelDrive : MonoBehaviour 
{
    private WheelCollider[] wheels;

    public float maxAngle = 30;
    public float maxTorque = 300;
    public GameObject wheelShape;
    
    // Поля для системы частиц выхлопа
    public ParticleSystem exhaustParticleSystem;  // Ссылка на компонент ParticleSystem
    public float minSpeedForExhaust = 5f;         // Минимальная скорость для появления частиц
    public float maxParticleEmission = 30f;       // Максимальная скорость эмиссии частиц
    
    private Rigidbody rb;
    private float currentSpeed;

    // here we find all the WheelColliders down in the hierarchy
    public void Start()
    {
        wheels = GetComponentsInChildren<WheelCollider>();
        rb = GetComponent<Rigidbody>();
        
        // Инициализируем систему частиц
        if (exhaustParticleSystem != null)
        {
            var emission = exhaustParticleSystem.emission;
            emission.rateOverTime = 0; // Начинаем с нулевой эмиссии
        }

        for (int i = 0; i < wheels.Length; ++i) 
        {
            var wheel = wheels[i];

            // create wheel shapes only when needed
            if (wheelShape != null)
            {
                var ws = GameObject.Instantiate(wheelShape);
                ws.transform.parent = wheel.transform;
            }
        }
    }

    // this is a really simple approach to updating wheels
    // here we simulate a rear wheel drive car and assume that the car is perfectly symmetric at local zero
    // this helps us to figure our which wheels are front ones and which are rear
    public void Update()
    {
        float angle = maxAngle * Input.GetAxis("Horizontal");
        float torque = maxTorque * Input.GetAxis("Vertical");

        foreach (WheelCollider wheel in wheels)
        {
            // a simple car where front wheels steer while rear ones drive
            if (wheel.transform.localPosition.z > 0)
                wheel.steerAngle = angle;

            if (wheel.transform.localPosition.z < 0)
                wheel.motorTorque = torque;

            // update visual wheels if any
            if (wheelShape) 
            {
                Quaternion q;
                Vector3 p;
                wheel.GetWorldPose(out p, out q);

                // assume that the only child of the wheelcollider is the wheel shape
                Transform shapeTransform = wheel.transform.GetChild(0);
                shapeTransform.position = p;
                shapeTransform.rotation = q;
                shapeTransform.localScale = new Vector3(1, 1, 1);
            }
        }
        
        // Обновляем систему частиц выхлопа
        UpdateExhaustParticles();
    }
    
    private void UpdateExhaustParticles()
    {
        if (exhaustParticleSystem == null) return;
        
        // Вычисляем текущую скорость автомобиля (в км/ч для наглядности или м/с)
        currentSpeed = rb.velocity.magnitude;
        
        var emission = exhaustParticleSystem.emission;
        
        // Включаем частицы только когда скорость выше минимальной
        if (currentSpeed > minSpeedForExhaust)
        {
            // Рассчитываем интенсивность частиц в зависимости от скорости
            // Чем быстрее едет машина, тем больше частиц
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