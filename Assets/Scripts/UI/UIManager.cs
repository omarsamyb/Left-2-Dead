using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    public GameObject craftingCanvas;
    public GameObject inventoryCanvas;
    public GameObject gameScreenCanvas;
    public GameObject pickUpCanvas;
    public GameObject gameOverScreen;

    private bool craftingToggle = false;
    private bool inventoryPanelToggle = false;
    private bool gameScreenCanvasToggle = true;
    private bool pickUpCanvasToggle = false;

    void Update(){
        if (Input.GetKeyDown(KeyCode.E))
        {
            if(gameScreenCanvasToggle){
                pickUpCanvasToggle = !pickUpCanvasToggle;
                if(pickUpCanvasToggle == false )
                {
                    lockCursor();
                }
                else
                {
                    unLockCursor();
                }
                pickUpCanvas.SetActive(pickUpCanvasToggle);
            }
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            LoadCrafting();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            LoadInventory();
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            LoadInventory();
        }
        if(GameManager.instance.isGameOver)
        {
            hideCraftingScreen();
            hideInventoryScreen();
            hideGameScreen();
            showGameOverScreen();
        }
    }
    public void Resume()
    {
        hideInventoryScreen();
        hideCraftingScreen();
        showGameScreen();
        GameManager.instance.inMenu = false;
    }
    
    public void RestartLevel()
    {
        SceneManager.LoadScene("CurrentLevel"); // ToDo Change the string with the actual level scene
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene"); // ToDo Change with actual main menu
    }

    public void LoadCrafting(){
        hideInventoryScreen();
        hideGameScreen();
        craftingToggle = !craftingToggle;
        if(craftingToggle == false )
        {
            Resume();
        }
        else
        {
            GameManager.instance.inMenu = true;
            craftingCanvas.SetActive(craftingToggle);
        }
    }
    public void LoadInventory(){
        hideCraftingScreen();
        hideGameScreen();
        inventoryPanelToggle = !inventoryPanelToggle;
        if(inventoryPanelToggle == false )
        {
            Resume();
        }
        else
        {
            GameManager.instance.inMenu = true;
            inventoryCanvas.SetActive(inventoryPanelToggle);
        }
    }
    public void unLockCursor(){
        GameManager.instance.inMenu = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void lockCursor(){
        GameManager.instance.inMenu = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void hideInventoryScreen()
    {
        inventoryPanelToggle = false;
        inventoryCanvas.SetActive(inventoryPanelToggle);
    }
    public void hideCraftingScreen()
    {
        craftingToggle = false;
        craftingCanvas.SetActive(craftingToggle);
    }
    public void hideGameScreen()
    {
        gameScreenCanvasToggle = false;
        gameScreenCanvas.SetActive(gameScreenCanvasToggle);
    }
    public void showGameScreen()
    {
        gameScreenCanvasToggle = true;
        gameScreenCanvas.SetActive(gameScreenCanvasToggle);
    }

    public void showGameOverScreen()
    {
        gameOverScreen.SetActive(true);
    }
    public void QuitGame(){
        Application.Quit();
    }
}
