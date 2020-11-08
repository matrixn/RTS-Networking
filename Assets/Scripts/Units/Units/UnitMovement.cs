using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 5f;

    #region Server
    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        //check if we have a target, if so chase it
        if (target != null)
        {
            //check if the difference between target position and our position is higher than the square(chaseRange)
            // if(Vector3.Distance()) {
            if((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange) {
                //chase
                agent.SetDestination(target.transform.position);
            } else if(agent.hasPath) {
                //stop chasing
                agent.ResetPath();
            }


            //if we are chasing the rest of the code is not necesarely
            return;
        }

        //normal movement part
        if (!agent.hasPath) { return; }
        if (agent.remainingDistance > agent.stoppingDistance) { return; }

        //reset and clear navmesh agent path
        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position)
    {
        //clear targets when moving
        targeter.ClearTarget();

        //check if clicked position for move is valid position on navmesh
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        //set destination for navmesh agent
        agent.SetDestination(hit.position);
    }

    
    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }

    #endregion


}
