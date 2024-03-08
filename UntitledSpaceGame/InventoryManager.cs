using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventorySlot> inventorySlots = new();

    [SerializeField] InventorySlot[] _toolbarSlots;

    [SerializeField] GameObject _inventoryItemPrefab;
    public GameObject InventoryItemPrefab { get { return _inventoryItemPrefab; } }

    int _selectedSlot = -1;

    public List<ItemInfo> itemsInInventory = new();

    public Item[] allItems;

    public InventoryItem heldItem;
    [SerializeField] InGameUIManager _uiManager;

    [SerializeField] Item _itemToSpawn;

    [Header("Canvas Information")]
    public EventSystem eventSystem;
    public List<GraphicRaycaster> graphicRaycasters;
    public RectTransform rectTransform;

    public bool canCheckInventoryQuest;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else if (Instance == null)
        {
            Instance = this;
        }
        UpdateItemsInfoList();
    }

    private void Update()
    {
        if (Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber && number > 0 && number <= _toolbarSlots.Length)
            {
                ChangeSelectedSlot(number - 1);
            }
        }
    }

    void ChangeSelectedSlot(int newSlot)
    {
        if (_selectedSlot == newSlot)
        {
            _toolbarSlots[_selectedSlot].Deselect();
            _selectedSlot = -1;
            return;
        }

        if (_selectedSlot >= 0)
        {
            _toolbarSlots[_selectedSlot].Deselect();
        }

        _toolbarSlots[newSlot].Select();
        _selectedSlot = newSlot;
    }

    public Item GetSelectedItem()
    {
        if (_selectedSlot > -1)
        {
            if (_toolbarSlots[_selectedSlot].GetInventoryItem() != null)
                return _toolbarSlots[_selectedSlot].GetInventoryItem().item;
            else return null;
        }
        return null;
    }

    public bool AddItem(int itemId, int amount)
    {
        if (itemId <= 0)
        {
            _itemToSpawn = null;
            Debug.Log($"{itemId} is not an existing itemId");
            return false;
        }
        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i].itemID == itemId)
            {
                _itemToSpawn = allItems[i];
                Debug.Log($"Adding Item {allItems[i]}");
            }
        }
        if (!HasSpace(itemId, amount))
        {
            Debug.Log("Inventory Was Full, Could Not Add " + _itemToSpawn);
            return false;
        }
        //Check for stackable slot
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();


            if (itemInSlot != null && itemInSlot.item == _itemToSpawn && itemInSlot.count < itemInSlot.item.maxStack)
            {
                if (itemInSlot.count + amount > itemInSlot.item.maxStack)
                {
                    int extra = itemInSlot.count + amount - itemInSlot.item.maxStack;
                    Debug.Log("Exta: " + extra);
                    itemInSlot.count = itemInSlot.item.maxStack;
                    AddItem(itemId, extra);
                    UpdateItemsInfoList();

                    itemInSlot.RefreshCount();

                    return true;
                }
                itemInSlot.count += amount;
                itemInSlot.RefreshCount();
                UpdateItemsInfoList();

                return true;
            }

        }
        //Check for empty Slot
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isHudSlot && !_itemToSpawn.canBeInHudSlot && !_itemToSpawn.isPlacable)
            {
                Debug.Log("Can't Spawn This Item Here As it Is BlackListed");
                DropItem(itemId, amount);
                UpdateItemsInfoList();
                return false;
            }

            int slot = i;
            InventoryItem itemInSlot = inventorySlots[slot].GetComponentInChildren<InventoryItem>();

            if (itemInSlot == null)
            {
                if (amount > _itemToSpawn.maxStack)
                {
                    int extra = amount - _itemToSpawn.maxStack;
                    Debug.Log("Extra: " + extra);

                    SpawnNewItem(itemId, _itemToSpawn.maxStack, slot);
                    AddItem(itemId, extra);
                    UpdateItemsInfoList();

                    return true;
                }
                SpawnNewItem(itemId, amount, slot);
                return true;
            }
        }

        UpdateItemsInfoList();

        DropItem(itemId, amount);
        return false;
    }

    public void SpawnNewItem(int itemID, int itemCount, int slotID)
    {
        GameObject newItemGO = Instantiate(_inventoryItemPrefab, inventorySlots[slotID].transform);
        InventoryItem inventoryItem = newItemGO.GetComponent<InventoryItem>();
        inventoryItem.count = itemCount;
        for (int i = 0; i < allItems.Length; i++)
        {
            if (allItems[i].itemID == itemID)
            {
                Debug.Log($"Id was found as {allItems[i].itemID}. Initializing Item {allItems[i]}");
                inventoryItem.InitializeItem(allItems[i], itemCount);
                break;
            }
        }

        UpdateItemsInfoList();
    }


    public void UseItem(int itemID, int itemCount)
    {
        int itemsLeft = itemCount;
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (itemsLeft == 0)
            {
                break;
            }
            if (itemsLeft <= 0)
            {
                Debug.LogError("Removed Too Many Items!");
                return;
            }
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            for (int z = 0; z < allItems.Length; z++)
            {
                if (allItems[z].itemID == itemID)
                {
                    if (itemInSlot != null && itemInSlot.item == allItems[z])
                    {
                        if (itemInSlot.count >= itemCount)
                        {
                            itemInSlot.count -= itemCount;
                            itemsLeft -= itemCount;
                            Debug.Log($"Used {itemCount} {allItems[z].name}");
                            itemInSlot.RefreshCount();
                            UpdateItemsInfoList();
                            break;
                        }
                        else
                        {
                            itemsLeft -= itemInSlot.count;
                            itemInSlot.count = 0;
                        }
                        Debug.Log($"Used {itemCount} {allItems[z].name}");
                        itemInSlot.RefreshCount();
                        UpdateItemsInfoList();
                    }
                    break;
                }
            }

        }
    }

    public void DropItem(int itemId, int amount, Transform spawnposition = null)
    {
        Item itemToDrop;
        for (int i = 0; i < allItems.Length; i++)
        {
            if (itemId == allItems[i].itemID)
            {
                itemToDrop = allItems[i];
                GameObject droppedItem;
                if (spawnposition != null)
                {
                    droppedItem = Instantiate(itemToDrop.itemPrefab, spawnposition.position, Quaternion.identity);
                }
                else
                {
                    droppedItem = Instantiate(itemToDrop.itemPrefab, player.transform.position, Quaternion.identity);
                }
                droppedItem.transform.GetComponent<DroppedItem>().item = itemToDrop;
                droppedItem.transform.GetComponent<DroppedItem>().amount = amount;
                Debug.Log($"Succesfully Dropped {amount} {itemToDrop}.");
                break;
            }
        }

    }

    public void UpdateItemsInfoList()
    {
        itemsInInventory.Clear();

        for (int i = 0; i < allItems.Length; i++)
        {
            int amount = GetTotalItemAmount(inventorySlots, allItems[i]);
            itemsInInventory.Add(new(allItems[i], amount));
        }

        if (canCheckInventoryQuest)
        {
            QuestManager.Instance.CheckInventory();
        }
    }

    static int GetTotalItemAmount(List<InventorySlot> list, Item item)
    {
        int result = 0;
        foreach (var itm in list)
        {
            if (itm.transform.childCount > 0)
            {
                InventoryItem thisItem = itm.GetComponentInChildren<InventoryItem>();

                if (thisItem != null)
                {
                    if (thisItem.item != item)
                    {
                        continue;
                    }

                    result += thisItem.count;
                }
            }
        }

        return result;
    }

    public bool HasSpace(int itemId, int amount)
    {
        //Check For Stackable Slot
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot != null && itemInSlot.item == _itemToSpawn && itemInSlot.count + amount < itemInSlot.item.maxStack)
            {
                return true;
            }
        }
        //Check for empty Slot
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isHudSlot && !_itemToSpawn.canBeInHudSlot)
            {
                return false;
            }

            int slot = i;
            InventoryItem itemInSlot = inventorySlots[slot].GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                return true;
            }
        }
        return false;
    }
}
