using TMPro;
using UnityEngine;

public class FPS_Counter : MonoBehaviour
{
    [Header("Настройки FPS")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f; // Как часто обновлять текст
    
    [Header("Цвета (зелёный - жёлтый - красный)")]
    [SerializeField] private Color goodColor = Color.green;    // 50+ FPS
    [SerializeField] private Color normalColor = Color.yellow; // 30-49 FPS
    [SerializeField] private Color badColor = Color.red;       // ниже 30 FPS
    
    [Header("Пороги")]
    [SerializeField] private int goodThreshold = 50;   // выше этого - зелёный
    [SerializeField] private int normalThreshold = 30; // выше этого - жёлтый, иначе красный
    
    private float deltaTime = 0f;
    private float timer = 0f;
    
    // Простой вариант (без усреднения)
    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        
        timer += Time.unscaledDeltaTime;
        if (timer >= updateInterval)
        {
            float fps = 1f / deltaTime;
            UpdateFPSDisplay(fps);
            timer = 0f;
        }
    }
    
    private void UpdateFPSDisplay(float fps)
    {
        int fpsInt = Mathf.RoundToInt(fps);
        
        // Выбираем цвет
        Color targetColor;
        if (fpsInt >= goodThreshold)
            targetColor = goodColor;
        else if (fpsInt >= normalThreshold)
            targetColor = normalColor;
        else
            targetColor = badColor;
        
        // Обновляем текст
        if (fpsText != null)
        {
            fpsText.text = $"FPS: {fpsInt}";
            fpsText.color = targetColor;
        }
    }
    
}