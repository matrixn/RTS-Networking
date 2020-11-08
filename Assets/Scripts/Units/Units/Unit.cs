using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] private int resourceCost = 10;
    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnityEvent onSelect = null;
    [SerializeField] private UnityEvent onDeSelect = null;

    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDespawned;
    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDespawned;


    public int GetResourceCost()
    {
        return resourceCost;
    }

    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }

    #region Server
    [Server]
    private void ServerhandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    public override void OnStartServer()
    {
        // base.OnStartServer();
        ServerOnUnitSpawned?.Invoke(this);

        health.ServerOnDie += ServerhandleDie;
    }

    public override void OnStopServer()
    {
        // base.OnStopServer();
        ServerOnUnitDespawned?.Invoke(this);

        health.ServerOnDie -= ServerhandleDie;
    }
    #endregion

    #region Client

    // public override void OnStartClient()
    // {
    //     if (!isClientOnly || !hasAuthority) { return; }
    //     AuthorityOnUnitSpawned?.Invoke(this);
    // }

    public override void OnStartAuthority()
    {
        AuthorityOnUnitSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        // if (!isClientOnly || !hasAuthority) { return; }
        if (!hasAuthority) { return; }
        AuthorityOnUnitDespawned?.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if (!hasAuthority) { return; }
        onSelect?.Invoke();
    }

    [Client]
    public void DeSelect()
    {
        if (!hasAuthority) { return; }
        onDeSelect?.Invoke();
    }
    #endregion
}
