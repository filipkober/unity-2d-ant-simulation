using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public int foodInside = 100;
    public int maxFoodInside = int.MaxValue;

    private void Start()
    {
        maxFoodInside = foodInside;
    }

    public void foodGrabbed()
    {
        foodInside--;
        if (foodInside == 0) Destroy(gameObject);
        var sr = GetComponent<SpriteRenderer>();
        var tempColor = sr.color;
        tempColor.a = (float)foodInside / maxFoodInside;
        sr.color = tempColor;
    }
    public void setFoodInside(int n)
    {
        maxFoodInside = n;
        foodInside = n;
        var sr = GetComponent<SpriteRenderer>();
        var tempColor = sr.color;
        tempColor.a = 1;
        sr.color = tempColor;
    }
}
