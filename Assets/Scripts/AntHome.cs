using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntHome : MonoBehaviour
{
    private int foodInside = 0;
    public TextMesh numberText;

    public void addFood()
    {
        foodInside++;
        numberText.text = foodInside.ToString();
    }

}
