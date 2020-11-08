using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform = null;
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildingsToSpawnOnServer = new Building[0];
    [SerializeField] private float buildingRangeLimit = 5f;

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 500;
    public event Action<int> ClientOnResourcesUpdated;
    private Color teamColor = new Color();
    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuildings = new List<Building>();

   public Transform GetCameraTransform()
    {
        return cameraTransform;
    }


    public Color GetTeamColor()
    {
        return teamColor;
    }

    public List<Unit> GetMyUnits()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuildings;
    }

    public int GetResources()
    {
        return resources;
    }

    private void ClientHandleResourcesUpdated(int oldResource, int newResource)
    {
        ClientOnResourcesUpdated?.Invoke(newResource);
    }

    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 pointToPlace)
    {
        //check if building is not overlapping other buildings and can be placed
        if (Physics.CheckBox(pointToPlace + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity, buildingBlockLayer))
        {
            return false;
        }

        //make sure is respecting building range rule
        foreach (Building building in myBuildings)
        {
            if ((pointToPlace - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }

        return false;
    }

    #region Server

    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
    }

    [Server]
    public void SetTeamColor(Color newTeamColor)
    {
        teamColor = newTeamColor;
    }

    [Server]
    public void SetResources(int newResources)
    {
        this.resources = newResources;
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 positionPoint)
    {
        Building buildingToPlace = null;

        foreach (Building building in buildingsToSpawnOnServer)
        {
            if (building.GetId() == buildingId)
            {
                buildingToPlace = building;
                //no need to find in rest of the array if we found the building (break foreach)
                break;
            }
        }

        if (buildingToPlace == null) { return; }

        //check if you have enough resources to purchase building
        if (resources < buildingToPlace.GetPrice()) { return; }


        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();
        if (!CanPlaceBuilding(buildingCollider, positionPoint)) { return; }


        //place building in client and server
        GameObject buildingInstance = Instantiate(buildingToPlace, positionPoint, buildingToPlace.transform.rotation).gameObject;
        NetworkServer.Spawn(buildingInstance, connectionToClient);

        //deduct the money from player
        SetResources(resources - buildingToPlace.GetPrice());

    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Add(building);
    }

    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuildings.Remove(building);
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Add(unit);

    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);
    }

    #endregion

    #region Client
    public override void OnStartAuthority()
    {
        if (NetworkServer.active) { return; }
        Unit.AuthorityOnUnitSpawned += ClientHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += ClientHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned += ClientHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += ClientHandleBuildingDespawned;

    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) { return; }
        Unit.AuthorityOnUnitSpawned -= ClientHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= ClientHandleUnitDespawned;
        Building.AuthorityOnBuildingSpawned -= ClientHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= ClientHandleBuildingDespawned;
    }

    private void ClientHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }

    private void ClientHandleUnitDespawned(Unit unit)
    {
        myUnits.Remove(unit);
    }

    private void ClientHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }

    private void ClientHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }

    #endregion
}
