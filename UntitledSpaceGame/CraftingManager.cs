using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [SerializeField] Recipe[] allRecipes;
    public Recipe selectedRecipeToCraft;

    public Transform recipeListTransform;
    public GameObject itemPrefab;

    [Header("Recipe Selection")]
    [SerializeField] GameObject imagePrefab;
    [SerializeField] Transform imageListTransform;
    public CraftButton selectedButtonObject;
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
        for (int i = 0; i < recipeListTransform.childCount; i++)
        {
            _currentRecipes.Add(recipeListTransform.GetChild(i).GetComponent<CraftButton>().recipe);
        }
    }

    public void SelectCraftingRecipe(Recipe recipe, CraftButton button)
    {
        for (int i = imageListTransform.childCount; i > 0; i--)
        {
            Destroy(imageListTransform.GetChild(i - 1).gameObject);
        }
        if (recipe == null)
        {
            Debug.Log("No Recipe Selected");
            return;
        }

        selectedRecipeToCraft = recipe;
        selectedButtonObject = button;

        Debug.Log($"Selected Recipe {recipe}");

        for (int i = 0; i < recipe.itemsNeeded.Length; i++)
        {
            GameObject spawnedImage = Instantiate(imagePrefab, imageListTransform);
            spawnedImage.GetComponent<Image>().sprite = recipe.itemsNeeded[i].item.image;
            spawnedImage.transform.GetChild(0).GetComponent<TMP_Text>().text = recipe.itemsNeeded[i].amount.ToString();
        }
    }

    public void CraftItem()
    {
        if (selectedRecipeToCraft != null)
        {
            for (int i = 0; i < selectedRecipeToCraft.itemsNeeded.Length; i++)
            {
                for (int y = 0; y < InventoryManager.Instance.itemsInInventory.Count; y++)
                {
                    if (InventoryManager.Instance.itemsInInventory[y].item == selectedRecipeToCraft.itemsNeeded[i].item)
                    {
                        if (InventoryManager.Instance.itemsInInventory[y].amount < selectedRecipeToCraft.itemsNeeded[i].amount)
                        {
                            Debug.Log($"You don't have enough {selectedRecipeToCraft.itemsNeeded[i].item.name} to craft this item!");
                            return;
                        }
                    }
                }
            }
            for (int i = 0; i < selectedRecipeToCraft.itemsNeeded.Length; i++)
            {
                InventoryManager.Instance.UseItem(selectedRecipeToCraft.itemsNeeded[i].item.itemID, selectedRecipeToCraft.itemsNeeded[i].amount);
            }
            InventoryManager.Instance.AddItem(selectedRecipeToCraft.itemToCraft.itemID, selectedRecipeToCraft.amountToCraft);
        }
        else
        {
            Debug.Log("No Recipe Selected");
        }
    }

    public void RecipeButton(int itemID)
    {
        for (int r = 0; r < allRecipes.Length; r++)
        {
            if (allRecipes[r].itemToCraft.itemID == itemID)
            {
                if (_currentRecipes.Contains(allRecipes[r]))
                {
                    Debug.LogError($"There is already a recipe for {allRecipes[r]}");
                    return;
                }
                else
                {
                    AddRecipe(allRecipes[r]);
                    return;
                }
            }
        }

        Debug.LogError("Item Index Does Not Exist As Item");
    }

    public void AddRecipe(Recipe recipe)
    {
        GameObject spawnedRecipe = Instantiate(itemPrefab, recipeListTransform);
        spawnedRecipe.GetComponent<CraftButton>().recipe = recipe;
        spawnedRecipe.GetComponent<CraftButton>().UpdateRecipeUI();
        _currentRecipes.Add(recipe);
    }
}
