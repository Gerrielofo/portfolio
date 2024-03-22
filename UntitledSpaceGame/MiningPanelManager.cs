using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MiningPanelManager : MonoBehaviour
{
    public static MiningPanelManager Instance;

    PlayerInput _playerInput;

    [SerializeField] GameObject _miningPanel;
    [SerializeField] InventorySlot _itemSlot;
    [SerializeField] InventorySlot _fuelSlot;
    [SerializeField] Slider _fuelLeftSlider;

    public bool panelActive;

    public DiggingMachine currentDigger;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        if (_playerInput == null)
        {
            _playerInput = FindObjectOfType<PlayerInput>();
        }
    }

    public void SetDiggerInfo(DiggingMachine diggingMachine)
    {
        diggingMachine.ItemSlot = _itemSlot;
        diggingMachine.FuelSlot = _fuelSlot;
        diggingMachine.fuelLeftSlider = _fuelLeftSlider;
    }

    public void ToggleMiningPanel(DiggingMachine diggingMachine)
    {
        InGameUIManager.Instance.animator.SetTrigger("SwitchInventoryType");
        if (!InGameUIManager.Instance.inventoryShown && !panelActive)
        {
            InGameUIManager.Instance.ToggleInventory();
            InventoryKeybids.Instance.InventorySubscribe();
        }
        else if (InGameUIManager.Instance.inventoryShown && panelActive)
        {
            InGameUIManager.Instance.ToggleInventory();
            InventoryKeybids.Instance.InventorySubscribe();
        }

        if (currentDigger == diggingMachine || diggingMachine == null)
        {
            currentDigger = null;
            if (_itemSlot.GetInventoryItem() != null)
                Destroy(_itemSlot.GetInventoryItem().gameObject);
            if (_fuelSlot.GetInventoryItem() != null)
                Destroy(_fuelSlot.GetInventoryItem().gameObject);
            _miningPanel.GetComponent<Canvas>().enabled = !_miningPanel.GetComponent<Canvas>().enabled;
            _miningPanel.GetComponent<GraphicRaycaster>().enabled = !_miningPanel.GetComponent<GraphicRaycaster>().enabled;
            panelActive = _miningPanel.GetComponent<GraphicRaycaster>().enabled;
            _playerInput.currentActionMap = _playerInput.actions.FindActionMap("Game");
            Debug.Log("Changed Actionmap To " + _playerInput.currentActionMap.name);
            return;
        }
        else
        {
            currentDigger = diggingMachine;
            if (_itemSlot.GetInventoryItem() != null)
            {
                Debug.Log("destroy");

                Destroy(_itemSlot.GetInventoryItem().gameObject);
            }
            if (_fuelSlot.GetInventoryItem() != null)
            {
                Debug.Log("destroy");

                Destroy(_fuelSlot.GetInventoryItem().gameObject);
            }

            _miningPanel.GetComponent<Canvas>().enabled = !_miningPanel.GetComponent<Canvas>().enabled;
            _miningPanel.GetComponent<GraphicRaycaster>().enabled = !_miningPanel.GetComponent<GraphicRaycaster>().enabled;
            panelActive = _miningPanel.GetComponent<GraphicRaycaster>().enabled;

            if (diggingMachine.ItemType != null && diggingMachine.ItemAmount > 0)
            {
                diggingMachine.AddMachineItem(false, diggingMachine.ItemType, diggingMachine.ItemAmount, true);
                Debug.Log("look ma i spawned something");
            }
            if (diggingMachine.FuelType != null && diggingMachine.FuelAmount > 0)
            {
                diggingMachine.AddMachineItem(true, diggingMachine.FuelType, diggingMachine.FuelAmount, true);
            }


        }

        if (panelActive)
        {
            _playerInput.currentActionMap = _playerInput.actions.FindActionMap("Menu");
            Debug.Log("Changed Actionmap To " + _playerInput.currentActionMap.name);
        }
    }
}
