using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public PlayerController playerController; // Reference to PlayerController
    public GameObject slotPrefab; // Prefab for inventory slots

    public int inventorySize = 8; // Number of slots in the hotbar
    private int previousInventorySize; // Tracks last known inventory size
    public List<Item> inventory = new List<Item>(); // Store actual item instances
    private int selectedIndex = 0; // Tracks which item is currently selected

    [SerializeField] private Sprite selectedSlotSprite;
    [SerializeField] private Sprite selectedSlotSpriteLeft;
    [SerializeField] private Sprite selectedSlotSpriteRight;

    [SerializeField] private Sprite normalSlotSprite;
    [SerializeField] private Sprite normalSlotSprite2;
    [SerializeField] private Sprite normalSlotSpriteLeft;
    [SerializeField] private Sprite normalSlotSpriteRight;


    [SerializeField] private Sprite emptySlot; //icon that essentially nothing
    [SerializeField] SpriteFlip flip;
    private int previousInvSize = 0;


    void Start()
    {
        Item.NoneItem.icon = emptySlot;
        previousInventorySize = inventorySize;
        previousInventorySize = inventorySize;
        InitializeSlots();
    }

    void Update()
    {
        GetRidOfExpiredItems();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!inventory[selectedIndex].canBeDropped) return;
            OnDropItem(selectedIndex);
        }
        if (Input.GetMouseButtonDown(1)) // 1 corresponds to the right mouse button
        {
            if (inventory[selectedIndex] == null) return;
            inventory[selectedIndex].isActive = true;
        }
        if (Input.GetMouseButtonDown(0)) // 1 corresponds to the left mouse button
        {
            if (!inventory[selectedIndex].isActive || inventory[selectedIndex] == Item.NoneItem) return;
            inventory[selectedIndex].Use();

            if (inventory[selectedIndex].isConsumable)
            {
                inventory[selectedIndex] = Item.NoneItem;
            }
            else if (inventory[selectedIndex].lifeLeft == inventory[selectedIndex].lifeTime)
            {
                StartCoroutine(RemoveAtEndOFLifeTime(inventory[selectedIndex].lifeTime));
            }

        }

        CheckInventorySizeChange();
        HandleScrollSelection();
    }

    private IEnumerator RemoveAtEndOFLifeTime(float life)
    {
        yield return new WaitForSeconds(life);
        inventory[selectedIndex] = Item.NoneItem;
    }

    private void GetRidOfExpiredItems()
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            Item item = inventory[i];
            if (item != Item.NoneItem && item.isLifeOver)
            {
                Destroy(item.gameObject);
                inventory[i] = Item.NoneItem;
            }
        }
    }

    // Instantiates slotPrefabs based on inventorySize
    private void InitializeSlots()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            Image slot = Instantiate(slotPrefab, transform).GetComponent<Image>(); // Creates slots as children of this object
            if (i == 0) slot.sprite = selectedSlotSpriteLeft;
            if (i == inventorySize - 1) slot.sprite = selectedSlotSpriteRight;
            inventory.Add(Item.NoneItem);
        }
    }

    // Checks if inventorySize has changed and adjusts slots accordingly
    private void CheckInventorySizeChange()
    {
        if (inventorySize != previousInventorySize)
        {
            if (inventorySize < 1) { inventorySize = 1; }
            AdjustInventorySlots();
            previousInventorySize = inventorySize; // Update tracking value
        }
    }

    // Adjusts slot count to match inventorySize
    private void AdjustInventorySlots()
    {
        int currentSlotCount = transform.childCount;

        if (inventorySize > currentSlotCount) // Need more slots
        {
            for (int i = 0; i < previousInventorySize; i++)
            {
                Image slot = transform.GetChild(i).gameObject.GetComponent<Image>();
                if (slot.sprite == normalSlotSpriteRight) slot.sprite = normalSlotSprite;
                if (slot.sprite == selectedSlotSpriteRight) slot.sprite = selectedSlotSprite;
            }

            for (int i = currentSlotCount; i < inventorySize; i++)
            {
                GameObject slot = Instantiate(slotPrefab, transform);
                inventory.Add(Item.NoneItem);
                if (i == inventorySize -1) slot.GetComponent<Image>().sprite = normalSlotSpriteRight;
            }
        }
        else if (inventorySize < currentSlotCount) // Need fewer slots
        {

            for (int i = currentSlotCount - 1; i >= inventorySize; i--)
            {
                Destroy(transform.GetChild(i).gameObject); // Remove extra slots

                // Remove excess items from inventory
                if (inventory[i] != Item.NoneItem)
                {
                    OnDropItem(i);
                    inventory.RemoveAt(i);
                }
            }

            if (selectedIndex >= transform.childCount - 1) // if the selected slot is removed, select the last available slot
            {
                selectedIndex = transform.childCount - 2;
                transform.GetChild(selectedIndex).gameObject.GetComponent<Image>().sprite = selectedSlotSpriteRight;
            }
            else
            {
                transform.GetChild(inventorySize - 1).gameObject.GetComponent<Image>().sprite = normalSlotSpriteRight;
            }

        }

        previousInventorySize = inventorySize;
        UpdateUI();
    }

    private void OnDropItem(int index)
    {
        if (inventory[index] == Item.NoneItem) return; // to check that we are trying to drop an item from an empty slot
        Item item = inventory[index].GetComponent<Item>();
        item.GroundState(); //set the state to a ground item
        int direction = flip.isFacingRight ? 1 : -1; // to know which direction to drop the item in
        item.rb.velocity = new Vector2(direction * 2.5f, 5); // drop movement
        inventory[index] = Item.NoneItem; // setting the item to nothing
    }

    // Handles scrolling through the hotbar slots
    private void HandleScrollSelection()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Item item;

        if (scroll > 0f) // Scroll up
        {
            item = inventory[selectedIndex]; // item and the if statement under just set the items visiblity to true or false when scrolling through the hot bar
            if (item != null) item.sr.enabled = false;

            selectedIndex = Mathf.Clamp(selectedIndex + 1, 0, inventory.Count - 1);
            item = inventory[selectedIndex];

            if (item != null) item.sr.enabled = true;
        }
        else if (scroll < 0f) // Scroll down
        {
            item = inventory[selectedIndex];
            if (item != null) item.sr.enabled = false;

            selectedIndex = Mathf.Clamp(selectedIndex - 1, 0, inventory.Count - 1);

            item = inventory[selectedIndex];
            if (item != null) item.sr.enabled = true;
        }

        UpdateUI();
    }


    // Updates the UI elements (assigns icons & highlights selection)
    private void UpdateUI()
    {
        if (inventory[selectedIndex] != Item.NoneItem)
        {
            inventory[selectedIndex].GetComponent<Item>().sr.enabled = true;
        }

        for (int i = 0; i < inventorySize; i++) // Guaranteed to match slot count
        {
            Transform slot = transform.GetChild(i);
            Image slotImage = slot.GetComponent<Image>();
            Image itemIcon = slot.GetChild(0).GetComponent<Image>(); // Get child Image for item icon
            Slider itemLifeBar = slot.GetChild(1).GetComponent<Slider>();
            GameObject fillArea = itemLifeBar.transform.GetChild(0).gameObject;

            // Just set the icon directly, since inventory and icons are guaranteed to align
            itemIcon.sprite = (inventory[i] == Item.NoneItem) ? emptySlot : inventory[i].icon;

            Item item = inventory[i];
            if (item != null && item.isActive && !item.canBePickedUpWhileActive)
            {
                fillArea.SetActive(true); // Enable life bar image component
                // Update the fill amount of the UI Image component (assuming it's an Image with a filled type)
                itemLifeBar.value = item.lifeLeft / item.lifeTime; // Calculate percentage
            }
            else if (item == Item.NoneItem)
            {
                itemLifeBar.value = 1;
                fillArea.SetActive(false); // Hide life bar when item is not active
            }
            // Highlight selected slot
            switch (i)
            {
                case 0:
                    slotImage.sprite = (i == selectedIndex) ? selectedSlotSpriteLeft : normalSlotSpriteLeft;
                    break;

                case var _ when i == (inventorySize - 1):
                    slotImage.sprite = (i == selectedIndex) ? selectedSlotSpriteRight : normalSlotSpriteRight;
                    break;

                default:
                    Sprite slotToUse = (i % 2 == 0) ? normalSlotSprite : normalSlotSprite2;
                    slotImage.sprite = (i == selectedIndex) ? selectedSlotSprite : slotToUse;
                    break;
            }

        }
    }

}
