using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


public class Networking : MonoBehaviour {

    public GameObject Canvas;
    public string ip = "";
    public int Port = 8123;

    public GameObject player;
    //NetworkView view;
    NetworkView plyView;

    public string gameType = "0.1";
    public GameObject buttonPrefab;
    HostData[] hostdata;

    List<GameObject> buttons = new List<GameObject>();

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
        Network.InitializeServer(4, Port, !Network.HavePublicAddress());
        MasterServer.RegisterHost(gameType, "Platform MP 0.1 Server");
        Canvas.SetActive(false);
        SpawnPlayer();
        //myPly.name = Network.player.ToString();
        //myPly.SendMessage("SetID", playerids);
        //playerids++;
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

        print(hostdata.Length);
        for (int i = 0; i < hostdata.Length; i++)
        {
            GameObject button = (GameObject)Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
            Button b = button.GetComponent<Button>();
            b.GetComponentInChildren<Text>().text = hostdata[i].gameName;
            b.onClick.AddListener(() =>
            {
                Connect(hostdata[i-1]);

            });
            b.transform.SetParent(Canvas.transform);
            b.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (i * -30) - 50);
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
        //print(player.ToString());
        //plyView.RPC("SetNetPly", player, player);
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {

    }

    void OnFailedToConnect()
    {
        print("rip");
    }

    GameObject myPly;

    void SpawnPlayer()
    {
        Camera.main.gameObject.SetActive(false);
        GameObject ply = Network.Instantiate(player, Vector3.zero, Quaternion.identity, 0) as GameObject;
        ply.GetComponentInChildren<Camera>().enabled = true;
        ply.GetComponentInChildren<AudioListener>().enabled = true;
        myPly = ply;
        plyView = myPly.GetComponent<PlayerScript>().view;
    }

    void OnConnectedToServer()
    {
        Canvas.SetActive(false);

        SpawnPlayer();
        print(plyView.viewID);
    }

    void OnDisconnectedFromServer()
    {
        mainCam.SetActive(true);

        foreach (GameObject ply in GameObject.FindGameObjectsWithTag("Player"))
        {
            Destroy(ply);
        }
        //Destroy(myPly);
        Canvas.SetActive(true);
        Refresh();
    }

}
