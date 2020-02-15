using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using TMPro;

public class PenguinArea : Area
{
    [Tooltip("The agent inside the area")]
    public PenguinAgent penguinAgent;

    [Tooltip("The baby penguin inside the area")]
    public GameObject penguinBaby;

    [Tooltip("The text mesh pro that shows the cumulative reward of the agent")]
    public TextMeshPro cumulativeRewardText;

    [Tooltip("Prefab of a live fish")]
    public Fish fishPrefab;


    private PenguinAcademy penguinAcademy;
    private List<GameObject> fishList;


    /// <summary>
    /// Called when the game starts
    /// </summary>
    private void Start()
    {
        penguinAcademy = FindObjectOfType<PenguinAcademy>();
        ResetArea();
    }
    /// <summary>
    /// Reset the area, including fish and penguin placement
    /// </summary>
    public override void ResetArea()
    {
        RemoveAllFish();
        PlacePenguin();
        PlaceBaby();
        SpawnFish(4, penguinAcademy.FishSpeed);
    }

    /// <summary>
    /// Called Every Frame
    /// </summary>
    private void Update()
    {
        //Update the cumulative Reward Text
        cumulativeRewardText.text=penguinAgent.GetCumulativeReward().ToString("0.00");
    }

    /// <summary>
    /// Remove a specific fish from the area when it is eaten
    /// </summary>
    /// <param name="fishObject">The fish to remove</param>
    public void RemoveSpecificFish(GameObject fishObject)
    {
        fishList.Remove(fishObject);
        Destroy(fishObject);
    }

    /// <summary>
    /// The number of fish remaining
    /// </summary>
    /// <returns> Returns the number of fishes</returns>
    public int FishRemaining()
    {
          return  fishList.Count; 
    }
    
    /// <summary>
    /// Choose a random position on the X-Z plane within a partial donut shape
    /// </summary>
    /// <param name="center">The center of the donut</param>
    /// <param name="minAngle">Minimum angle of the wedge</param>
    /// <param name="maxAngle">Maximum angle of the wedge</param>
    /// <param name="minRadius">Minimum distance from the center</param>
    /// <param name="maxRadius">Maximum distance from the center</param>
    /// <returns>A position falling within the specified region</returns>
    public static Vector3 ChooseRandomPosition(Vector3 center,float minAngle,float maxAngle,float minRadius,float maxRadius)
    {
        float radius = minRadius;
        float angle = minAngle;

        if (maxRadius > minRadius)
        {
            //Pick a random radius
            radius = Random.Range(minRadius, maxRadius);
        }

        if (maxAngle > minAngle)
        {
            //Pick a random angle
            angle = Random.Range(minAngle, maxAngle);
        }

        //center+forward vector rotated around "angle" degrees,multiplied by radius
        return center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;

    }

    /// <summary>
    /// Remove all fishes from the area
    /// </summary>
    public void RemoveAllFish()
    {
        if (fishList != null)
        {
            for(int i = 0; i < fishList.Count; i++)
            {
                if (fishList[i] != null)
                {
                    Destroy(fishList[i]);
                }
            }
        }
        fishList = new List<GameObject>();
    }

    /// <summary>
    /// Place the penguin in Area
    /// </summary>
    private void PlacePenguin()
    {
        Rigidbody rigidbody = penguinAgent.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        penguinAgent.transform.position = ChooseRandomPosition(transform.position, 0f, 360f, 0f, 9f) + Vector3.up * 0.5f;
        penguinAgent.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
    }

    /// <summary>
    /// Place the baby in Area
    /// </summary>
    private void PlaceBaby()
    {
        Rigidbody rigidbody = penguinBaby.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        penguinBaby.transform.position = ChooseRandomPosition(transform.position, -45f, 45f, 4f, 9f) + Vector3.up * 0.5f;
        penguinBaby.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    /// <summary>
    /// spawn some number of fishes in the area and set their speed
    /// </summary>
    /// <param name="count">The number to spawn</param>
    /// <param name="fishSpeed">The swin speed</param>
    private void SpawnFish(int count,float fishSpeed)
    {
        //spawn and place the fish
        GameObject fishObject = Instantiate(fishPrefab.gameObject);
        fishObject.transform.position = ChooseRandomPosition(transform.position, 100f, 260f, 2f, 13f) + Vector3.up * 0.5f;
        fishObject.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        //Set the fish's transform to this area's transform
        fishObject.transform.SetParent(transform);

        //keep track of fish speed
        fishList.Add(fishObject);

        //Set fish speed
        fishObject.GetComponent<Fish>().fishSpeed = fishSpeed;

    }
}
