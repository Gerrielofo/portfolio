using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiggingMachine : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] Resource _collectedResource;
    [SerializeField] bool _isDigging;
    [SerializeField] Item _itemType;
    [SerializeField] int _itemAmount;
    [SerializeField] Item _fuelType;
    [SerializeField] int _fuelAmount;

    [SerializeField] bool _resourceInitialized;
    [SerializeField] bool _fuelInitialized;

    [Header("Miner Settings")]
    [SerializeField] LayerMask _resourceLayer;

    [Header("MinerInventory")]
    [SerializeField] InventorySlot _itemSlot;
    [SerializeField] InventorySlot _fuelSlot;
    public Slider fuelLeftSlider;

    float _currentMineProgression;

    public int ItemAmount { get { return _itemAmount; } set { _itemAmount = value; } }
    public int FuelAmount { get { return _fuelAmount; } set { _fuelAmount = value; } }
    public InventorySlot ItemSlot { get { return _itemSlot; } set { _itemSlot = value; } }
    public InventorySlot FuelSlot { get { return _fuelSlot; } set { _fuelSlot = value; } }
    public Item ItemType { get { return _itemType; } set { _itemType = value; } }
    public Item FuelType { get { return _fuelType; } set { _fuelType = value; } }
    public bool ResourceInitialized { get { return _resourceInitialized; } set { _resourceInitialized = value; } }
    public bool FuelInitialized { get { return _fuelInitialized; } set { _fuelInitialized = value; } }

    [SerializeField] bool _fuelTimeInitialized;
    [SerializeField] float _fuelLeft;

    private void Start()
    {
        // Check What Ore The Digger Was Placed On
        if (_collectedResource == null)
        {
            _collectedResource = GetComponentInParent<ResourceVein>().Resource;
            _itemType = _collectedResource.item;
            _currentMineProgression = _collectedResource.mineDuration;
        }
        MiningPanelManager.Instance.SetDiggerInfo(this);
    }

    public bool HandleFuel()
    {
        if (!_fuelTimeInitialized && _fuelLeft <= 0)
        {
            // Check If Digger's UI Is Opened
            if (MiningPanelManager.Instance.panelActive && MiningPanelManager.Instance.currentDigger == this)
            {
                if (FuelAmount == 0 && _fuelLeft <= 0)
                {
                    _fuelTimeInitialized = false;
                    return false;
                }
                else if (FuelAmount > 0)
                {
                    _fuelSlot.UseItem();
                    _fuelAmount--;
                    _fuelLeft = _fuelType.fuelTime;
                    _fuelTimeInitialized = true;
                    Debug.Log("Used Some Fuel, Fuel Items Left: " + FuelAmount);
                    return true;
                }
            }
            else
            {
                if (FuelAmount == 0 && _fuelLeft <= 0)
                {
                    _fuelTimeInitialized = false;
                    return false;
                }
                _fuelAmount--;
                _fuelLeft = _fuelType.fuelTime;
                _fuelTimeInitialized = true;
                return true;
            }
            if (_fuelType != null && _fuelAmount > 0 && _fuelLeft <= 0)
            {
                _fuelLeft = _fuelType.fuelTime;
                _fuelTimeInitialized = true;
                return true;
            }
            else if (_fuelAmount <= 0 && _fuelLeft <= 0)
            {
                _fuelTimeInitialized = false;
                return false;
            }
            _fuelTimeInitialized = true;
            return true;
        }
        else if (_fuelType == null)
        {
            if (_fuelSlot.GetInventoryItem() != null && MiningPanelManager.Instance.currentDigger == this)
            {
                InitializeFuelType();
                return false;
            }
        }
        else if (_fuelLeft > 0)
        {
            _fuelLeft -= Time.deltaTime;\
            if (MiningPanelManager.Instance.currentDigger == this)
                fuelLeftSlider.value = _fuelLeft;
            return true;

        }
        else if (_fuelLeft <= 0)
        {
            _fuelTimeInitialized = false;
            return false;
        }
        else if (_fuelType == null && _fuelLeft <= 0)
        {
            _fuelTimeInitialized = false;
            return false;
        }
        return false;
    }


    void Update()
    {
        #region Debugging
        if (fuelLeftSlider != null && MiningPanelManager.Instance.currentDigger == this)
        {
            fuelLeftSlider.value = _fuelLeft;
        }
        else if (fuelLeftSlider == null && MiningPanelManager.Instance.currentDigger == this)
        {
            Debug.LogError("fuelLeftSlider == null!");
        }
        if (_fuelAmount == 0 && _fuelLeft <= 0)
        {
            _fuelType = null;
            FuelInitialized = false;
        }
        if (_collectedResource == null)
        {
            Debug.LogError("Miner Has No Resource To Gather!");
        }
        #endregion
        else
        {
            if (_itemType != null)
            {
                if (_itemAmount >= _itemType.maxStack)
                {
                    Debug.Log("Item Has Reached Max Stack! Remove It To Continue");
                    return;
                }
            }
            if (MiningPanelManager.Instance.panelActive && _itemSlot.GetInventoryItem() != null && MiningPanelManager.Instance.currentDigger == this)
            {
                _itemAmount = _itemSlot.GetInventoryItem().count;
            }
            bool result = HandleFuel();
            if (_currentMineProgression >= 0)
            {
                _isDigging = result;
                if (_fuelAmount > 0)
                {
                    if (fuelLeftSlider != null && MiningPanelManager.Instance.currentDigger == this)
                        fuelLeftSlider.maxValue = _fuelType.fuelTime;
                }

                if (result)
                {
                    _currentMineProgression -= Time.deltaTime;
                    if (fuelLeftSlider != null && MiningPanelManager.Instance.currentDigger == this)
                        fuelLeftSlider.value = _fuelLeft;
                }
                else
                {
                    if (_currentMineProgression < 0.05)
                    {
                        _currentMineProgression = 0;
                    }
                }
            }
            else if (_currentMineProgression <= 0 && _fuelLeft > 0 && _fuelTimeInitialized)
            {
                AddMachineItem(false, _itemType);
                Debug.LogWarning($"Added Item {_itemType}");
            }
            else if (_currentMineProgression < 0 && _fuelLeft <= 0)
            {
                Debug.LogError("Machine Has No Fuel AND Is Not Trying To Mine");
            }
        }
    }

    public void AddMachineItem(bool isFuel, Item item = null, int amount = 0, bool calledFromPanel = false)
    {
        if (!isFuel)
        {
            if (!MiningPanelManager.Instance.panelActive || MiningPanelManager.Instance.panelActive && MiningPanelManager.Instance.currentDigger != this)
            {
                if (amount == 0)
                {
                    _itemAmount += _collectedResource.recourceAmount;
                    _currentMineProgression = _collectedResource.mineDuration;
                }
                else
                {
                    _itemAmount += amount;
                    _currentMineProgression = _collectedResource.mineDuration;
                }
                Debug.Log($"Added {amount} Item(s) When Panel Not Shown");
                return;
            }
            else if (MiningPanelManager.Instance.panelActive && MiningPanelManager.Instance.currentDigger == this)
            {
                if (amount > 0 && _itemSlot.GetInventoryItem() == null)
                {
                    SpawnMachineItem(item, amount);
                    Debug.Log($"Spawned {amount} {item}");
                }
                else if (_itemSlot.GetInventoryItem() != null)
                {
                    if (amount == 0)
                    {
                        _itemAmount += _collectedResource.recourceAmount;
                        _itemSlot.GetInventoryItem().count += _collectedResource.recourceAmount;
                        _itemSlot.GetInventoryItem().RefreshCount();
                    }
                    else
                    {
                        _itemAmount += amount;
                        _itemSlot.GetInventoryItem().count += amount;
                        _itemSlot.GetInventoryItem().RefreshCount();
                    }
                }
                else
                {
                    if (amount == 0)
                    {
                        SpawnMachineItem(_collectedResource.item, _collectedResource.recourceAmount);
                        // _itemAmount += _collectedResource.recourceAmount;
                        Debug.Log("Spawned First Of " + _collectedResource.item);
                        // Debug.Log("Had No resources to spawn");
                    }
                    else
                    {
                        SpawnMachineItem(item, amount);
                        Debug.Log($"Spawned {amount} {item.name}");
                    }
                }
            }
            if (!calledFromPanel)
            {
                _itemAmount += amount;
                _currentMineProgression = _collectedResource.mineDuration;
            }
        }
        else
        {
            if (!MiningPanelManager.Instance.panelActive || MiningPanelManager.Instance.panelActive && MiningPanelManager.Instance.currentDigger != this)
            {
                if (amount == 0)
                {
                    Debug.Log("Did absolutly nothing");
                }
                else
                {
                    _fuelAmount += amount;
                }
                Debug.Log($"Added {amount} {_fuelType} When Panel Not Shown");
                return;
            }
            if (amount > 0)
            {
                SpawnMachineFuel(item, amount);
            }
            else if (_fuelSlot.GetInventoryItem() != null && amount > 0 && _fuelSlot.GetInventoryItem() == item)
            {
                _fuelSlot.GetInventoryItem().count += amount;
                _fuelSlot.GetInventoryItem().RefreshCount();
            }
            else
            {
                SpawnMachineFuel(item, _fuelAmount);
            }
            if (!calledFromPanel)
            {
                _currentMineProgression = _collectedResource.mineDuration;
            }
        }

    }

    public void SpawnMachineItem(Item item, int amount)
    {
        GameObject newItemGO = Instantiate(InventoryManager.Instance.InventoryItemPrefab, _itemSlot.transform);
        _itemSlot.SetInventoryItem(newItemGO.GetComponent<InventoryItem>());
        _itemSlot.GetInventoryItem().lastInventorySlot = _itemSlot;
        _itemSlot.GetInventoryItem().count = amount;
        _itemSlot.GetInventoryItem().InitializeItem(item, amount);

        Debug.Log("Resource was Initialized: " + newItemGO.name);

        if (_resourceInitialized)
        {
            Debug.Log("Resource was already Initialized");
            return;
        }
        _itemType = item;
        _resourceInitialized = true;
        _itemAmount = _itemSlot.GetInventoryItem().count;
    }

    void SpawnMachineFuel(Item item, int amount)
    {
        GameObject newItemGO = Instantiate(InventoryManager.Instance.InventoryItemPrefab, _fuelSlot.transform);
        _fuelSlot.SetInventoryItem(newItemGO.GetComponent<InventoryItem>());
        _fuelSlot.GetInventoryItem().count = amount;
        _fuelSlot.GetInventoryItem().InitializeItem(item, amount);
        if (_fuelInitialized)
        {
            Debug.Log("Fuel was already Initialized");
            return;
        }
        InitializeFuelType();
    }

    public void InitializeFuelType()
    {
        if (_fuelSlot.GetInventoryItem() == null)
        {
            return;
        }
        _fuelSlot.SetInventoryItem(_fuelSlot.transform.GetChild(0).GetComponent<InventoryItem>());
        _fuelType = _fuelSlot.GetInventoryItem().item;
        _fuelAmount = _fuelSlot.GetInventoryItem().count;
        Debug.Log("Initialized fuel type: " + _fuelAmount + " " + _fuelType);
        _fuelInitialized = true;
    }
}
