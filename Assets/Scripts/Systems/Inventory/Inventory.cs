using System;
using System.Collections.Generic;
using UnityEngine;

public enum KeyItems
{
    Nothing,
    Level1Key,
    Level3Key_1,
    Level3Key_2,
    Level3Key_3
}

public class Inventory : MonoBehaviour
{

    public static Inventory Instance {get; private set;}

    private HashSet<KeyItems> keyItems;
    public List<Pickup> regularItems {get; private set;}
    public int selectedItemIndex{get; private set;}

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        keyItems = new HashSet<KeyItems>();
        regularItems = new List<Pickup>();
        selectedItemIndex = -1;
    }

    public void AddKeyItem(KeyItems item)
    {
        keyItems.Add(item);
    }

    public bool HasKeyItem(KeyItems item)
    {
        return keyItems.Contains(item);
    }

    public void AddItem(Pickup p)
    {
        if(selectedItemIndex == -1)
        {
            selectedItemIndex = 0;
        }
        regularItems.Add(p);
        p.gameObject.SetActive(false);
    }

    public void UpdateSelectedItem(int indexUpdate)
    {
        selectedItemIndex += indexUpdate;
        if(selectedItemIndex < 0)
        {
            selectedItemIndex = regularItems.Count - 1;
        }
        if(selectedItemIndex >= regularItems.Count)
        {
            selectedItemIndex = 0;
        }
    }

    public Pickup GetSelectedItem(Vector3 position)
    {
        Pickup p = regularItems[selectedItemIndex];
        regularItems.RemoveAt(selectedItemIndex);
        UpdateSelectedItem(0);
        p.gameObject.transform.position = position;
        p.noiseTimer = 0.5f;
        p.gameObject.SetActive(true);
        return p;
    }

    public float GetTotalWeight()
    {
        float weight = 0;
        foreach(Pickup p in regularItems)
        {
            weight += p.GetWeight();
        }
        return weight;
    }
}