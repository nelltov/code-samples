/*
 * This script controls the UI Canvas which facilitates the player's selection
 * of a particular sandwich ingredient. When the player presses a button corresponding
 * to an ingredient category (bread, meat, cheese, veggies, or sauce), a panel
 * will pop up with the various options of specific items in that category.
 * 
 * The player can navigate this menu with the Adaptive Controller's D-pad, and 
 * their current selected ingredient is highlighted. They can press the plate button 
 * to close the menu and add that ingredient to the sandwich, or they can press any  
 * other button to close the panel and complete a different action.
 */

using UnityEngine;
using UnityEngine.UI;
using static IngredientManager;

public class IngredientOptionUIController : Singleton<IngredientOptionUIController>
{
    private Ingredient[][] currentGridLayout;
    private int currentRow = 0;
    private int currentColumn = 0;
    private Animator animator;
    [SerializeField] public GameObject WhiteBreadTransform;

    private PlayerInputManager adaptiveControllerInputManager;
    private PlayerInputManager debugInputManager;

    [SerializeField] private GameObject ingredientRowPrefab;
    [SerializeField] private GameObject ingredientImagePrefab;

    // Ingredient Panel Parent Objects
    private Transform currentIngredientPanel;
    [SerializeField] private Transform breadPanel;
    [SerializeField] private Transform meatPanel;
    [SerializeField] private Transform cheesePanel;
    [SerializeField] private Transform veggiePanel;
    [SerializeField] private Transform saucePanel;

    #region IngredientSprites
    [Header("Ingredient Sprites")]
    [SerializeField] private Sprite whiteBreadSprite;
    [SerializeField] private Sprite wholeWheatBreadSprite;
    [SerializeField] private Sprite pattySprite;
    [SerializeField] private Sprite baconSprite;
    [SerializeField] private Sprite cheddarSprite;
    [SerializeField] private Sprite swissSprite;
    [SerializeField] private Sprite lettuceSprite;
    [SerializeField] private Sprite tomatoSprite;
    [SerializeField] private Sprite onionSprite;
    [SerializeField] private Sprite mushroomSprite;
    [SerializeField] private Sprite ketchupSprite;
    [SerializeField] private Sprite mustardSprite;
    [SerializeField] private Sprite mayoSprite;
    #endregion

    #region Ingredient Layouts
    private Ingredient[][] breadLayout = new Ingredient[2][]
    {
        new Ingredient[1] { Ingredient.WhiteBread },
        new Ingredient[1] { Ingredient.WholeWheatBread }
    };

    private Ingredient[][] meatLayout = new Ingredient[1][]
    {
        new Ingredient[2] { Ingredient.Patty, Ingredient.Bacon },
    };

    private Ingredient[][] cheeseLayout = new Ingredient[1][]
    {
        new Ingredient[2] { Ingredient.Cheddar, Ingredient.Swiss },
    };

    private Ingredient[][] veggieLayout = new Ingredient[2][]
    {
        new Ingredient[2] { Ingredient.Onion, Ingredient.Mushroom },
        new Ingredient[2] { Ingredient.Tomato, Ingredient.Lettuce }
    };

    private Ingredient[][] sauceLayout = new Ingredient[1][]
    {
        new Ingredient[3] { Ingredient.Ketchup, Ingredient.Mayo, Ingredient.Mustard },
    };
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        adaptiveControllerInputManager = GameObject.Find("Input_AdaptiveController").GetComponent<PlayerInputManager>();
        debugInputManager = GameObject.Find("Input_Debug").GetComponent<PlayerInputManager>();

        // Find all the ingredient panels in children of this object
        foreach (Transform child in transform)
        {
            switch (child.name)
            {
                case "BreadPanel":
                    breadPanel = child;
                    break;
                case "MeatPanel":
                    meatPanel = child;
                    break;
                case "CheesePanel":
                    cheesePanel = child;
                    break;
                case "VeggiePanel":
                    veggiePanel = child;
                    break;
                case "SaucePanel":
                    saucePanel = child;
                    break;
                default:
                    break;
            }
        }
        animator = GetComponent<Animator>();

        ClearUI();
    }

    // Note: This will only be updating ingredients within the same selected ingredient category
    private void UpdateInputManagerSelectedObject(Ingredient ingredient)
    {
        if (adaptiveControllerInputManager) { adaptiveControllerInputManager.selectedIngredient = ingredient; }
        if (debugInputManager) { debugInputManager.selectedIngredient = ingredient; }
    }

    // Figure out the specific sprite you want for each ingredient
    private Sprite GetSpriteForIngredient(Ingredient ingredient)
    {
        return ingredient switch
        {
            Ingredient.WhiteBread => whiteBreadSprite,
            Ingredient.WholeWheatBread => wholeWheatBreadSprite,
            Ingredient.Patty => pattySprite,
            Ingredient.Bacon => baconSprite,
            Ingredient.Cheddar => cheddarSprite,
            Ingredient.Swiss => swissSprite,
            Ingredient.Lettuce => lettuceSprite,
            Ingredient.Tomato => tomatoSprite,
            Ingredient.Onion => onionSprite,
            Ingredient.Mushroom => mushroomSprite,
            Ingredient.Ketchup => ketchupSprite,
            Ingredient.Mustard => mustardSprite,
            Ingredient.Mayo => mayoSprite,
            _ => null
        };
    }

    // Get the appropriate panel for the ingredient category
    private Transform GetIngredientPanel(IngredientCategory ingredientCategory)
    {
        return ingredientCategory switch
        {
            IngredientCategory.Bread => breadPanel,
            IngredientCategory.Meat => meatPanel,
            IngredientCategory.Cheese => cheesePanel,
            IngredientCategory.Veggie => veggiePanel,
            IngredientCategory.Sauce => saucePanel,
            _ => null
        };
    }

    // Note: This function is called only when switching to a new ingredient category
    public void SetUpIngredientGridLayout(IngredientCategory ingredientCategory)
    {
        ClearUI();

        currentGridLayout = ingredientCategory switch
        {   
            IngredientCategory.Bread => breadLayout,
            IngredientCategory.Meat => meatLayout,
            IngredientCategory.Cheese => cheeseLayout,
            IngredientCategory.Veggie => veggieLayout,
            IngredientCategory.Sauce => sauceLayout,
            _ => null
        };

        // Sanity check: the current grid layout should never be null here
        if (currentGridLayout == null) { return; }

        Ingredient defaultIngredient = currentGridLayout[currentRow][currentColumn];
        UpdateInputManagerSelectedObject(defaultIngredient);

        // draw the appropriate grid UI
        currentIngredientPanel = GetIngredientPanel(ingredientCategory);
        foreach (Ingredient[] row in currentGridLayout)
        {
            GameObject rowObject = Instantiate(ingredientRowPrefab, currentIngredientPanel);
            foreach (Ingredient ingredient in row)
            {
                GameObject ingredientObject = Instantiate(ingredientImagePrefab, rowObject.transform);
                ingredientObject.transform.GetChild(0).gameObject.GetComponent<Image>().enabled = false;
                ingredientObject.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = GetSpriteForIngredient(ingredient);
            }
        }

        HighlightSelectedIngredient(currentRow, currentColumn);
        AnimationManager.Instance.SelectedIngredientAnimation(defaultIngredient);
    }

    private void HighlightSelectedIngredient(int rowIdx, int colIdx)
    {
        if (currentGridLayout == null || currentIngredientPanel == null) { return; }

        // turn off all the highlights
        foreach (Transform ingredientRow in currentIngredientPanel)
        {
            foreach (Transform ingredientImage in ingredientRow)
            {
                ingredientImage.GetChild(0).gameObject.GetComponent<Image>().enabled = false;
            }
        }
        
        // turn on the highlight for the selected ingredient
        Transform selectedIngredientImage = currentIngredientPanel.GetChild(rowIdx).GetChild(colIdx);
        selectedIngredientImage.GetChild(0).gameObject.GetComponent<Image>().enabled = true;
    }

    public void SelectDifferentIngredientInGrid(int xDir, int yDir)
    {
        if (currentGridLayout == null) 
        {
            // There is no grid UI -> input ignored
            return; 
        }

        // navigate the grid for the current category
        int newRow = currentRow + yDir;
        int newColumn = currentColumn + xDir;
        if (newRow < 0 || newRow >= currentGridLayout.Length 
         || newColumn < 0 || newColumn >= currentGridLayout[newRow].Length)
        {
            // out of bounds -> input ignored
            return;
        }
        else
        {
            Ingredient newIngredient = currentGridLayout[newRow][newColumn];
            UpdateInputManagerSelectedObject(newIngredient);

            currentColumn = newColumn;
            currentRow = newRow;

            HighlightSelectedIngredient(currentRow, currentColumn);
            AudioManager.Instance.PlayIngredientUISwitchSound();
            AnimationManager.Instance.SelectedIngredientAnimation(newIngredient);
        }
    }

    private void DeleteChildrenOfTransform(Transform transform)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }


    public void ClearUI()
    {
        foreach (Transform ingredientPanel in transform)
        {
            DeleteChildrenOfTransform(ingredientPanel);
        }

        currentGridLayout = null;
        currentIngredientPanel = null;
        currentRow = 0;
        currentColumn = 0;

        AnimationManager.Instance.LowerAllIngredients();
    }
}