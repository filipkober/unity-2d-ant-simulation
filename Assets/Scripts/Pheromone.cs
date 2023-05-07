using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pheromone : MonoBehaviour
{
    // Start is called before the first frame update

    public PheromoneType type = PheromoneType.Undefined;
    [Range(0f,1f)]
    public float strength = 1f;

    public float maxLifeSpan = 5f;
    private float lifeSpan = float.MaxValue;

    private Vector2 initScale = new Vector2(0.3f, 0.3f);

    void Start()
    {
        lifeSpan = maxLifeSpan;
    }

    // Update is called once per frame
    void Update()
    {
        lifeSpan -= Time.deltaTime;
        if (lifeSpan <= 0)
        {
            Destroy(gameObject);
            return;
        }
        strength = lifeSpan / maxLifeSpan;
        var tempColor = GetComponent<SpriteRenderer>().color;
        tempColor.a = strength;

        GetComponent<SpriteRenderer>().color = tempColor;

    }

    public void setTowardsHome()
    {
        type = PheromoneType.TowardsHome;
        GetComponent<SpriteRenderer>().color = Color.blue;
    }

    public void setTowardsFood()
    {
        type = PheromoneType.TowardsFood;
        GetComponent<SpriteRenderer>().color = Color.green;
    }
}
