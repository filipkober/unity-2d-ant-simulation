using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    private Color initColor;
    UIController uIController;

    void Start()
    {
        initColor = GetComponent<SpriteRenderer>().color;
        uIController = GameObject.Find("UI").GetComponent<UIController>();
    }

    void Update()
    {
        if(uIController.currentMode == UIController.UIMode.Destroy && Input.GetMouseButton(0))
        {
            uIController.ResetUI();
            Destroy(gameObject);
        }
    }
    private void OnMouseEnter()
    {
        if (uIController.currentMode == UIController.UIMode.Destroy)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
    private void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = initColor;
    }
}
