using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public enum UIMode
    {
        SpawnAnts,
        SpawnFood,
        None
    }
    [Header("UI GameObjects")]
    public TMPro.TMP_InputField mapWidthField;
    public TMPro.TMP_InputField mapHeightField;
    public TMPro.TMP_InputField antsToSpawn;
    public TMPro.TMP_InputField foodInSource;
    public TMPro.TMP_Text modeText;

    [Header("Prefabs")]
    public GameObject antPrefab;
    public GameObject antHomePrefab;
    public GameObject foodPrefab;

    [Header("Misc")]
    public GameObject map;
    public UIMode currentMode = UIMode.None;

    private GameObject preview;
    private Vector3 antHomeSize = new Vector3(1, 1);
    private Vector3 antSize = new Vector3(1, 1);
    private Vector3 foodSize = new Vector3(1.8f, 1.8f);

    void Start()
    {

    }
    void Update()
    {
        var camPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        camPos.z = 0;
        if (preview != null)
        {
            preview.transform.position = camPos;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (currentMode == UIMode.SpawnAnts)
            {
                int antsToSpawnNum = 0;
                if (int.TryParse(antsToSpawn.text, out antsToSpawnNum) && antsToSpawnNum > 0)
                {
                    var antHome = Instantiate(antHomePrefab, parent: map.transform, position: camPos, rotation: Quaternion.identity);
                    for (int i = 0; i < antsToSpawnNum; i++)
                    {
                        Instantiate(antPrefab, parent: antHome.transform, position: antHome.transform.position, rotation: Quaternion.identity);
                    }
                }
                ResetUI();
            }
            else if (currentMode == UIMode.SpawnFood)
            {
                int foodNum = 0;
                if (int.TryParse(foodInSource.text, out foodNum) && foodNum > 0)
                {
                    var food = Instantiate(foodPrefab, parent: map.transform, position: camPos, rotation: Quaternion.identity);
                    food.GetComponent<Food>().setFoodInside(foodNum);
                }
                ResetUI();
            }
        }
    }
    public void ResetUI()
    {
        Destroy(preview);
        currentMode = UIMode.None;
        modeText.text = "Current mode: none";
    }

    public void NewMap()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        int mapWidth, mapHeight = 0;
        if (int.TryParse(mapWidthField.text, out mapWidth) && int.TryParse(mapHeightField.text, out mapHeight) && mapWidth > 0 && mapHeight > 0)
        {
            var mapGen = map.GetComponent<MapGenerator>();
            mapGen.SetWidthAndHeight(mapWidth, mapHeight);
            mapGen.GenerateMap();
        }
    }

    public void PlaceAnts()
    {
        ResetUI();
        var antHome = Instantiate(antHomePrefab);
        Destroy(antHome.GetComponent<AntHome>());
        var c = antHome.GetComponent<SpriteRenderer>().color;
        c.a = 0.5f;
        antHome.GetComponent<SpriteRenderer>().color = c;
        preview = antHome;
        modeText.text = "Current mode: Spawn Ants";
        currentMode = UIMode.SpawnAnts;
    }

    public void PlaceFood()
    {
        ResetUI();
        var food = Instantiate(foodPrefab);
        Destroy(food.GetComponent<Food>());
        var c = food.GetComponent<SpriteRenderer>().color;
        c.a = 0.5f;
        food.GetComponent<SpriteRenderer>().color = c;
        preview = food;
        modeText.text = "Current mode: Spawn Food";
        currentMode = UIMode.SpawnFood;
    }

}
