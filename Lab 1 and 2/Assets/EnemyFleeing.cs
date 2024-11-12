using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFleeing : MonoBehaviour
{
    // Definirea stărilor AI
    enum AIState
    {
        Idle, Patrolling, Fleeing
    }

    [Header("Patrol")]
    [SerializeField] private Transform wayPoints; // Obiect care conține punctele de patrulare
    [SerializeField] private float waitAtPoint = 2f; // Timp de așteptare la punctele de patrulare
    private int currentWaypoint;
    private float waitCounter;

    [Header("Components")]
    NavMeshAgent agent;

    [Header("AI States")]
    [SerializeField] private AIState currentState = AIState.Idle;

    [Header("Detection")]
    [SerializeField] private float viewRange = 10f; // Raza de vedere a inamicului
    [SerializeField] private float fleeDistance = 15f; // Distanța de fugă
    private GameObject player;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>(); // Referință la componenta NavMeshAgent
        player = GameObject.FindGameObjectWithTag("Player"); // Găsește jucătorul după tag-ul "Player"

        waitCounter = waitAtPoint; // Inițializează timer-ul de așteptare
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Comportament în funcție de starea curentă
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState(distanceToPlayer);
                break;

            case AIState.Patrolling:
                HandlePatrollingState(distanceToPlayer);
                break;

            case AIState.Fleeing:
                HandleFleeingState(distanceToPlayer);
                break;
        }
    }

    // Gestionare stare de Idle
    private void HandleIdleState(float distanceToPlayer)
    {
        if (waitCounter > 0)
        {
            waitCounter -= Time.deltaTime;
        }
        else
        {
            currentState = AIState.Patrolling; // Trecere la starea de patrulare
            agent.SetDestination(wayPoints.GetChild(currentWaypoint).position); // Se mută la următorul punct
        }

        // Dacă jucătorul este în raza de vedere, fugi
        if (distanceToPlayer <= viewRange)
        {
            currentState = AIState.Fleeing;
        }
    }

    // Gestionare stare de patrulare
    private void HandlePatrollingState(float distanceToPlayer)
    {
        if (agent.remainingDistance <= 0.2f) // Dacă a ajuns la punctul de patrulare
        {
            currentWaypoint++; // Treci la următorul punct
            if (currentWaypoint >= wayPoints.childCount) // Dacă toate punctele sunt parcurse, reia de la început
            {
                currentWaypoint = 0;
            }
            currentState = AIState.Idle; // Revino la starea de așteptare
            waitCounter = waitAtPoint;
        }

        // Dacă jucătorul este în raza de vedere, fugi
        if (distanceToPlayer <= viewRange)
        {
            currentState = AIState.Fleeing;
        }
    }

    // Gestionare stare de fugă
    private void HandleFleeingState(float distanceToPlayer)
    {
        // Fugi de jucător
        Vector3 directionAwayFromPlayer = (transform.position - player.transform.position).normalized;
        Vector3 fleePosition = transform.position + directionAwayFromPlayer * fleeDistance;

        // Mută inamicul la poziția de fugă calculată
        agent.SetDestination(fleePosition);

        // Dacă jucătorul este în afara razei de vedere, revino la Idle
        if (distanceToPlayer > viewRange)
        {
            currentState = AIState.Idle;
        }
    }
}
