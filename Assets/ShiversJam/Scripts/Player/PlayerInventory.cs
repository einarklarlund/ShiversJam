using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Item CurrentItem => _items[_index];
    public Transform ItemHoldTransform => _itemHoldTransform;

    [SerializeField]
    Transform _itemHoldTransform;
    List<Item> _items;
    int _index;

    public void Add(Item item)
    {
        _items.Add(item);
        SwitchToItem(item);
    }
    
    public void Remove(Item item)
    {
        _items.Remove(item);
    }

    // called when PlayerController.Message.CurrentItemRemoved is sent
    public void RemoveCurrentItem()
    {
        _items.RemoveAt(_index);
    }

    public void UseCurrentItem()
    {
        if(_items.Count > 0) 
            _items[_index].hub.Post(Item.Message.Used);
            _index -= _index == 0 ? 0 : 1;
    }

    public void SwitchCurrentItem(int direction)
    {
        if(direction != 0 && direction != 1)
        {
            Debug.LogError("[PlayerInventory] SwitchCurrentItem must be called with direction = 1 or -1");
        }

        _index = Mathf.Clamp(_index + direction, 0, _items.Count - 1);
    }

    public void SwitchToItem(Item item)
    {
        _index = _items.IndexOf(item);
        Debug.Log(_items.Count);
    }

    public bool HasItem(Item item)
    {
        return _items.Contains(item);
    }

    void Start()
    {
        _items = new List<Item>();
        if(!_itemHoldTransform)
            Debug.LogError("[PlayerInventory] Item hold transform must be set in the inspector");

        GetComponent<PlayerController>().hub.Connect(PlayerController.Message.CurrentItemDropped, RemoveCurrentItem);
    }
}