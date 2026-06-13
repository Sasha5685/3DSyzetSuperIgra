using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Slot[] SlotsInventory;
    public Slot UseSlot;
    public HandItem handItem;
    
    [Header("Throw Settings")]
    public float throwForce = 10f;
    public float throwUpwardForce = 5f;
    public Transform throwPoint; // Точка откуда вылетает предмет
    
    private int currentSelectedSlotIndex = 0;
    private bool isInitialized = false;
    [Header("Sound Settings")]
    [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip KickSound;
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 0.7f;
    void Start()
    {
        StartCoroutine(InitializeInventory());
    }
    
    IEnumerator InitializeInventory()
    {
        // Ждем один кадр, чтобы UI успел инициализироваться
        yield return null;
        
        CleanSlotsArray();
        
        if (SlotsInventory != null && SlotsInventory.Length > 0 && SlotsInventory[0] != null)
        {
            SelectSlot(SlotsInventory[0]);
        }
        
        isInitialized = true;
    }
    
    void CleanSlotsArray()
    {
        if (SlotsInventory == null) return;
        
        List<Slot> validSlots = new List<Slot>();
        foreach (var slot in SlotsInventory)
        {
            if (slot != null)
            {
                validSlots.Add(slot);
            }
        }
        SlotsInventory = validSlots.ToArray();
    }
    
    void Update()
    {
        if (!isInitialized) return;
        if (SlotsInventory == null || SlotsInventory.Length == 0) return;
        
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollWheel > 0)
        {
            SelectPreviousSlot();
        }
        else if (scrollWheel < 0)
        {
            SelectNextSlot();
        }
        
        for (int i = 0; i < Mathf.Min(5, SlotsInventory.Length); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (SlotsInventory[i] != null)
                {
                    SelectSlot(SlotsInventory[i]);
                }
            }
        }
        
        // Бросок предмета на клавишу T
        if (Input.GetKeyDown(KeyCode.T))
        {
            ThrowCurrentItem();
        }
    }
    
    // Новый метод для броска предмета
    public void ThrowCurrentItem()
    {
        // Получаем текущий выбранный слот
        Slot currentSlot = GetCurrentSelectedSlot();
        
        if (currentSlot == null)
        {
            Debug.LogWarning("Нет выбранного слота");
            return;
        }
        
        if (currentSlot.IsEmpety())
        {
            Debug.LogWarning("Выбранный слот пуст");
            return;
        }

        // Получаем предмет
        BaseItem  itemToThrow = currentSlot.Item;
        
        if (itemToThrow == null || itemToThrow.itemPrefab == null)
        {
            Debug.LogWarning("У предмета нет префаба для броска");
            return;
        }
                PlayKickSound();
        // Создаем физический объект для броска
        GameObject thrownItem = Instantiate(itemToThrow.itemPrefab, GetThrowPosition(), Quaternion.identity);
        
        
        // Добавляем физику для броска
        Rigidbody rb = thrownItem.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = thrownItem.AddComponent<Rigidbody>();
        }
        
        // Рассчитываем направление броска
        Vector3 throwDirection = GetThrowDirection();
        
        // Добавляем силу для броска
        rb.AddForce(throwDirection * throwForce + Vector3.up * throwUpwardForce, ForceMode.Impulse);
        
        // Добавляем случайное вращение
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
        
        // Очищаем слот
        currentSlot.SetItem(null);
        
        // Убираем модель из рук
        if (handItem != null && handItem.GetCurrentItem() == itemToThrow)
        {
            handItem.ClearCurrentItem();
        }
        
        Debug.Log($"Выброшен предмет: {itemToThrow.itemName.GetString("Russian")}");
    }
    
    // Получаем позицию для броска
    private Vector3 GetThrowPosition()
    {
        if (throwPoint != null)
        {
            return throwPoint.position;
        }
        
        // Если точка не задана, используем позицию камеры
        Camera cam = Camera.main;
        if (cam != null)
        {
            return cam.transform.position + cam.transform.forward * 1f;
        }
        
        // Если нет камеры, используем позицию инвентаря
        return transform.position + Vector3.up * 1.5f + transform.forward;
    }
    
    // Получаем направление для броска
    private Vector3 GetThrowDirection()
    {
        if (throwPoint != null)
        {
            return throwPoint.forward;
        }
        
        Camera cam = Camera.main;
        if (cam != null)
        {
            return cam.transform.forward;
        }
        
        return transform.forward;
    }
    
    public void SelectNextSlot()
    {
        if (!isInitialized) return;
        if (SlotsInventory == null || SlotsInventory.Length == 0) return;
        
        int startIndex = currentSelectedSlotIndex;
        do
        {
            currentSelectedSlotIndex = (currentSelectedSlotIndex + 1) % SlotsInventory.Length;
            if (SlotsInventory[currentSelectedSlotIndex] != null)
            {
                SelectSlot(SlotsInventory[currentSelectedSlotIndex]);
                return;
            }
        } while (currentSelectedSlotIndex != startIndex);
    }
    
    public void SelectPreviousSlot()
    {
        if (!isInitialized) return;
        if (SlotsInventory == null || SlotsInventory.Length == 0) return;
        
        int startIndex = currentSelectedSlotIndex;
        do
        {
            currentSelectedSlotIndex = (currentSelectedSlotIndex - 1 + SlotsInventory.Length) % SlotsInventory.Length;
            if (SlotsInventory[currentSelectedSlotIndex] != null)
            {
                SelectSlot(SlotsInventory[currentSelectedSlotIndex]);
                return;
            }
        } while (currentSelectedSlotIndex != startIndex);
    }
    
    public void SelectSlot(Slot slot)
    {
        if (slot == null)
        {
            Debug.LogWarning("Попытка выбрать null слот");
            return;
        }
        
        // Снимаем выделение со всех слотов
        if (SlotsInventory != null)
        {
            foreach (var s in SlotsInventory)
            {
                if (s != null)
                {
                    s.SetSelected(false);
                }
            }
        }
        
        // Находим индекс выбранного слота
        for (int i = 0; i < SlotsInventory.Length; i++)
        {
            if (SlotsInventory[i] == slot)
            {
                currentSelectedSlotIndex = i;
                break;
            }
        }
        
        // Выделяем выбранный слот
        slot.SetSelected(true);
        UseSlot = slot;
        
        // Показываем модель предмета в руках
        if (handItem != null)
        {
            if (slot.Item != null)
            {
                handItem.ShowItem(slot.Item);
            }
            else
            {
                handItem.ClearCurrentItem();
            }
        }
    }
    
    public Slot GetCurrentSelectedSlot()
    {
        if (!isInitialized) return null;
        if (currentSelectedSlotIndex >= 0 && currentSelectedSlotIndex < SlotsInventory.Length)
        {
            return SlotsInventory[currentSelectedSlotIndex];
        }
        return null;
    }
    
    public void AddItem(BaseItem  NewItem)
    {
        if (SlotsInventory == null) return;
        PlayPickupSound();
        for (int i = 0; i < SlotsInventory.Length; i++)
        {
            if (SlotsInventory[i] != null && SlotsInventory[i].IsEmpety())
            {
                SlotsInventory[i].SetItem(NewItem);
                return;
            }
        }
        
        Debug.LogWarning("Нет свободных слотов для добавления предмета");
    }
    private void PlayPickupSound()
    {
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position, pickupVolume);
    }
    private void PlayKickSound()
    {
            AudioSource.PlayClipAtPoint(KickSound, Camera.main.transform.position, pickupVolume);
    }
}