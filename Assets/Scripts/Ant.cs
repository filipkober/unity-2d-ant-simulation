using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ant : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject viewTarget = null;
    public GameObject bg;
    public GameObject pheromone;

    [Header("General speed controls")]
    [Range(1f, 20f)]
    public float maxSpeed = 5;
    [Range(1f, 20f)]
    public float maxForce = 15;
    [Range(1f, 20f)]
    public float mass = 15;

    [Header("Obstacle avoidance controls")]
    [Range(1f, 20f)]
    public float maxAvoidForce = 5f;
    [Range(1f, 10f)]
    public float seeAheadDistance = 2.5f;
    [Range(1, 10)]
    public int rayNumber = 3;
    [Range(30f, 90f)]
    public float visionConeWidthDegrees = 30f;

    [Header("Wandering controls")]
    [Range(0f, 20f)]
    public float circleDistance = 0.08f;
    [Range(0f, 360f)]
    public float angleChange = 5f;

    [Header("Scanning controls")]
    [Range(1f, 10f)]
    public float scanDistance = 2.5f;

    [Header("Pheromone Controls")]
    public float pheromoneSpawnDelay = 0.5f;
    [Range(1f, 20f)]
    public float pheromoneAttractionForce = 1f;
    [Range(0f, 5f)]
    public float pheromoneDetectionRadius = 1f;
    [Range(30f, 360f)]
    public float pheromoneDetectionAngle = 60f;
    [Range(1f, 3f)]
    public float pheromoneDetectionDistance = 1f;

    [Header("Debug drawings")]
    public bool drawDetectionSphere = false;
    public bool drawCollisionDetectionRays = false;
    public bool drawDesiredVelocity = false;
    public bool drawVelocity = false;
    public bool drawPheromoneDetectionSpheres = false;
    public bool drawPheromoneAttractionForce = false;

    private Vector3 velocity;
    private float wanderAngle;
    private AntBehavior currentAction = AntBehavior.FindFood;
    private bool hasFood = false;

    LayerMask wallMask;
    LayerMask poiMask;
    LayerMask pheromoneMask;

    void Start()
    {
        velocity = Vector3.zero;
        wanderAngle = 0;

        bg = transform.parent.parent.gameObject;

        wallMask = LayerMask.GetMask("Wall");
        poiMask = LayerMask.GetMask("Point Of Interest");
        pheromoneMask = LayerMask.GetMask("Pheromone");

        InvokeRepeating("SpawnPheromones", pheromoneSpawnDelay, pheromoneSpawnDelay);
    }

    void Update()
    {

        // walking by using steering forces, yay!
        var currentPosition = transform.position;
        var targetPosition = Vector3.zero;
        if (viewTarget != null)
        {
            targetPosition = viewTarget.transform.position;
        }
        else
        {
            targetPosition = RandomPointInBounds();
        }

        var direction = targetPosition - currentPosition;
        var desiredVelocity = direction.normalized * maxSpeed;

        var steering = Vector3.zero;

        // if ant has nothing to do wander, else walk towards target

        if (viewTarget == null)
        {
            steering = Wander();
        }
        else
        {
            steering = desiredVelocity - velocity;
        }
        var pheromoneForce = DetectPheromones();
        if (drawPheromoneAttractionForce) Debug.DrawRay(transform.position, pheromoneForce.normalized * 2, Color.red);
        steering += pheromoneForce;
        steering += AvoidObstacles();

        steering = Vector3.ClampMagnitude(steering, maxForce);
        steering /= mass;

        // limit max velocity to max speed
        velocity = Vector3.ClampMagnitude((velocity + steering) * maxSpeed, maxSpeed);

        if(drawVelocity) Debug.DrawRay(transform.position, velocity, Color.green);
        if(drawDesiredVelocity) Debug.DrawRay(transform.position, desiredVelocity.normalized * 2, Color.magenta);

        var rotation = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        if (rotation < 0) rotation += 360;

        transform.SetPositionAndRotation(currentPosition + (velocity * Time.deltaTime), Quaternion.Euler(0, 0, rotation + 180));

        GameObject scanned = ScanFoodOrHome();
        if (scanned != null) viewTarget = (GameObject)scanned;
    }

    Vector3 Wander()
    {
        var circleCenter = velocity;
        circleCenter.Normalize();
        circleCenter *= circleDistance;

        var displacement = new Vector3(0, -1);
        displacement *= circleDistance;

        displacement = Quaternion.Euler(0, 0, wanderAngle) * displacement;

        wanderAngle += (Random.Range(0f, 1f) * angleChange) - (angleChange + 0.5f);

        var wanderForce = circleCenter + displacement;

        return wanderForce;
    }

    Vector3 RandomPointInBounds()
    {
        var boundsScale = bg.transform.localScale;
        var middle = bg.transform.position;

        var randomX = Random.Range(middle.x - (boundsScale.x / 2), middle.x + (boundsScale.x / 2));
        var randomY = Random.Range(middle.y - (boundsScale.x / 2), middle.y + (boundsScale.x / 2));

        return new Vector3(randomX, randomY);
    }

    Vector3 AvoidObstacles()
    {
        var avoidanceForce = Vector3.zero;
        float angleStep = 0;
        int raysTriggered = 0;

        if (rayNumber > 1) angleStep = visionConeWidthDegrees / (rayNumber - 1);

        for (int i = 0; i < rayNumber; i++)
        {
            float angle = -visionConeWidthDegrees / 2 + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, 0, angle) * velocity.normalized;
            Vector3? obstacle = ObstacleNearby(transform.position, direction, seeAheadDistance);

            if (obstacle.HasValue)
            {
                Vector3 obstaclePosition = (Vector3)obstacle;
                if (drawCollisionDetectionRays) Debug.DrawLine(transform.position, obstaclePosition, Color.red);

                Vector3 avoidance = transform.position - obstaclePosition;
                avoidance.Normalize();
                avoidance *= maxAvoidForce;
                avoidanceForce += avoidance;
                raysTriggered++;
            }
            else if(drawCollisionDetectionRays)
            {
                Debug.DrawRay(transform.position, direction * seeAheadDistance, Color.blue);
            }
        }

        if(raysTriggered == rayNumber)
        {
            //avoidanceForce = Quaternion.Euler(0, 0, Random.Range(0f, visionConeWidthDegrees / rayNumber)) * avoidanceForce * 2;
            //return avoidanceForce;
            velocity = Quaternion.Euler(0, 0, 180) * velocity.normalized * maxAvoidForce * 2;
        }

        if(drawCollisionDetectionRays) Debug.DrawRay(transform.position, avoidanceForce.normalized * 2, Color.white);

        return Vector3.ClampMagnitude(avoidanceForce,maxAvoidForce);
    }
#nullable enable
    Vector3? ObstacleNearby(Vector3 position, Vector3 direction, float magnitude)
    {
        var hit = Physics2D.Raycast(position, direction, magnitude, wallMask);
        return hit.collider != null ? new Vector3(hit.point.x, hit.point.y) : null;
    }
    GameObject? ScanFoodOrHome()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, scanDistance, poiMask);

        IEnumerable<GameObject> objectQuery =
            from hit in hits
            where (currentAction == AntBehavior.FindFood && hit.gameObject.GetComponent<Food>() != null) ||
            (currentAction == AntBehavior.ReturnHome && hit.gameObject.GetComponent<AntHome>() != null)
            select hit.gameObject;

        GameObject? hitObject = null;

        if(objectQuery.Count() > 0) hitObject = objectQuery.ToList()[0];
        if (hitObject is null) return hitObject;
        if (currentAction == AntBehavior.FindFood && hitObject.GetComponent<Food>()) return hitObject;
        if (currentAction == AntBehavior.ReturnHome && hitObject.gameObject.GetComponent<AntHome>()) return hitObject;
        return null;
    }
#nullable disable
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Food>() && currentAction == AntBehavior.FindFood)
        {
            currentAction = AntBehavior.ReturnHome;
            hasFood = true;
            this.GetComponent<SpriteRenderer>().color = Color.green;
            collision.gameObject.GetComponent<Food>().foodGrabbed();
            viewTarget = null;
            velocity = Quaternion.Euler(0, 0, 180) * velocity;
            return;
        }
        if (collision.gameObject.GetComponent<AntHome>() && currentAction == AntBehavior.ReturnHome)
        {
            if (hasFood)
            {
                hasFood = false;
                collision.gameObject.GetComponent<AntHome>().addFood();
                this.GetComponent<SpriteRenderer>().color = Color.white;
            }
            viewTarget = null;
            currentAction = AntBehavior.FindFood;
            velocity = Quaternion.Euler(0, 0, 180) * velocity;
            return;
        }
    }
    private void SpawnPheromones()
    {
        var newPheromone = Instantiate(pheromone, position: transform.position, Quaternion.identity).GetComponent<Pheromone>();
        if(currentAction == AntBehavior.FindFood)
        {
            newPheromone.setTowardsHome();
            return;
        }
        if(currentAction == AntBehavior.ReturnHome)
        {
            newPheromone.setTowardsFood();
            return;
        }
    }

    private Vector3 DetectPheromones()
    {
        var angleStep = pheromoneDetectionAngle / 3;
        float maxPheromoneConcentration = 0;
        Vector3 pheromoneForce = Vector3.zero;

        for (int i = 0; i < 3; i++)
        {
            float angle = -pheromoneDetectionAngle / 3 + angleStep * i;
            Vector3 newPosition = (Quaternion.Euler(0, 0, angle) * velocity.normalized).normalized * pheromoneDetectionDistance;
            var pheromonesInDetection = PheromonesInArea(newPosition);
            var pheromoneConcentration = pheromonesInDetection.Sum(p => p.strength);
            if (pheromoneConcentration > maxPheromoneConcentration)
            {
                pheromoneForce = newPosition.normalized * (pheromoneAttractionForce * (pheromoneConcentration / pheromonesInDetection.Count));
            }
        }

        return pheromoneForce;

    }

    private List<Pheromone> PheromonesInArea(Vector3 position)
    {

        var detection = Physics2D.OverlapCircleAll(transform.position + position, pheromoneDetectionRadius, pheromoneMask);

        var pheromonesInDetection = detection.ToList().Select(col => col.gameObject.GetComponent<Pheromone>()).ToList();

        if(currentAction == AntBehavior.FindFood)
        {
            pheromonesInDetection = pheromonesInDetection.Where(p => p.type == PheromoneType.TowardsFood).ToList();
        }
        else if(currentAction == AntBehavior.ReturnHome)
        {
            pheromonesInDetection = pheromonesInDetection.Where(p => p.type == PheromoneType.TowardsHome).ToList();
        }

        return pheromonesInDetection;
    }

    private void OnDrawGizmos()
    {
        if (drawDetectionSphere)
        {
            Gizmos.DrawSphere(transform.position, scanDistance);
        }
        if (drawPheromoneDetectionSpheres)
        {
            var angleStep = pheromoneDetectionAngle / 3;
            for (int i = 0; i < 3; i++)
            {
                float angle = -pheromoneDetectionAngle / 3 + angleStep * i;
                Vector3 newPosition = (Quaternion.Euler(0, 0, angle) * velocity.normalized).normalized * pheromoneDetectionDistance;
                Gizmos.DrawSphere(transform.position + newPosition, pheromoneDetectionRadius);
            }
        }
    }

}
