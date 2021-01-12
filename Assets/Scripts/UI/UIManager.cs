using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject craftingCanvas;
    public GameObject inventoryCanvas;
    public GameObject gameScreenCanvas;
    public GameObject pickUpCanvas;
    private bool craftingToggle = false;
    private bool inventoryPanelToggle = false;
    private bool gameScreenCanvasToggle = true;
    private bool pickUpCanvasToggle = false;
    private bool InGamePause = false;

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
            inventoryPanelToggle = false;
            inventoryCanvas.SetActive(inventoryPanelToggle);
            gameScreenCanvasToggle = false;
            gameScreenCanvas.SetActive(gameScreenCanvasToggle);

            craftingToggle = !craftingToggle;
            if(craftingToggle == false )
            {
                Resume();
            }
            else
            {
                PauseGame();
                craftingCanvas.SetActive(craftingToggle);
            }
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            craftingToggle = false;
            craftingCanvas.SetActive(craftingToggle);
            gameScreenCanvasToggle = false;
            gameScreenCanvas.SetActive(gameScreenCanvasToggle);

            inventoryPanelToggle = !inventoryPanelToggle;
            if(inventoryPanelToggle == false )
            {
                Resume();
            }
            else
            {
                PauseGame();
                inventoryCanvas.SetActive(inventoryPanelToggle);
            }
        }
    }
    public void Resume()
    {
        GameManager.instance.inMenu = false;
        Cursor.lockState = CursorLockMode.Locked;
        InGamePause = false;
        inventoryPanelToggle = false;
        inventoryCanvas.SetActive(inventoryPanelToggle);
        craftingToggle = false;
        craftingCanvas.SetActive(craftingToggle);
        gameScreenCanvasToggle = true;
        gameScreenCanvas.SetActive(gameScreenCanvasToggle);
        Time.timeScale = 1f;
    }
    
    public void onCanvasResume()
    {
        Debug.Log("Hello");
        GameManager.instance.inMenu = false;
        Cursor.lockState = CursorLockMode.Locked;
        InGamePause = false;
        inventoryPanelToggle = false;
        inventoryCanvas.SetActive(inventoryPanelToggle);
        craftingToggle = false;
        craftingCanvas.SetActive(craftingToggle);
        Time.timeScale = 1f;
    }


    public void PauseGame(){
        GameManager.instance.inMenu = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
        InGamePause = true;
    }

    public void unLockCursor(){
        GameManager.instance.inMenu = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void lockCursor(){
        GameManager.instance.inMenu = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
