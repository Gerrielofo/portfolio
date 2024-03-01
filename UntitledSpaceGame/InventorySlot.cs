using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler, IDataPersistence
{
    [Header("Slot Settings")]
    public int slotId;

    public Image image;
    public Color selectedColor, notSelectedColor;

    [SerializeField] InventoryItem _itemInThisSlot;

    [Header("Slot Type")]
    public bool isHudSlot;
    public bool isMachineSlot;
    public bool isFuelSlot;
    public bool isResourceSlot;
    private void Awake()
    {
        Deselect();
    }

    public void Select()
    {
        image.color = selectedColor;
    }

    public void Deselect()
    {
        image.color = notSelectedColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (isMachineSlot)
        {
            Debug.Log("Can't Place Items Into The Machine");
            return;
        }
        else if (isFuelSlot && !InventoryManager.Instance.heldItem.item.isFuel)
        {
            Debug.Log("This Item Cannot Be Used As Fuel.");
            return;
        }
        else if (isHudSlot && !InventoryManager.Instance.heldItem.item.canBeInHudSlot)
        {
            Debug.Log("This Item Is Not Allowed In This Slot! Change This In The Inspector");
            return;
        }

        if (transform.childCount == 0)
        {
            if (InventoryManager.Instance.heldItem != null)
            {
                InventoryManager.Instance.heldItem.parentAfterDrag = transform;
                if (InventoryManager.Instance.heldItem.item.isFuel && isFuelSlot)
                {

                }
            }

        }
        else
        {
            _itemInThisSlot = transform.GetChild(0).GetComponent<InventoryItem>();

            if (InventoryManager.Instance.heldItem.item == _itemInThisSlot.item)
            {
                if (_itemInThisSlot.count < _itemInThisSlot.item.maxStack)
                {
                    int spaceLeft = _itemInThisSlot.item.maxStack - _itemInThisSlot.count;
                    int overFlow = InventoryManager.Instance.heldItem.count - spaceLeft;
                    if (overFlow > 0)
                    {
                        InventoryManager.Instance.heldItem.count = overFlow;
                        _itemInThisSlot.count = _itemInThisSlot.item.maxStack;
                        _itemInThisSlot.RefreshCount();
                        InventoryManager.Instance.heldItem.RefreshCount();
                        return;
                    }
                    _itemInThisSlot.count += InventoryManager.Instance.heldItem.count;
                    Destroy(InventoryManager.Instance.heldItem.gameObject);
                    _itemInThisSlot.RefreshCount();
                }
                else
                {
                    Debug.Log("This Slot Has Reached It's Max Stack!");
                }
            }
            else
            {
                Debug.Log("You Can't Stack These Items Together!");
            }

        }
        // _itemInThisSlot.GetComponent<Image>().raycastTarget = true;
        InventoryManager.Instance.UpdateItemsInfoList();
    }


    public void AddItemToSlot(InventoryItem inventoryItem)
    {
        // heldItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (_itemInThisSlot != null)
        {
            if (InventoryManager.Instance.heldItem.item == _itemInThisSlot.item)
            {
                if (_itemInThisSlot.count < _itemInThisSlot.item.maxStack)
                {
                    _itemInThisSlot.count++;
                    InventoryManager.Instance.heldItem.count--;
                    if (inventoryItem.count <= 0)
                    {
                        Destroy(InventoryManager.Instance.heldItem.gameObject);
                    }
                    _itemInThisSlot.RefreshCount();
                    InventoryManager.Instance.heldItem.RefreshCount();
                }
                else
                {
                    Debug.Log("This Slot Has Reached It's Max Stack!");
                }
            }
            else
            {
                Debug.Log("You Can't Stack These Items Together!");
            }
        }
        else
        {
            InventoryManager.Instance.SpawnNewItem(inventoryItem.item.itemID, 1, slotId);
            InventoryManager.Instance.heldItem.count--;
            InventoryManager.Instance.heldItem.RefreshCount();
            Debug.Log($"{InventoryManager.Instance.heldItem.count}");
            Debug.Log("Succesfully Spawned New Item In Slot: " + gameObject.name);
        }
    }

    public InventoryItem GetInventoryItem()
    {
        return _itemInThisSlot;
    }

    public void SetInventoryItem(InventoryItem newItem)
    {
        _itemInThisSlot = newItem;
    }

    public void UseItem()
    {
        if (_itemInThisSlot.count > 1)
        {
            _itemInThisSlot.count--;
            _itemInThisSlot.RefreshCount();
        }
        else
        {
            Destroy(_itemInThisSlot.gameObject);
        }

    }

    public void LoadData(GameData data)
    {
        if (data.itemId[slotId] == -1)
        {
            return;
        }
        if (isMachineSlot || isFuelSlot || isResourceSlot)
        {
            // InventoryManager.Instance.SpawnNewItemMining(data.itemId[slotId], data.itemAmount[slotId], this.slotId);
            return;
        }
        else
        {
            InventoryManager.Instance.SpawnNewItem(data.itemId[slotId], data.itemAmount[slotId], this.slotId);
            Debug.Log($"Spawned New Item From Slot {slotId} On GameObject {gameObject.name}");
        }
    }

    public void SaveData(GameData data)
    {
        if (isMachineSlot || isFuelSlot || isResourceSlot)
        {
            //Not needed to save and load
            return;
        }
        if (_itemInThisSlot == null)
        {
            // Debug.Log("didnt find Item in slot: " + slotId);

            data.itemId[slotId] = -1;
            data.itemAmount[slotId] = 0;
            return;
        }
        // Debug.Log("found Item in slot: " + slotId);
        data.itemId[slotId] = _itemInThisSlot.item.itemID;
        data.itemAmount[slotId] = _itemInThisSlot.count;
    }

}
