using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmeltingMachine : MonoBehaviour
{
    [Header("Info")]
    public int smelterIndex;

    [SerializeField] Item _resourceToSmelt;
    [SerializeField] bool _isSmelting;

    [SerializeField] Item _resourceType;
    [SerializeField] Item _outputType;
    [SerializeField] Item _fuelType;

    [SerializeField] int _resourceAmount;
    [SerializeField] int _outputAmount;
    [SerializeField] int _fuelAmount;

    [SerializeField] bool _resourceInitialized;
    [SerializeField] bool _fuelInitialized;
    [SerializeField] bool _fuelTimeInitialized;

    [Header("SmeltingInventory")]
    [SerializeField] InventorySlot _resourceInputSlot;
    [SerializeField] InventorySlot _itemOutputSlot;
    [SerializeField] InventorySlot _fuelInputSlot;

    public Slider fuelLeftSlider;
    public Slider progressSlider;

    [SerializeField] float _currentSmeltProgression;
    [SerializeField] float _fuelLeft;

    public int ResourceAmount { get { return _resourceAmount; } set { _resourceAmount = value; } }
    public int OutputAmount { get { return _outputAmount; } set { _outputAmount = value; } }
    public int FuelAmount { get { return _fuelAmount; } set { _fuelAmount = value; } }

    public InventorySlot ResourceInputSlot { get { return _resourceInputSlot; } set { _resourceInputSlot = value; } }
    public InventorySlot ItemOutputSlot { get { return _itemOutputSlot; } set { _itemOutputSlot = value; } }
    public InventorySlot FuelInputSlot { get { return _fuelInputSlot; } set { _fuelInputSlot = value; } }

    public Item ResourceType { get { return _resourceType; } set { _resourceType = value; } }
    public Item OutputType { get { return _outputType; } set { _outputType = value; } }
    public Item FuelType { get { return _fuelType; } set { _fuelType = value; } }

    public float FuelLeft { get { return _fuelLeft; } set { _fuelLeft = value; } }
    public float SmeltProgression { get { return _currentSmeltProgression; } set { _currentSmeltProgression = value; } }

    public bool ResourceInitialized { get { return _resourceInitialized; } set { _resourceInitialized = value; } }
    public bool FuelInitialized { get { return _fuelInitialized; } set { _fuelInitialized = value; } }

    private void Start()
    {
        SmeltingPanelManager.Instance.SetSmelterInfo(this);
    }

    public bool HandleFuel()
    {
        // Check If Fuel Needs To Be Used
        if (_fuelLeft <= 0 && !_fuelTimeInitialized && _outputAmount < _outputType.maxStack)
        {
            if (_fuelAmount > 0)
            {
                _fuelLeft = FuelType.fuelTime;
                _fuelTimeInitialized = true;
                _fuelAmount--;
                if (SmeltingPanelManager.Instance.panelActive && SmeltingPanelManager.Instance.currentSmelter == this)
                {
                    _fuelInputSlot.GetInventoryItem().count--;
                    _fuelInputSlot.GetInventoryItem().RefreshCount();
                }
                return true;
            }
        }
        else
        {
            if (_fuelLeft > 0)
            {
                _fuelLeft -= Time.deltaTime;
                return true;
            }
            else if (_fuelLeft <= 0)
            {
                _fuelTimeInitialized = false;
                return false;
            }
        }
        return false;
    }

    private void HandleSmelt()
    {
        if (HasSmeltableResource() && HandleFuel() && _outputAmount < _outputType.maxStack)
        {
            if (_currentSmeltProgression < _resourceType.smeltTime)
            {
                _currentSmeltProgression += Time.deltaTime;
            }
            if (_currentSmeltProgression >= _resourceType.smeltTime)
            {
                //Spawn Output Item
                AddMachineItem(_itemOutputSlot, _outputType);

                if (_resourceInputSlot.GetInventoryItem())
                {
                    _resourceAmount--;
                    if (_resourceInputSlot.GetInventoryItem().count == 1)
                    {
                        Destroy(_resourceInputSlot.GetInventoryItem().gameObject);
                        ResourceType = null;
                        OutputType = null;
                    }
                    else
                    {
                        _resourceInputSlot.GetInventoryItem().count--;
                        _resourceInputSlot.GetInventoryItem().RefreshCount();
                    }
                }
                else
                {
                    _resourceAmount--;
                }

                _currentSmeltProgression = 0;
            }
        }
        else if (_currentSmeltProgression > 0)
        {
            _currentSmeltProgression -= Time.deltaTime;
        }
    }


    private void Update()
    {
        #region Slider Values
        if (progressSlider != null && SmeltingPanelManager.Instance.currentSmelter == this)
        {
            if (_outputType != null)
                progressSlider.maxValue = _outputType.smeltTime;
            progressSlider.value = _currentSmeltProgression;
        }
        else if (progressSlider == null && SmeltingPanelManager.Instance.currentSmelter == this)
        {
            Debug.LogError("Progress Slider Not Set");
        }

        if (fuelLeftSlider != null && SmeltingPanelManager.Instance.currentSmelter == this)
        {
            if (_fuelType != null)
            {
                fuelLeftSlider.maxValue = _fuelType.fuelTime;
            }
            fuelLeftSlider.value = _fuelLeft;
        }
        else if (progressSlider == null && SmeltingPanelManager.Instance.currentSmelter == this)
        {
            Debug.LogError("FuelLeft Slider Not Set");
        }
        #endregion

        if (_fuelType != null && _resourceType != null)
        {
            HandleSmelt();
        }
        else
        {
            //Check If Fuel Needs To Be Initialized
            if (_fuelType == null && _fuelInputSlot.GetInventoryItem() && SmeltingPanelManager.Instance.currentSmelter == this)
            {
                InitializeFuelType();
            }
            else if (_fuelType != null && !_fuelInputSlot.GetInventoryItem())
            {
                //Reset FuelType When FuelSlot Is Empty
                if (SmeltingPanelManager.Instance.currentSmelter == this)
                {
                    _fuelType = null;
                    _fuelInitialized = false;
                }
            }
            else if (_fuelType != null && _fuelAmount <= 0)
            {
                _fuelType = null;
                _fuelInitialized = false;
            }

            // Check And Reset Resource Type
            if (_resourceType == null && _resourceInputSlot.GetInventoryItem())
            {
                if (SmeltingPanelManager.Instance.currentSmelter == this)
                {
                    _resourceType = _resourceInputSlot.GetInventoryItem().item;
                    _resourceAmount = _resourceInputSlot.GetInventoryItem().count;
                    _resourceInitialized = true;
                    _resourceToSmelt = _resourceType;
                }
            }
            else if (_resourceType != null && !_resourceInputSlot.GetInventoryItem())
            {
                if (SmeltingPanelManager.Instance.currentSmelter == this)
                {
                    _resourceType = null;
                    _outputType = null;
                    _resourceInitialized = false;
                }
            }
            else if (_resourceType != null && _resourceAmount <= 0)
            {
                _resourceType = null;
                _resourceInitialized = false;
            }

            // Check And Reset Output Type
            if (_outputType == null && _resourceType != null)
            {
                _outputType = _resourceType.itemToGetAfterSmelt;
            }
            else if (_outputType != null && _resourceType == null)
            {
                _outputType = null;
            }
        }
    }



    public void AddMachineItem(InventorySlot slot, Item item = null, int amount = 0, bool calledFromPanel = false)
    {
        //Spawn All Current Items When Panel Got Opened
        if (calledFromPanel)
        {
            SpawnItem(slot, item, amount);
        }
        else
        {
            //Add Singular Item To Output
            if (amount == 0)
            {
                //Add Item To Output When Panel Closed
                if (!SmeltingPanelManager.Instance.panelActive || SmeltingPanelManager.Instance.panelActive && SmeltingPanelManager.Instance.currentSmelter != this)
                {
                    _outputAmount++;
                }
                //Add Item To Output When Panel Opened
                else
                {
                    if (_outputAmount == 0)
                    {
                        //Spawn Item To Output
                        SpawnItem(ItemOutputSlot, _resourceToSmelt.itemToGetAfterSmelt, _resourceToSmelt.amountToGetAfterSmelt);
                    }
                    else
                    {
                        //Add Item To Output
                        _itemOutputSlot.GetInventoryItem().count++;
                        _itemOutputSlot.GetInventoryItem().RefreshCount();

                        _outputAmount++;
                    }
                }
            }
            else
            {
                Debug.LogError("This Shouldn't be called");
            }
        }
    }

    public void SpawnItem(InventorySlot slot, Item item, int amount)
    {
        // Spawn New UI Item (None Were Present)
        GameObject newItemGO = Instantiate(InventoryManager.Instance.InventoryItemPrefab, slot.transform);
        slot.SetInventoryItem(newItemGO.GetComponent<InventoryItem>());
        slot.GetInventoryItem().lastInventorySlot = _resourceInputSlot;
        slot.GetInventoryItem().count = amount;
        slot.GetInventoryItem().InitializeItem(item, amount);

        if (item.isFuel)
        {
            InitializeFuelType();
        }
        else if (item.isSmeltable)
        {
            _resourceToSmelt = item;
            _resourceInitialized = true;
            _resourceAmount = slot.GetInventoryItem().count;
        }
        else
        {
            Debug.Log($"Spawned {amount} {item} In Output Slot");
            _outputAmount = amount;
        }
    }

    public void InitializeFuelType()
    {
        // Set Smelters Fuel Info When UI Item Dragged In
        if (_fuelInputSlot.GetInventoryItem() == null)
        {
            return;
        }
        _fuelInputSlot.SetInventoryItem(_fuelInputSlot.transform.GetChild(0).GetComponent<InventoryItem>());
        _fuelType = _fuelInputSlot.GetInventoryItem().item;
        _fuelAmount = _fuelInputSlot.GetInventoryItem().count;
        Debug.Log($"Initialized fuel type: {_fuelAmount} {_fuelType}");
        _fuelInitialized = true;
    }

    bool HasSmeltableResource()
    {
        if (_resourceType != null && _resourceType.isSmeltable)
        {
            return true;
        }
        return false;
    }
}
