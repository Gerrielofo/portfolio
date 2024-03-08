using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [SerializeField] Recipe[] _allRecipes;
    [SerializeField] Recipe _selectedRecipeToCraft;

    [SerializeField] Transform _recipeListTransform;
    [SerializeField] GameObject _itemPrefab;

    [Header("Recipe Selection")]
    [SerializeField] GameObject _imagePrefab;
    [SerializeField] Transform _imageListTransform;
    [SerializeField] CraftButton _selectedButtonObject;
    [SerializeField] List<Recipe> _currentRecipes = new();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        for (int i = 0; i < _recipeListTransform.childCount; i++)
        {
            _currentRecipes.Add(_recipeListTransform.GetChild(i).GetComponent<CraftButton>().recipe);
        }
    }

    public void SelectCraftingRecipe(Recipe recipe, CraftButton button)
    {
        for (int i = _imageListTransform.childCount; i > 0; i--)
        {
            Destroy(_imageListTransform.GetChild(i - 1).gameObject);
        }
        if (recipe == null)
        {
            Debug.Log("No Recipe Selected");
            return;
        }

        _selectedRecipeToCraft = recipe;
        _selectedButtonObject = button;

        Debug.Log($"Selected Recipe {recipe}");

        for (int i = 0; i < recipe.itemsNeeded.Length; i++)
        {
            GameObject spawnedImage = Instantiate(_imagePrefab, _imageListTransform);
            spawnedImage.GetComponent<Image>().sprite = recipe.itemsNeeded[i].item.image;
            spawnedImage.transform.GetChild(0).GetComponent<TMP_Text>().text = recipe.itemsNeeded[i].amount.ToString();
        }
    }

    public void CraftItem()
    {
        if (_selectedRecipeToCraft != null)
        {
            for (int i = 0; i < _selectedRecipeToCraft.itemsNeeded.Length; i++)
            {
                for (int y = 0; y < InventoryManager.Instance.itemsInInventory.Count; y++)
                {
                    if (InventoryManager.Instance.itemsInInventory[y].item == _selectedRecipeToCraft.itemsNeeded[i].item)
                    {
                        if (InventoryManager.Instance.itemsInInventory[y].amount < _selectedRecipeToCraft.itemsNeeded[i].amount)
                        {
                            Debug.Log($"You don't have enough {_selectedRecipeToCraft.itemsNeeded[i].item.name} to craft this item!");
                            return;
                        }
                    }
                }
            }
            for (int i = 0; i < _selectedRecipeToCraft.itemsNeeded.Length; i++)
            {
                InventoryManager.Instance.UseItem(_selectedRecipeToCraft.itemsNeeded[i].item.itemID, _selectedRecipeToCraft.itemsNeeded[i].amount);
            }
            InventoryManager.Instance.AddItem(_selectedRecipeToCraft.itemToCraft.itemID, _selectedRecipeToCraft.amountToCraft);
        }
        else
        {
            Debug.Log("No Recipe Selected");
        }
    }

    public void RecipeButton(int itemID)
    {
        for (int r = 0; r < _allRecipes.Length; r++)
        {
            if (_allRecipes[r].itemToCraft.itemID == itemID)
            {
                if (_currentRecipes.Contains(_allRecipes[r]))
                {
                    Debug.LogError($"There is already a recipe for {_allRecipes[r]}");
                    return;
                }
                else
                {
                    AddRecipe(_allRecipes[r]);
                    return;
                }
            }
        }

        Debug.LogError("Item Index Does Not Exist As Item");
    }

    public void AddRecipe(Recipe recipe)
    {
        GameObject spawnedRecipe = Instantiate(_itemPrefab, _recipeListTransform);
        spawnedRecipe.GetComponent<CraftButton>().recipe = recipe;
        spawnedRecipe.GetComponent<CraftButton>().UpdateRecipeUI();
        _currentRecipes.Add(recipe);
    }
}
