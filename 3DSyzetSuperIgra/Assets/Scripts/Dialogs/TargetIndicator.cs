using UnityEngine;

public class ArrowLineRenderer : MonoBehaviour
{
    public Transform startObject;
    public Transform endObject;
    public Material arrowMaterial;  // Материал с текстурой стрелки
    public float arrowWidth = 0.3f;
    public float textureSpeed = 1f;  // Скорость движения текстуры
    public float textureTileSize = 1f; // Размер одного тайла текстуры (НЕ меняется)
    
    private LineRenderer line;
    private float textureOffset = 0f;
    
    private void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = arrowWidth;
        line.endWidth = arrowWidth;
        line.generateLightingData = true;
        line.textureMode = LineTextureMode.Stretch;
        line.material.mainTextureScale = new Vector2(1, 1);
        line.material = new Material(arrowMaterial); // Создаем копию материала
        line.positionCount = 2;
        line.textureMode = LineTextureMode.Stretch;
    }
    
    private void LateUpdate()
    {
        if (startObject == null || endObject == null) return;
        
        line.SetPosition(0, startObject.position);
        line.SetPosition(1, endObject.position);
        
        // ФИКСИРОВАННЫЙ размер текстуры (не зависит от расстояния)
        float distance = Vector3.Distance(startObject.position, endObject.position);
        
        // Количество повторений текстуры в зависимости от длины
        // Но размер самой текстуры остается постоянным!
        float tileCount = distance / textureTileSize;
        line.material.mainTextureScale = new Vector2(tileCount, 1);
        
        // Двигаем текстуру
        textureOffset += Time.deltaTime * textureSpeed;
        line.material.mainTextureOffset = new Vector2(textureOffset, 0);
        
    }
    
    void OnDestroy()
    {
        if (line != null && line.material != null)
            Destroy(line.material);
    }
}