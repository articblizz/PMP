using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


public class Networking : MonoBehaviour {

    public GameObject Canvas;
    public string ip = "";
    public int Port = 8123;

    public GameObject player;
    NetworkView view;

    public string gameType = "0.1";
    public GameObject buttonPrefab;
    HostData[] hostdata;

    List<GameObject> buttons = new List<GameObject>();

    int playerids = 0;

    GameObject mainCam;

	// Use this for initialization
	void Start () {
        mainCam = Camera.main.gameObject;
        view = GetComponent<NetworkView>();
        Refresh();
	}

	// Update is called once per frame
	void Update () {
	
	}

    public void StartServer()
    {
        Network.InitializeServer(4, Port, !Network.HavePublicAddress());
        MasterServer.RegisterHost(gameType, "Da server yo");
        Canvas.SetActive(false);
        SpawnPlayer();
        myPly.SendMessage("SetID", playerids);
        playerids++;
    }

    void OnServerInitialized()
    {
        print("Server initialized");
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
        view.RPC("TagPlayer", RPCMode.All, playerids);
        
        playerids++;
    }

    [RPC]
    void TagPlayer(int id)
    {
        myPly.SendMessage("SetID", id);
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
    }


    void OnConnectedToServer()
    {
        Canvas.SetActive(false);

        SpawnPlayer();
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
