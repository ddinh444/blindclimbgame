using System;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public TextMeshProUGUI inventoryItemsText;


    // Update is called once per frame
    void Update()
    {
        String text = "\n";
        for(int i = 0; i < Inventory.Instance.regularItems.Count; i++)
        {
            Pickup p = Inventory.Instance.regularItems[i];
            if(i == Inventory.Instance.selectedItemIndex)
            {
                text += $"<u>{p.itemName}({p.GetWeight():F1} kg)</u>\n";
            }
            else
            {
                text += $"{p.itemName}({p.GetWeight():F1} kg)\n";
            }
        }

        inventoryItemsText.text = text;
    }
}
