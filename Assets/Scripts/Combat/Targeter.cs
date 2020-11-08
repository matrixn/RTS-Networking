using System;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    private Targetable target;

    public Targetable GetTarget()
    {
        return target;
    }

    public override void OnStartServer()
    {
        GameOverHandler.ClientOnGameOver += ServerHandleGameOver;
    }


    public override void OnStopServer()
    {
        GameOverHandler.ClientOnGameOver += ServerHandleGameOver;
    }

    #region  Server

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }

        this.target = newTarget;
    }

    [Server]
    public void ClearTarget()
    {
        this.target = null;
    }

    #endregion

    [Server]
    private void ServerHandleGameOver(string obj)
    {
        ClearTarget();
    }

}
