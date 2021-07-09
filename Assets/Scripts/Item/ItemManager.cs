using System.Collections;
using System.Collections.Generic;
using Prime31.ZestKit;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    List<Item> _items;

    class RemoveItemsParams
    {
        public Item item;
        public float delay;
    }

    public void RemoveItem(Item item, float delay = 0)
    {
        if(_items.Contains(item))
            _items.Remove(item);

        StartCoroutine("RemoveItemEnumerator", new RemoveItemsParams() 
            {
                item = item,
                delay = delay
            });
    }

    IEnumerator RemoveItemEnumerator(RemoveItemsParams paramsObj)
    {
        yield return new WaitForSeconds(paramsObj.delay);

        Destroy(paramsObj.item.gameObject);
    }

    void Start()
    {
        _items = new List<Item>();
    }
}