using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour
{
    [SerializeField] private GameObject gameOverDisplayParent = null;
    [SerializeField] private TMP_Text winnerNameText = null;
    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandlerGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandlerGameOver;
    }

    public void LeaveGame()
    {
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            //stop server hosting
            NetworkManager.singleton.StopHost();
        }
        else
        {
            //stop client
            NetworkManager.singleton.StopClient();
        }
    }

    private void ClientHandlerGameOver(string winnerName)
    {
        winnerNameText.text = $"{winnerName} Has Won!";

        gameOverDisplayParent.SetActive(true);
    }
}
