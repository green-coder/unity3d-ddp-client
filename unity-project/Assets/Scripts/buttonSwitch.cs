using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonSwitch : MonoBehaviour
{
    public GameObject connectButton;
    public GameObject disconnectButton;

    public void OnConnectClick(){
        //Debug.Log("Connecting ...");
		//ddpConnection.Connect();
        connectButton.SetActive(false);
        disconnectButton.SetActive(true);
    }

    public void OnDisconnectClick(){
        //Debug.Log("Connecting ...");
		//ddpConnection.Connect();
        connectButton.SetActive(true);
        disconnectButton.SetActive(false);
    }
}
