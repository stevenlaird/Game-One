using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StorageBarrelController : MonoBehaviour
{
    private InventoryManager inventory;

    private Sprite storageSpriteClosed;
    private Sprite storageSpriteOpened;
    public bool storageInRange;
    public bool storageEmpty;

    private GameObject storageParent;
    public InvSlotClass[] storageItems;
    private GameObject[] storageSlots;

    private InvSlotClass grabItemOrigin;
    private InvSlotClass changeSlotTemp;

    ///////////////////

    void Start()
    {
        inventory = InventoryManager.FindObjectOfType<InventoryManager>();
        storageSpriteClosed = Resources.Load<Sprite>("StorageBarrel");
        storageSpriteOpened = Resources.Load<Sprite>("StorageBarrelOpen");

        storageInRange = false;
        storageEmpty = true;

        this.tag = "StorageBarrel";
        this.AddComponent<BoxCollider2D>().isTrigger = true;
        this.GetComponent<BoxCollider2D>().size = new Vector2(2, 1);


        storageParent = inventory.transform.GetChild(3).gameObject.transform.GetChild(6).gameObject;

        storageSlots = new GameObject[storageParent.transform.childCount];
        storageItems = new InvSlotClass[storageSlots.Length];
        for (int i = 0; i < storageParent.transform.childCount; i++)
        {
            storageSlots[i] = storageParent.transform.GetChild(i).gameObject;
        }
        for (int i = 0; i < storageItems.Length; i++)
        {
            storageItems[i] = new InvSlotClass();
        }

    }

    void Update()
    {
        HighlightHoveredSlot();

        if (storageInRange && inventory.transform.GetChild(3).transform.GetChild(6).gameObject.activeSelf == true)
        {
            LoadStorageItems();
            CheckIfEmpty();

            if (Input.GetMouseButtonDown(0)) //left click
            {
                if (inventory.grabbingItem)
                {
                    DropItemLeftClick();
                }
                else
                {
                    GrabItemLeftClick();
                }
            }
            else if (Input.GetMouseButtonDown(1)) //right click
            {
                if (inventory.grabbingItem)
                {
                    DropItemRightClick();
                }
                else
                {
                    GrabItemRightClick();
                }
            }
        }
    }

    private void LoadStorageItems()
    {
        for (int i = 0; i < storageSlots.Length; i++)
        {
            try
            {
                storageSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = true;
                storageSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = storageItems[i].GetItem().itemSprites[0];

                if (storageItems[i].GetQuantity() == 1)
                    storageSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                else
                    storageSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = storageItems[i].GetQuantity() + "";
                if (storageItems[i].GetItem().GetTool() || storageItems[i].GetItem().GetArmor())
                {
                    storageSlots[i].transform.GetChild(2).gameObject.SetActive(true);
                    storageSlots[i].transform.GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()), 1, 1);
                    if (((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) == 1)
                    {
                        storageSlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.white;
                        storageSlots[i].transform.GetChild(2).gameObject.SetActive(false);
                    }
                    else if (((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) <= .99 && ((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) > .50)
                    {
                        storageSlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.green;
                    }
                    if (((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) <= .50 && ((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) > .25)
                    {
                        storageSlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.yellow;
                    }
                    if (((float)storageItems[i].GetCurrentDurability() / (float)storageItems[i].GetStartingDurability()) <= .25)
                    {
                        storageSlots[i].transform.GetChild(2).GetComponent<Image>().color = Color.red;
                    }
                }
                else
                    storageSlots[i].transform.GetChild(2).gameObject.SetActive(false);
            }
            catch
            {
                storageSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
                storageSlots[i].transform.GetChild(0).GetComponent<Image>().enabled = false;
                storageSlots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
                storageSlots[i].transform.GetChild(2).gameObject.SetActive(false);
            }
        }
    }

    private void CheckIfEmpty()
    {
        storageEmpty = true;

        for (int i = 0; i < storageItems.Length; i++)
        {
            if (storageItems[i].GetItem() != null)
            {
                storageEmpty = false;
            }
        }
    }

    public void HighlightHoveredSlot()
    {
        for (int i = 0; i < storageSlots.Length; i++)
        {
            if (Vector2.Distance(storageSlots[i].transform.position, Input.mousePosition) <= 29)
            {
                storageSlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlotHover");
            }
            else
            {
                storageSlots[i].GetComponent<Image>().sprite = Resources.Load<Sprite>("InventorySlot");
            }
        }
    }

    public InvSlotClass Contains(ItemClass item)
    {
        for (int i = 0; i < storageItems.Length; i++)
        {
            if (storageItems[i].GetItem() == item)
                return storageItems[i];
        }

        return null;
    }

    private InvSlotClass GetClosestSlot()
    {
        for (int i = 0; i < storageSlots.Length; i++)
        {
            if (Vector2.Distance(storageSlots[i].transform.position, Input.mousePosition) <= 30)
            {
                return storageItems[i];
            }
        }

        return null;
    }

    private bool GrabItemLeftClick()
    {
        grabItemOrigin = GetClosestSlot();
        if (grabItemOrigin == null || grabItemOrigin.GetItem() == null)
        {
            return false;
        }
        inventory.grabItemMoving = new InvSlotClass(grabItemOrigin);
        grabItemOrigin.Clear();
        inventory.grabbingItem = true;
        return true;
    }

    private bool GrabItemRightClick()
    {
        grabItemOrigin = GetClosestSlot();
        if (grabItemOrigin == null || grabItemOrigin.GetItem() == null)
        {
            return false;
        }
        if (grabItemOrigin.GetQuantity() == 1)
        {
            inventory.grabItemMoving = new InvSlotClass(grabItemOrigin);
            grabItemOrigin.Clear();
        }
        else
        {
            inventory.grabItemMoving = new InvSlotClass(grabItemOrigin.GetItem(), Mathf.CeilToInt(grabItemOrigin.GetQuantity() / 2f), grabItemOrigin.GetStartingDurability(), grabItemOrigin.GetCurrentDurability());
            grabItemOrigin.SubtractQuantity(Mathf.CeilToInt(grabItemOrigin.GetQuantity() / 2f));
        }

        inventory.grabbingItem = true;
        inventory.RefreshUI();
        return true;
    }

    private bool DropItemLeftClick()
    {
        grabItemOrigin = GetClosestSlot();

        if (grabItemOrigin == null)
        { return false; }

        if (grabItemOrigin.GetItem() != null)
        {
            if (grabItemOrigin.GetItem() == inventory.grabItemMoving.GetItem()                 //grabbed item is same as item where dropped
                && grabItemOrigin.GetItem().stack                                    //grabbed item is stackable
                && grabItemOrigin.GetQuantity() < grabItemOrigin.GetItem().maxStack) //grabbed item quantity is less than a max stack of the item
            {
                int availableSpace = grabItemOrigin.GetItem().maxStack - grabItemOrigin.GetQuantity();
                int quantityToAdd = Mathf.Clamp(inventory.grabItemMoving.GetQuantity(), 0, availableSpace);
                int remainder = inventory.grabItemMoving.GetQuantity() - availableSpace;
                grabItemOrigin.AddQuantity(quantityToAdd);
                if (remainder <= 0) { inventory.grabItemMoving.Clear(); }
                else
                {
                    inventory.grabItemMoving.SubtractQuantity(availableSpace);
                    inventory.RefreshUI();
                    return false;
                }
            }
            else
            {
                changeSlotTemp = new InvSlotClass(grabItemOrigin);
                grabItemOrigin.AddItem(inventory.grabItemMoving.GetItem(), inventory.grabItemMoving.GetQuantity(), inventory.grabItemMoving.GetStartingDurability(), inventory.grabItemMoving.GetCurrentDurability());
                inventory.grabItemMoving.AddItem(changeSlotTemp.GetItem(), changeSlotTemp.GetQuantity(), changeSlotTemp.GetStartingDurability(), changeSlotTemp.GetCurrentDurability());
                inventory.RefreshUI();
                return true;
            }
        }
        else //drop item in empty slot
        {
            grabItemOrigin.AddItem(inventory.grabItemMoving.GetItem(), inventory.grabItemMoving.GetQuantity(), inventory.grabItemMoving.GetStartingDurability(), inventory.grabItemMoving.GetCurrentDurability());
            inventory.grabItemMoving.Clear();
        }

        inventory.grabbingItem = false;
        inventory.RefreshUI();
        return true;
    }

    private bool DropItemRightClick()
    {
        grabItemOrigin = GetClosestSlot();

        if (grabItemOrigin == null)
        { return false; }

        if (grabItemOrigin.GetItem() != null //dropped on an available slot
            && (grabItemOrigin.GetItem() != inventory.grabItemMoving.GetItem() //not the same item OR location is at max stack
            || grabItemOrigin.GetQuantity() >= grabItemOrigin.GetItem().maxStack))
        {
            changeSlotTemp = new InvSlotClass(grabItemOrigin);
            grabItemOrigin.AddItem(inventory.grabItemMoving.GetItem(), inventory.grabItemMoving.GetQuantity(), inventory.grabItemMoving.GetStartingDurability(), inventory.grabItemMoving.GetCurrentDurability());
            inventory.grabItemMoving.AddItem(changeSlotTemp.GetItem(), changeSlotTemp.GetQuantity(), changeSlotTemp.GetStartingDurability(), changeSlotTemp.GetCurrentDurability());
            inventory.RefreshUI();
            return true;
        }

        if (grabItemOrigin != null && grabItemOrigin.GetItem() == inventory.grabItemMoving.GetItem())
        { grabItemOrigin.AddQuantity(1); }
        else
        { grabItemOrigin.AddItem(inventory.grabItemMoving.GetItem(), 1, inventory.grabItemMoving.GetStartingDurability(), inventory.grabItemMoving.GetCurrentDurability()); }
        inventory.grabItemMoving.SubtractQuantity(1);

        if (inventory.grabItemMoving.GetQuantity() < 1)
        { inventory.grabbingItem = false; inventory.grabItemMoving.Clear(); }
        else
        { inventory.grabbingItem = true; }

        inventory.RefreshUI();
        return true;
    }

    private void OnMouseUpAsButton()
    {
        if (storageInRange)
        {
            this.GetComponent<SpriteRenderer>().sprite = storageSpriteOpened;

            inventory.transform.GetChild(3).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(0).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(false);
            inventory.transform.GetChild(3).transform.GetChild(3).gameObject.SetActive(false);
            inventory.transform.GetChild(3).transform.GetChild(4).gameObject.SetActive(false);
            inventory.transform.GetChild(3).transform.GetChild(5).gameObject.SetActive(false);
            inventory.transform.GetChild(3).transform.GetChild(6).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(7).gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            storageInRange = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            storageInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            storageInRange = false;
            this.GetComponent<SpriteRenderer>().sprite = storageSpriteClosed;
            inventory.transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(3).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(4).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(5).gameObject.SetActive(true);
            inventory.transform.GetChild(3).transform.GetChild(6).gameObject.SetActive(false);
        }
    }
}
