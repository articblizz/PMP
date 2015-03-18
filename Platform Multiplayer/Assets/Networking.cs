using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


public class Networking : MonoBehaviour {

    public GameObject Canvas;
    public GameObject nameCanvas;

    public string ip = "";
    public int Port = 8123;

    public GameObject player;
    //NetworkView view;
    NetworkView plyView;

    public GameObject popupCanvas;
    public string gameType = "0.1";
    public GameObject buttonPrefab;
    HostData[] hostdata;

    public PopupText popupScript;

    public InputField nameInput;

    List<GameObject> buttons = new List<GameObject>();
    public string ServerName = "PlatformMP 0.1";

    GameObject mainCam;

    // Use this for initialization
    void Start () {
        mainCam = Camera.main.gameObject;

        //view = GetComponent<NetworkView>();
        //Refresh();
    }

    // Update is called once per frame
    void Update () {
    
    }

    public void StartServer()
    {
        Network.InitializeServer(11, Port, !Network.HavePublicAddress());
        MasterServer.RegisterHost(gameType, ServerName);
        Canvas.SetActive(false);
        nameCanvas.SetActive(true);
        popupScript.AddText("Server initialized!");
        //SpawnPlayer();
    }

    void OnServerInitialized()
    {
    }

    void FixedUpdate()
    {
    }   

    public void Refresh()
    {
        buttons.Clear();
        MasterServer.RequestHostList(gameType);
        hostdata = MasterServer.PollHostList();

        //print(hostdata.Length);
        for (int i = 0; i < hostdata.Length; i++)
        {
            GameObject button = (GameObject)Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
            Button b = button.GetComponent<Button>();
            b.GetComponentInChildren<Text>().text = hostdata[i].gameName + " (" + hostdata[i].connectedPlayers + "/" + hostdata[i].playerLimit + ")";
            b.onClick.AddListener(() =>
            {
                Connect(hostdata[i-1]);
            });
            b.transform.SetParent(Canvas.transform);
            b.GetComponent<RectTransform>().anchoredPosition = new Vector2(80, i * -30);
            buttons.Add(button);
        }
    }

    void Connect(HostData hostData)
    {
        Network.Connect(hostData);
    }

    void OnPlayerConnected(NetworkPlayer player)
    {
        print("Player connected (" + player.ipAddress + ")");

        //popupCanvas.SendMessage("AddText", "Player connected!");
        plyView.RPC("Broadcast", RPCMode.All, player.ipAddress + " connected.");
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in playerList)
        {
            var plyScript = obj.GetComponent<PlayerScript>();
            if (plyScript != null)
            {
                plyScript.SendNickToPlayer(player);
            }
        }
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.DestroyPlayerObjects(player);
        plyView.RPC("Broadcast", RPCMode.All, player.ipAddress + " disconnected:");
    }

    void OnFailedToConnect()
    {
        popupScript.AddText("Failed to connect :(");
        print("rip");
    }

    GameObject myPly;

    public void SpawnPlayerAndDeleteOldOne()
    {
        Network.DestroyPlayerObjects(myPly.GetComponent<PlayerScript>().view.owner);
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        int spawnIndex = 0;
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        spawnIndex = Random.Range(0, spawnPoints.Length - 1);

        nameCanvas.SetActive(false);
        mainCam.SetActive(false);
        //Camera.main.gameObject.SetActive(false);
        GameObject ply = Network.Instantiate(player, spawnPoints[spawnIndex].transform.position, Quaternion.identity, 0) as GameObject;
        ply.GetComponentInChildren<Camera>().enabled = true;
        ply.GetComponentInChildren<AudioListener>().enabled = true;
        myPly = ply;
        var plyScript = ply.GetComponent<PlayerScript>();
        plyView = plyScript.view;
        plyScript.networkScript = this;

        plyScript.nickname.text = nameInput.text;
        plyScript.popupCanvas = popupCanvas;
        plyView.RPC("ChangeName", RPCMode.All, plyScript.nickname.text);

    }

    void OnConnectedToServer()
    {
        Canvas.SetActive(false);
        nameCanvas.SetActive(true);
        popupScript.AddText("Connected!");
        //SpawnPlayer();
        print(plyView.viewID);
    }

    void OnDisconnectedFromServer()
    {
        mainCam.SetActive(true);

        foreach (GameObject ply in GameObject.FindGameObjectsWithTag("Player"))
        {
            Network.Destroy(ply);
        }
        //Destroy(myPly);
        Canvas.SetActive(true);
        Refresh();
    }

}
