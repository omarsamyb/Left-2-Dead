﻿using System.Collections;
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
    public GameObject toolTipCanvas;
    public GameObject UICamera;
    public GameObject[] fireParticles;

    private bool craftingToggle = false;
    private bool inventoryPanelToggle = false;
    private bool gameScreenCanvasToggle = true;
    private bool pickUpCanvasToggle = false;
    private bool toolTipCanvasToggle = false;
    private bool UICameraToggle = false;

    void Update(){
        if (PlayerController.instance.isGettingPinned || PlayerController.instance.isPartiallyPinned || PlayerController.instance.isPinned || PlayerController.instance.health <= 0)
        {
            pickUpCanvasToggle = false;
            GameManager.instance.LockCursor();
            pickUpMenu(false);
            pickUpCanvas.SetActive(pickUpCanvasToggle);
        }
        if (PlayerController.instance.health > 0)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (gameScreenCanvasToggle)
                {
                    pickUpCanvasToggle = !pickUpCanvasToggle;
                    if (pickUpCanvasToggle)
                    {
                        GameManager.instance.UnlockCursor();
                        pickUpMenu(true);
                    }
                    else
                    {
                        GameManager.instance.LockCursor();
                        pickUpMenu(false);
                    }
                    pickUpCanvas.SetActive(pickUpCanvasToggle);
                }
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                LoadCrafting();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                LoadInventory();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LoadInventory();
            }
        }
        if(GameManager.instance.isGameOver)
        {
            showGameOverScreen();
        }

    }
    public void Resume()
    {
        GameManager.instance.inMenu = false;
        hideInventoryScreen();
        hideCraftingScreen();
        showGameScreen();
        TooltipSystem.Hide();
    }


    
    public void RestartLevel()
    {
        GameManager.instance.inMenu = false;
        GameManager.instance.inPickUp = false;
        GameManager.instance.isGameOver = false;
        GameManager.instance.RestartLevel();
    }

    public void ToMainMenu()
    {
        GameManager.instance.inMenu = false;
        SceneManager.LoadScene(0);
    }

    public void LoadCrafting(){
        hideInventoryScreen();
        hideGameScreen();
        craftingToggle = !craftingToggle;
        if(craftingToggle == false )
        {
            Resume();
            hideUICameraScreen();
        }
        else
        {
            GameManager.instance.inMenu = true;
            craftingCanvas.SetActive(craftingToggle);
            showUICameraScreen();
        }
    }
    public void LoadInventory(){
        hideCraftingScreen();
        hideGameScreen();
        inventoryPanelToggle = !inventoryPanelToggle;
        if(inventoryPanelToggle == false )
        {
            Resume();
            hideUICameraScreen();
        }
        else
        {
            GameManager.instance.inMenu = true;
            inventoryCanvas.SetActive(inventoryPanelToggle);
            showUICameraScreen();
        }
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
    public void hideToolTipScreen()
    {
        toolTipCanvasToggle = false;
        toolTipCanvas.SetActive(toolTipCanvasToggle);
    }
    public void showToolTipScreen()
    {
        toolTipCanvasToggle = true;
        toolTipCanvas.SetActive(toolTipCanvasToggle);
    }
    public void hideUICameraScreen()
    {
        foreach(GameObject i in fireParticles){
            i.GetComponent<UIparticleSystem>().hide();
            i.SetActive(false);
        }
    }
    public void showUICameraScreen()
    {
        foreach(GameObject i in fireParticles){
            i.SetActive(true);
        }
        // UICameraToggle = true;
        // UICamera.SetActive(UICameraToggle);
    }

    public void showGameOverScreen()
    {
        GameManager.instance.UnlockCursor();
        hideCraftingScreen();
        hideInventoryScreen();
        hideGameScreen();
        gameOverScreen.SetActive(true);
    }
    public void QuitGame(){
        Application.Quit();
    }

    public void pickUpMenu(bool b){
        GameManager.instance.inPickUp = b;
    }
}
