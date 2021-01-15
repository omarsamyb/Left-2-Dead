using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryObject ingredientInventory;
    public InventoryObject ammoInventory;
    public InventoryObject craftableInventory;
    GameObject player;
    public int[] collectableItemsCount;
    public float distanceFromPlayer = 2;
    public LayerMask m_LayerMask;
    Vector3 size = new Vector3(2f,2f,2f);
    int compMult = 1;
    void Start(){
        player = PlayerController.instance.player;
        if(GameManager.instance.companionId == 1)
            compMult = 2;
    }

    void FixedUpdate(){
        GetCollectables();
    }

    void GetCollectables(){
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, size, Quaternion.identity, m_LayerMask);
        collectableItemsCount = new int[13];
        if(hitColliders.Length > 0){
            foreach (Collider col in hitColliders){
                Item itemScript = col.gameObject.GetComponent<Item>();
                if(itemScript.item.name == "Heavy Ammo"){
                    collectableItemsCount[0] += itemScript.amount * compMult;
                }
                else if(itemScript.item.name == "Light Ammo"){
                    collectableItemsCount[1] += itemScript.amount * compMult;
                }
                else if(itemScript.item.name == "Shotgun shells"){
                    collectableItemsCount[2] += itemScript.amount * compMult;
                }
                else if(itemScript.item.name == "Alcohol"){
                    collectableItemsCount[3] += itemScript.amount * compMult;
                }
                else if(itemScript.item.name == "Canister"){
                    collectableItemsCount[4] += itemScript.amount * compMult;
                }
                else if(itemScript.item.name == "Sugar"){
                    collectableItemsCount[5] += itemScript.amount * compMult;
                }
                else if(itemScript.item.name == "GunPowder"){
                    collectableItemsCount[6] += itemScript.amount * compMult;
                }
                else if(itemScript.item.name == "Rag"){
                    collectableItemsCount[7] += itemScript.amount * compMult;
                }
                else if(itemScript.item.name == "Health pack"){
                    collectableItemsCount[8] += itemScript.amount;
                }
                else if(itemScript.item.name == "Assault Rifle"){
                    collectableItemsCount[9] += itemScript.amount;
                }
                else if(itemScript.item.name == "Hunting Rifle"){
                    collectableItemsCount[10] += itemScript.amount;
                }
                else if(itemScript.item.name == "Submachine Gun"){
                    collectableItemsCount[11] += itemScript.amount;
                }
                else if(itemScript.item.name == "Tactical Shotgun"){
                    collectableItemsCount[12] += itemScript.amount;
                }
            }
        }
    }


    public void pickUp(string itemName){

        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, size, Quaternion.identity, m_LayerMask);
        foreach (Collider col in hitColliders){
            Item itemScript = col.gameObject.GetComponent<Item>();
            if(itemScript.item.name == itemName){
                Destroy(col.gameObject);
            }
        }

        if(itemName == "Heavy Ammo"){
            ammoInventory.addItem(itemName, collectableItemsCount[0]);
            if(PlayerController.instance.player.GetComponent<GunInventory>().AddAmmo("Assault Rifle",collectableItemsCount[0]/2) == 1)
                PlayerController.instance.player.GetComponent<GunInventory>().AddAmmo("Hunting Rifle",collectableItemsCount[0]/2);
            else
                PlayerController.instance.player.GetComponent<GunInventory>().AddAmmo("Hunting Rifle",collectableItemsCount[0]);
        }
        else if(itemName == "Light Ammo"){
            ammoInventory.addItem(itemName, collectableItemsCount[1]);
            PlayerController.instance.player.GetComponent<GunInventory>().AddAmmo("Submachine Gun",collectableItemsCount[1]);
        }
        else if(itemName == "Shotgun shells"){
            ammoInventory.addItem(itemName, collectableItemsCount[2]);
            PlayerController.instance.player.GetComponent<GunInventory>().AddAmmo("Tactical Shotgun",collectableItemsCount[2]);
        }
        else if(itemName == "Alcohol"){
            ingredientInventory.addItem(itemName, collectableItemsCount[3]);
        }
        else if(itemName == "Canister"){
            ingredientInventory.addItem(itemName, collectableItemsCount[4]);
        }
        else if(itemName == "Sugar"){
            ingredientInventory.addItem(itemName, collectableItemsCount[5]);
        }
        else if(itemName == "GunPowder"){
            ingredientInventory.addItem(itemName, collectableItemsCount[6]);
        }
        else if(itemName == "Rag"){
            ingredientInventory.addItem(itemName, collectableItemsCount[7]);
        }
        else if(itemName == "Health pack"){
            craftableInventory.addItem(itemName, collectableItemsCount[8]);
        }
        else if(itemName == "Assault Rifle"){
            PlayerController.instance.player.GetComponent<GunInventory>().AddWeapon("Assault Rifle");
            print(itemName);
        }
        else if(itemName == "Hunting Rifle"){
            PlayerController.instance.player.GetComponent<GunInventory>().AddWeapon("Hunting Rifle");
        }
        else if(itemName == "Submachine Gun"){
            PlayerController.instance.player.GetComponent<GunInventory>().AddWeapon("Submachine Gun");
        }
        else if(itemName == "Tactical Shotgun"){
            PlayerController.instance.player.GetComponent<GunInventory>().AddWeapon("Tactical Shotgun");
        }
    }

    IEnumerator initCompMult()
    {
        yield return new WaitForSeconds(0.1f);
        if(GameManager.instance.companionId == 1)
            compMult = 2;
    }

    public void pickUpAmmoCheat(){
        ammoInventory.addItem("Heavy Ammo", 450+165);
        PlayerController.instance.player.GetComponent<GunInventory>().AddAmmo("Assault Rifle",450);
        PlayerController.instance.player.GetComponent<GunInventory>().AddAmmo("Hunting Rifle",165);

        ammoInventory.addItem("Light Ammo", 700);
        PlayerController.instance.player.GetComponent<GunInventory>().AddAmmo("Submachine Gun",700);

        ammoInventory.addItem("Shotgun shells", 130);
        PlayerController.instance.player.GetComponent<GunInventory>().AddAmmo("Tactical Shotgun",130);
    }
}
