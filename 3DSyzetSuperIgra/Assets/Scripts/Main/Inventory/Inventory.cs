using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Inventory : MusicSystem
{
    public Slot[] SlotsInventory;
    public Slot UseSlot;
    public HandItem handItem;
    public static Inventory instatiate;
    
    [Header("Throw Settings")]
    public float throwForce = 10f;
    public float throwUpwardForce = 5f;
    public Transform throwPoint;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip KickSound;
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 0.7f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private int currentSelectedSlotIndex = 0;
    private bool isInitialized = false;
    public GameManager GameManager;

    public GameObject UIInventory;
    public bool BlockInventory;
    
    public GameObject UIKickObject;
    void Awake()
    {
        instatiate = this;
        UIKickObject.SetActive(false);
        InitSystem(sfxMixerGroup);
        
    }

    void Start()
    {
        GameManager = GameManager.instatiate;
        StartCoroutine(InitializeInventory());
    }
    
    IEnumerator InitializeInventory()
    {
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
        if (BlockInventory) return;
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
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            ThrowCurrentItem();
        }
    }
    
    public void ThrowCurrentItem()
    {
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
        BaseItem itemToThrow = currentSlot.Item;
        
        if (itemToThrow == null || itemToThrow.itemPrefab == null)
        {
            Debug.LogWarning("У предмета нет префаба для броска");
            return;
        }
        
        ShotSound(KickSound, pickupVolume);
        
        GameObject thrownItem = Instantiate(itemToThrow.itemPrefab, GetThrowPosition(), Quaternion.identity);
        
        Rigidbody rb = thrownItem.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = thrownItem.AddComponent<Rigidbody>();
        }
        
        Vector3 throwDirection = GetThrowDirection();
        rb.AddForce(throwDirection * throwForce + Vector3.up * throwUpwardForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
                UIKickObject.SetActive(false);
        currentSlot.SetItem(null, null);
        
        if (handItem != null && handItem.GetCurrentItem() == itemToThrow)
        {
            handItem.ClearCurrentItem();
        }
    }
    
    private Vector3 GetThrowPosition()
    {
        if (throwPoint != null)
        {
            return throwPoint.position;
        }
        
        Camera cam = Camera.main;
        if (cam != null)
        {
            return cam.transform.position + cam.transform.forward * 1f;
        }
        
        return transform.position + Vector3.up * 1.5f + transform.forward;
    }
    
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
        UIKickObject.SetActive(false);
        foreach (var s in SlotsInventory)
        {
            if (s != null && s != slot)
            {
                s.SetSelected(false);
            }
        }
        
        for (int i = 0; i < SlotsInventory.Length; i++)
        {
            if (SlotsInventory[i] == slot)
            {
                currentSelectedSlotIndex = i;
                break;
            }
        }
        
        slot.SetSelected(true);
        UseSlot = slot;
        
        if (handItem != null)
        {
            if (slot.Item != null)
            {
                handItem.ShowItem(slot.Item);
                        UIKickObject.SetActive(true);
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
    
    public bool HandItem(string Name)
    {
        for (int i = 0; i < SlotsInventory.Length; i++)
        {            
            if(SlotsInventory[i].Item == null) continue;
            if(SlotsInventory[i].Item.itemName.GetString("en") == Name)
            {
                return true;
            }
        }
        return false;
    }
    
    public void AddItem(BaseItem NewItem)
    {
        if (SlotsInventory == null) return;
        
        if (NewItem.itemName.GetString("en") == "Tire iron")
        {
            InvokeManager.instatiate.SendMessageEvent("PickCrowBar");
        }
        
        ShotSound(pickupSound, pickupVolume);
        
        for (int i = 0; i < SlotsInventory.Length; i++)
        {
            if (SlotsInventory[i] != null && SlotsInventory[i].IsEmpety())
            {
                SlotsInventory[i].SetItem(NewItem, GameManager.Lang);
                
                Slot currentSlot = GetCurrentSelectedSlot();
                if (currentSlot != null && currentSlot == SlotsInventory[i])
                {
                    if (handItem != null)
                    {                        UIKickObject.SetActive(true);
                        handItem.ShowItem(NewItem);
                    }
                }
                return;
            }
        }
        
        Debug.LogWarning("Нет свободных слотов для добавления предмета");
    }
    
    public void RefreshCurrentSlot()
    {
        Slot currentSlot = GetCurrentSelectedSlot();
        if (currentSlot != null && handItem != null)
        {
            if (currentSlot.Item != null)
            {
                handItem.ShowItem(currentSlot.Item);
            }
            else
            {
                handItem.ClearCurrentItem();
            }
        }
    }
}