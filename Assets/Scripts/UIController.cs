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
        PlaceWall,
        Destroy,
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
    public GameObject wallPrefab;

    [Header("Misc")]
    public GameObject map;
    public UIMode currentMode = UIMode.None;

    private GameObject preview;
    private GameObject wallPreview;
    private Vector3 antHomeSize = new Vector3(1, 1);
    private Vector3 antSize = new Vector3(1, 1);
    private Vector3 foodSize = new Vector3(1.8f, 1.8f);
    private Vector3[] wallPoints = null;

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
        if(wallPreview != null && wallPoints != null)
        {
            wallPoints[1] = camPos;
            var direction = (wallPoints[0] - wallPoints[1]);
            var rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            wallPreview.transform.localScale = new Vector3(direction.magnitude, wallPreview.transform.localScale.y, 1);
            wallPreview.transform.SetPositionAndRotation(wallPoints[0] + direction * -0.5f, Quaternion.Euler(0,0, rotation));
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
                    antHome.AddComponent<Destroyable>();
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
                    food.AddComponent<Destroyable>();
                }
                ResetUI();
            }
            else if (currentMode == UIMode.PlaceWall)
            {
                if(wallPoints == null)
                {
                    Destroy(preview);
                    wallPoints = new Vector3[2] { camPos, camPos };
                    wallPreview = Instantiate(wallPrefab, camPos, Quaternion.identity);
                    var c = wallPreview.GetComponent<SpriteRenderer>().color;
                    c.a = 0.5f;
                    wallPreview.GetComponent<SpriteRenderer>().color = c;
                    modeText.text = "Current mode: Place Wall (click 2nd point)";
                }
                else
                {
                    var wall = Instantiate(wallPrefab, wallPreview.transform.position, wallPreview.transform.rotation);
                    wall.transform.localScale = wallPreview.transform.localScale;
                    wall.layer = LayerMask.NameToLayer("Wall");
                    wall.AddComponent<BoxCollider2D>();
                    wall.AddComponent<Destroyable>();
                    ResetUI();
                }
            }
        }
    }
    public void ResetUI()
    {
        if(preview) Destroy(preview);
        if(wallPreview) Destroy(wallPreview);
        wallPoints = null;
        currentMode = UIMode.None;
        modeText.text = "Current mode: none";
    }

    public void NewMap()
    {
        int mapWidth, mapHeight = 0;
        if (int.TryParse(mapWidthField.text, out mapWidth) && int.TryParse(mapHeightField.text, out mapHeight) && mapWidth > 0 && mapHeight > 0)
        {
            foreach(Transform child in map.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (GameObject child in GameObject.FindGameObjectsWithTag("Placed By User"))
            {
                if (child.GetComponent<Pheromone>()) Destroy(child);
            }
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

    public void PlaceWall()
    {
        ResetUI();
        modeText.text = "Current mode: Place Wall (click 1st point)";
        currentMode = UIMode.PlaceWall;
        preview = Instantiate(wallPrefab);
        var c = preview.GetComponent<SpriteRenderer>().color;
        c.a = 0.5f;
        preview.GetComponent<SpriteRenderer>().color = c;
    }

    public void DestroyMode()
    {
        currentMode = UIMode.Destroy;
        modeText.text = "Current mode: Destroy";
    }

    public void CloseGame()
    {
        Application.Quit();
    }

}
