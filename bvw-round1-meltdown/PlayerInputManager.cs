/*
 * This script uses Unity's new input system, connected with the buttons on the
 * adaptive controller's custom interface, to trigger gameplay actions related to
 * making a sandwich.
 */

using System.Collections.Generic;
using UnityEngine;
using static IngredientManager;
using static PopUpManager;

public class PlayerInputManager : MonoBehaviour
{
    public Ingredient selectedIngredient = Ingredient.None;
    public IngredientCategory selectedIngredientCategory = IngredientCategory.None;
    private List<Ingredient> currentSandwich = new List<Ingredient>();

    public void PlayerSelectingNoIngredient()
    {
        selectedIngredient = Ingredient.None;
        selectedIngredientCategory = IngredientCategory.None;
    }

    private void OnPlate()
    {
        // Nothing else to do if there was no ingredient selected
        if (selectedIngredient == Ingredient.None) 
        { 
            PopUpManager.Instance.ShowPopUp(PopUpType.NoSelectedIngredient);
            return; 
        }

        // Play sound effect for ingredient dropping on plate
        AudioManager.Instance.PlayFoodOnPlateSound(selectedIngredientCategory);

        // Update player sandwich and UI, drop ingredient on plate
        currentSandwich.Add(selectedIngredient);
        PlayerSandwichUIController.Instance.AddIngredientToPanel(selectedIngredient);
        IngredientDropController.Instance.DropIngredient(selectedIngredient);

        // Player is no longer holding an ingredient after dropping it
        GameManager.Instance.SetSelectedIngredientOnAllInputManager(IngredientCategory.None);
        IngredientOptionUIController.Instance.ClearUI();
    }

    private void OnSubmit()
    {
        if (GameManager.Instance.customerSpeaking) return;

        // Trigger various pop-ups for empty sandwich
        if (currentSandwich.Count == 0)
        {
            if (NPCManager.Instance.currentNPC == CustomerName.Player)
            {
                // player trying to eat an empty sandwich 
                PopUpManager.Instance.ShowPopUp(PopUpType.EmptyPlateSubmitPlayer);
            }
            else if ((int)NPCManager.Instance.currentNPC < (int)CustomerName.Robot1)
            {
                // giving an empty sandwich to an NPC
                PopUpManager.Instance.ShowPopUp(PopUpType.EmptyPlateSubmitCustomer);
            }
            else
            {
                // giving an empty sandwich to the robot
                PopUpManager.Instance.ShowPopUp(PopUpType.EmptyPlateSubmitRobot);
            }
            return;
        }

        NPCManager.Instance.HandlePlayerSandwichSubmit(currentSandwich);

        // Clear out the player's sandwich and UI/environment, play sound effect
        GameManager.Instance.SetSelectedIngredientOnAllInputManager(IngredientCategory.None);
        currentSandwich.Clear();
        PlayerSandwichUIController.Instance.ClearPanel();
        IngredientOptionUIController.Instance.ClearUI();
        AudioManager.Instance.PlaySandwichSubmitSound();
        IngredientDropController.Instance.ClearSandwichModelOnPlate();
    }

    private void PressedIngredientButton(IngredientCategory ingredientCategory)
    {
        if (GameManager.Instance.customerSpeaking)
        {
            PopUpManager.Instance.ShowPopUp(PopUpType.CustomerSpeaking);
            return;
        }

        if (currentSandwich.Count >= 10)
        {
            PopUpManager.Instance.ShowPopUp(PopUpType.IngredientLimit);
            return;
        }

        // Pressed the same ingredient button twice to deselct it
        if (selectedIngredientCategory == ingredientCategory)
        {
            GameManager.Instance.SetSelectedIngredientOnAllInputManager(IngredientCategory.None);
            IngredientOptionUIController.Instance.ClearUI();
            AudioManager.Instance.PlayDeselectSound();
        }
        // Switching ingredient category
        else
        {
            IngredientOptionUIController.Instance.SetUpIngredientGridLayout(ingredientCategory);
            GameManager.Instance.SetSelectedIngredientOnAllInputManager(ingredientCategory);
            AudioManager.Instance.PlayIngredientClickSound();
        }
    }

    private void OnBread()
    {
        PressedIngredientButton(IngredientCategory.Bread);
    }

    private void OnMeat()
    {
        PressedIngredientButton(IngredientCategory.Meat);
    }
    private void OnCheese()
    {
        PressedIngredientButton(IngredientCategory.Cheese);
    }

    private void OnVeg()
    {
        PressedIngredientButton(IngredientCategory.Veggie);
    }

    private void OnSauce()
    {
        PressedIngredientButton(IngredientCategory.Sauce);
    }

    private void OnTrash()
    {
        if (GameManager.Instance.customerSpeaking) return;

        currentSandwich.Clear();
        GameManager.Instance.SetSelectedIngredientOnAllInputManager(IngredientCategory.None); // player shouldn't hold ingredient when throwing in trash
        PlayerSandwichUIController.Instance.ClearPanel();
        IngredientOptionUIController.Instance.ClearUI();
        AudioManager.Instance.PlayTrashSound();
        IngredientDropController.Instance.ClearSandwichModelOnPlate();
    }

    private void OnUp()
    {
        IngredientOptionUIController.Instance.SelectDifferentIngredientInGrid(0, -1);
    }

    private void OnDown()
    {
        IngredientOptionUIController.Instance.SelectDifferentIngredientInGrid(0, 1);
    }

    private void OnLeft()
    {
        IngredientOptionUIController.Instance.SelectDifferentIngredientInGrid(-1, 0);
    }

    private void OnRight()
    {
        IngredientOptionUIController.Instance.SelectDifferentIngredientInGrid(1, 0);
    }
}