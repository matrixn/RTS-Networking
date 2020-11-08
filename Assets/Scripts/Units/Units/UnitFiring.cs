using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float rotationSpeed = 20f;

    private float lastFiredTime;

    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if(target == null) { return; }

        if(!CanFireAtTarget()) { return; }

        //calculate rotation towards target
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //calculating fire rate
        if(Time.time > (1/fireRate) + lastFiredTime) {

            //calculate projectile rotation
            Quaternion projectileRotation =  Quaternion.LookRotation(target.GetAimAtPoint().transform.position - projectileSpawnPoint.position);

            //we can now shoot
            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);

            //spawn the projectile on server and give it's proper permission by connectioToClient
            NetworkServer.Spawn(projectileInstance, connectionToClient);

            //set time when we fired last
            lastFiredTime = Time.time;
        }
    }

    [Server]
    private bool CanFireAtTarget() {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <= fireRange * fireRange;
    }

}
