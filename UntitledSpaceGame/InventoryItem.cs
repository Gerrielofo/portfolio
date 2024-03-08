using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Unity.VisualScripting;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Image image;
    [SerializeField] TMP_Text countText;

    public Item item;
    public int count = 1;

    [HideInInspector] public Transform parentAfterDrag;

    [SerializeField] bool isDragging;

    [SerializeField] InventorySlot lastInventorySlot;

    [SerializeField] bool dropOnDrop;


    public void InitializeItem(Item newItem, int amount)
    {
        if (newItem == null)
        {
            Debug.LogError("No Item To Initialize!");
            return;
        }
        count = amount;
        item = newItem;
        image.sprite = newItem.image;
        RefreshCount();
        GetComponentInParent<InventorySlot>().SetInventoryItem(this);
        lastInventorySlot = GetComponentInParent<InventorySlot>();
    }

    public void RefreshCount()
    {
        if (count == 0)
        {
            Destroy(gameObject);
        }
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
        Debug.Log("Refreshed Count Of " + item);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dropOnDrop)
        {
            Debug.Log($"Dropping {count} {item.name}");

            InventoryManager.Instance.DropItem(item.itemID, count);
            Destroy(gameObject);
        }
        InventorySlot slot = transform.GetComponentInParent<InventorySlot>();
        slot.SetInventoryItem(null);
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        isDragging = true;
        InventoryManager.Instance.heldItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        InventoryManager.Instance.UpdateItemsInfoList();

        if (lastInventorySlot == null)
        {
            return;
        }
        else if (lastInventorySlot.isMachineSlot)
        {
            if (MiningPanelManager.Instance.currentDigger != null)
            {
                MiningPanelManager.Instance.currentDigger.ItemAmount = 0;
            }
            else if (SmeltingPanelManager.Instance.currentSmelter != null)
            {
                SmeltingPanelManager.Instance.currentSmelter.OutputAmount = 0;
            }
        }
        else if (lastInventorySlot.isFuelSlot)
        {
            if (MiningPanelManager.Instance.currentDigger != null)
            {
                MiningPanelManager.Instance.currentDigger.FuelAmount = 0;
            }
            else if (SmeltingPanelManager.Instance.currentSmelter != null)
            {
                SmeltingPanelManager.Instance.currentSmelter.FuelAmount = 0;
            }
        }
        else if (lastInventorySlot.isResourceSlot)
        {
            if (SmeltingPanelManager.Instance.currentSmelter != null)
            {
                SmeltingPanelManager.Instance.currentSmelter.ResourceAmount = 0;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -15);
        InventoryManager.Instance.heldItem = eventData.pointerDrag.GetComponent<InventoryItem>();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dropOnDrop)
        {
            InventoryManager.Instance.DropItem(item.itemID, count);
            lastInventorySlot = null;

            Destroy(gameObject);
        }
        else if (parentAfterDrag.childCount == 0)
        {
            image.raycastTarget = true;
            transform.SetParent(parentAfterDrag);
            transform.position = parentAfterDrag.position;
            isDragging = false;
            InventoryManager.Instance.heldItem = null;
            GetComponentInParent<InventorySlot>().SetInventoryItem(this);
            InventoryManager.Instance.UpdateItemsInfoList();
            if (parentAfterDrag.GetComponent<InventorySlot>().isFuelSlot)
            {
                if (MiningPanelManager.Instance.currentDigger != null)
                {
                    MiningPanelManager.Instance.currentDigger.InitializeFuelType();
                }
                else if (SmeltingPanelManager.Instance.currentSmelter != null)
                {
                    SmeltingPanelManager.Instance.currentSmelter.InitializeFuelType();
                }
            }
            lastInventorySlot = GetComponentInParent<InventorySlot>();
        }
        else if (parentAfterDrag != null)
        {
            if (parentAfterDrag.GetComponent<InventorySlot>().GetInventoryItem().count + count <= item.maxStack)
            {
                parentAfterDrag.GetComponent<InventorySlot>().GetInventoryItem().count += count;
            }
            else
            {
                int overflow = parentAfterDrag.GetComponent<InventorySlot>().GetInventoryItem().count + count - parentAfterDrag.GetComponent<InventorySlot>().GetInventoryItem().item.maxStack;
                parentAfterDrag.GetComponent<InventorySlot>().GetInventoryItem().count = item.maxStack;
                InventoryManager.Instance.AddItem(item.itemID, overflow);
            }
            parentAfterDrag.GetComponent<InventorySlot>().GetInventoryItem().RefreshCount();
            InventoryManager.Instance.UpdateItemsInfoList();

            Destroy(gameObject);
        }
        else
        {
            InventoryManager.Instance.AddItem(item.itemID, count);
            Destroy(gameObject);
        }
    }

    public void SetItemParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    [Header("UI Raycasting")]
    [SerializeField] List<GraphicRaycaster> _raycaster = new();
    PointerEventData _pointerEventData;
    [SerializeField] EventSystem _eventSystem;

    private void Update()
    {
        if (isDragging)
        {
            if (!_raycaster.Any())
            {
                _raycaster = inventoryManager.graphicRaycasters;
            }
            if (_eventSystem == null)
            {
                _eventSystem = inventoryManager.eventSystem;
            }
            List<RaycastResult> results = new();
            _pointerEventData = new PointerEventData(_eventSystem);
            _pointerEventData.position = transform.position;

            for (int i = 0; i < _raycaster.Count; i++)
            {
                _raycaster[i].Raycast(_pointerEventData, results);
            }

            if (results.Count <= 0)
            {
                dropOnDrop = true;
                Debug.Log("DropOnDrop True");
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    InventoryManager.Instance.DropItem(item.itemID, 1);
                    count--;
                    RefreshCount();
                }
                return;
            }
            if (results[0].gameObject.transform.GetComponent<Button>())
            {
                Debug.Log("DropOnDrop True");
                dropOnDrop = true;
                return;
            }
            dropOnDrop = false;
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                InventorySlot slot;
                results[0].gameObject.transform.TryGetComponent<InventorySlot>(out slot);
                if (slot != null)
                {
                    slot.AddItemToSlot(this);
                    Debug.Log($"Added {item} In Slot {slot.name}");
                }
                else
                {
                    InventoryItem item;
                    results[0].gameObject.transform.TryGetComponent<InventoryItem>(out item);
                    if (item != null)
                    {
                        if (item.count < item.item.maxStack)
                        {
                            item.GetComponentInParent<InventorySlot>().AddItemToSlot(this);
                        }
                        return;
                    }
                    Debug.Log("No Slot Found!");
                }

            }

        }
    }
}
