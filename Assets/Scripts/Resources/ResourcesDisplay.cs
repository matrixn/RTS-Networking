using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{

    [SerializeField] private TMPro.TMP_Text resourcesText = null;
    private RTSPlayer player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (player == null)
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        
            if(player != null)
            {
                ClientHandleResourcesUpdated(player.GetResources());

                player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
            }
        }
    }

    private void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int newResources)
    {
        resourcesText.text = $"Money: {newResources}";
    }
}
