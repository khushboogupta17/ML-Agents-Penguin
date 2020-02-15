
using UnityEngine;
using MLAgents;

public class PenguinAgent: Agent
{
    [Tooltip("How fast the agnet moves forward")]
    public float moveSpeed = 5f;

    [Tooltip("How fast the agent turns")]
    public float turnSpeed = 180f;

    [Tooltip("Prefab of heart that appears when the baby is fed")]
    public GameObject heartPrefab;

    [Tooltip("Prefab of the regurgitated fish that appears when the baby is fed")]
    public GameObject regurgitatedFishPrefab;


    private PenguinAcademy penguinAcademy;
    private PenguinArea penguinArea;
    new private Rigidbody rigidbody;
    private GameObject baby;

    private bool isFull; //if true, penguin has a full stomach
    private float feedRadius = 0f;


    /// <summary>
    /// Initial setup,called when agent is enabled
    /// </summary>
    public override void InitializeAgent()
    {
        base.InitializeAgent();
        penguinArea = GetComponentInParent<PenguinArea>();
        penguinAcademy = FindObjectOfType<PenguinAcademy>();
        baby = penguinArea.penguinBaby;
        rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Perform actions based on a vector of numbers
    /// </summary>
    /// <param name="vectorAction">The list of actions to take</param>
    public override void AgentAction(float[] vectorAction)
    {
        //Convert the first action to forward amount
        float forwardAmount = vectorAction[0];

        //convert the second action to turning left or right
        float turnAmount = 0f;

        if (vectorAction[1] == 1f)
        {
            turnAmount = -1f;
        }
        else if(vectorAction[1]==2f)
        {
            turnAmount = 1f;
        }

        //Apply movement
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * Time.fixedDeltaTime * moveSpeed);
        transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

        //Apply a tiny negative reward every step to encourage action
        AddReward(-1f / agentParameters.maxStep);
    }

    /// <summary>
    /// Read inputs from the keyboard and convert them to a list of actions.
    /// This is called only when the player wants to control the agent and has set
    /// Behavior Type to "Heuristic Only" in the Behavior Parameters inspector.
    /// </summary>
    /// <returns>A vectorAction array of floats that will be passed into <see cref="AgentAction(float[])"/></returns>

    public override float[] Heuristic()
    {
        float forwardAction = 0f;
        float turnAction = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            //move forward
            forwardAction = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            //turn left
            turnAction = 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            //turn right
            turnAction = 2f;
        }
        //Put the actions into an array and return
        return new float[] { forwardAction, turnAction };
    }

    /// <summary>
    /// Reset the agent and area
    /// </summary>
    public override void AgentReset()
    {
        isFull = false;
        penguinArea.ResetArea();
        feedRadius = penguinAcademy.FeedRadius;
    }

    /// <summary>
    /// Collect All non-Raycast Observations
    /// </summary>
    public override void CollectObservations()
    {
        //Whether the penguin has eaten a fish(1 float=1 value)
        AddVectorObs(isFull);

        //Distance to the baby (1 float=1 value)
        AddVectorObs(Vector3.Distance(baby.transform.position, transform.position));

        //Direction to baby(1 vector3 =3 values)
        AddVectorObs((baby.transform.position - transform.position).normalized);

        //Direction penguin is facing(1 vector3=3 values)
        AddVectorObs(transform.forward);

        //1 + 1 + 3 + 3=8 total values
    }

    private void FixedUpdate()
    {
        //Test if agent is close enough to baby
        if (Vector3.Distance(transform.position, baby.transform.position) < feedRadius)
        {
            //Close enough,try to feed the baby
            RegurgitateFish();
        }
    }

    /// <summary>
    /// When the agent collides with something else take action
    /// </summary>
    /// <param name="collision">The collision info</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("fish"))
        {
            //Try to eat the fish
            EatFish(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("baby"))
        {
            //Try to feed the baby
            RegurgitateFish();
        }
    }

    /// <summary>
    /// Check if agent is full,if not eat a fish and get a reward
    /// </summary>
    /// <param name="fishObject"></param>
    private void EatFish(GameObject fishObject)
    {
        if (isFull)//Can't eat another fish while full
            return;

        isFull = true;

        penguinArea.RemoveSpecificFish(fishObject);
        AddReward(1f);
    }


    /// <summary>
    /// Check if agent is full,if yes, feed the baby
    /// </summary>
    private void RegurgitateFish()
    {
        if (!isFull)
            return;//Nothing to regurgitate
        isFull = false;

        //Spawn regurgitated fish
        GameObject regurgitatedFish = Instantiate(regurgitatedFishPrefab);
        regurgitatedFish.transform.parent = transform.parent;
        regurgitatedFish.transform.position = baby.transform.position;
        Destroy(regurgitatedFish, 4f);

        //spawn heart
        GameObject heart = Instantiate(heartPrefab);
        heart.transform.parent = transform.parent;
        heart.transform.position = baby.transform.position+Vector3.up;
        Destroy(heart, 4f);

        AddReward(1f);

        if (penguinArea.FishRemaining() <= 0)
        {
            Done();
        }


    }



}
