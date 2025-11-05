using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform destination;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = destination.position;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
