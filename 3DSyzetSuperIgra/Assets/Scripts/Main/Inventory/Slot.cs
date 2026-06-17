using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class Slot : MonoBehaviour, IPointerClickHandler
{
    public int IdSlot;
    public Image UISlot;
    public TextMeshProUGUI UIName;
    public BaseItem Item;
    
    [Header("Selection Animation")]
    [SerializeField] private float selectedScale = 1.2f; // 120% от исходного размера
    [SerializeField] private float animationSpeed = 5f; // Скорость анимации
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Coroutine scaleCoroutine;
    private bool isSelected = false;
    private Inventory inventory;
    
    void Start()
    {
        inventory = Inventory.instatiate;

        Color color = UISlot.color;
        color.a = 0f;
        UISlot.color = color;
        
        // Сохраняем исходный масштаб
        originalScale = transform.localScale;
        targetScale = originalScale;
    }
    
    public void ClearItem()
    {
        Item = null;
        
        if (UISlot != null)
        {
            UISlot.sprite = null;
            Color color = UISlot.color;
            color.a = 0f;
            UISlot.color = color;
        }
        
        if (UIName != null)
        {
            UIName.text = "";
        }
    }
    
    public void SetItem(BaseItem newItem, string Lang)
    {
        Item = newItem;
        
        if (newItem == null)
        {
            ClearItem();
            return;
        }
        
        if (UISlot != null && newItem.itemIcon != null)
        {
            UISlot.sprite = newItem.itemIcon;
            Color color = UISlot.color;
            color.a = 1f;
            UISlot.color = color;
        }
        
        if (UIName != null && newItem.itemName != null)
        {
            UIName.text = newItem.itemName.GetString(Lang);
        }
    }
    
    public bool IsEmpety()
    {
        return Item == null;
    }
    
    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        
        // Останавливаем текущую анимацию, если она есть
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }
        
        // Запускаем новую анимацию
        scaleCoroutine = StartCoroutine(AnimateScale(isSelected));
    }
    
    private IEnumerator AnimateScale(bool selected)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = selected ? originalScale * selectedScale : originalScale;
        
        float progress = 0f;
        
        while (progress < 1f)
        {
            progress += Time.deltaTime * animationSpeed;
            progress = Mathf.Clamp01(progress);
            
            // Используем плавную интерполяцию (SmoothStep)
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            transform.localScale = Vector3.Lerp(startScale, endScale, smoothProgress);
            
            yield return null;
        }
        
        transform.localScale = endScale;
        scaleCoroutine = null;
    }
    
    // Обработчик клика по слоту
    public void OnPointerClick(PointerEventData eventData)
    {
        inventory.SelectSlot(this);
    }
}