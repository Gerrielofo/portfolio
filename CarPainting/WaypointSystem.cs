using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class WaypointSystem : MonoBehaviour
{
    private Transform target;
    [SerializeField] private int waypointindex = 0;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Animator door1;
    [SerializeField] private Animator door2;
    public bool end;
    [SerializeField] bool shopEntered;

    public float rotationDuration = 1f;  // The duration of the rotation

    private Quaternion startingRotation;  // The starting rotation of the object
    private Quaternion targetRotation;  // The target rotation of the object
    private float rotationTimer;  // A timer for the rotation

    void Start()
    {
        target = Waypoints.points[0];
        end = false;
        shopEntered = false;

        door1 = GameObject.Find("GarageDoor1").GetComponent<Animator>();
        door2 = GameObject.Find("GarageDoor2").GetComponent<Animator>();

        startingRotation = transform.rotation;
    }

    void Update()
    {   
        if(!end)
        {
            Vector3 dir = target.position - transform.position;
            transform.Translate(dir.normalized * movementSpeed * Time.deltaTime, Space.World);
        }

        if (Vector3.Distance(transform.position, target.position) <= 0.4f)
        {
            if (waypointindex == 0)
            {
                door1.SetTrigger("DoorMove");
            }
            else if (waypointindex == 3 && !shopEntered)
            {
                InShop();
            }
            else if (waypointindex == 4 && shopEntered)
            {
                door2.SetTrigger("DoorMove");
            }

            GetNextWaypoint();
            
        }
        Vector3 direction = target.position - transform.position;
        targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        rotationTimer += Time.deltaTime;
        transform.rotation = Quaternion.Slerp(startingRotation, targetRotation, rotationTimer / rotationDuration * rotationSpeed);

        if (rotationTimer >= rotationDuration)
        {
            rotationTimer = 0f;
            startingRotation = transform.rotation;
        }
    }

    void GetNextWaypoint()
    {
        if (waypointindex >= Waypoints.points.Length - 1)
        {
            ExitShop();
            end = true;
        }
        if (end)
            return;
        waypointindex++;
   
        target = Waypoints.points[waypointindex];
    }

    void InShop()
    {
        if(!shopEntered) 
        {
            door1.SetTrigger("DoorMove");
            Debug.Log("Car has arrived");
            shopEntered = true;
            end = true;
        }
        
    }

    void ExitShop()
    {
        if(!end)
        door2.SetTrigger("DoorMove");
    }

}
