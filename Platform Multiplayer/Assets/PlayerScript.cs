using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    float direction;
    public float Speed = 10;

    Quaternion correctRot;

    public GameObject bullet;
    public float BulletSpeed = 20;
    float lookingAtDirection = 1;
    public int playerID = 0;

    Vector3 correctPos;
    public NetworkView view;
    Rigidbody2D rigidBody;

    public Networking networkScript;

    bool isDead = false;

    public Transform WhatsGroundPos;

    public float JumpForce = 100;

    public float health = 100;

    public LayerMask whatIsGround;

    public float AttackSpeed = 2;
    float attackTimer = 2;


    float TimeBeingDeadBeforeRespawn = 3;
    float deadTimer;

    public Text nickname;

    public GameObject popupCanvas;

    void Awake()
    {
        view = GetComponent<NetworkView>();
    }

    // Use this for initialization
    void Start () {

        rigidBody = GetComponent<Rigidbody2D>();
        gameObject.name = "Player " + view.viewID;

        nickname = GetComponentInChildren<Text>();
    }

    [RPC]
    void Broadcast(string text)
    {
        if(view.isMine)
            popupCanvas.SendMessage("AddText", text);
    }

    public void SendNickToPlayer(NetworkPlayer player)
    {
        view.RPC("ChangeName", player, nickname.text);
    }

    [RPC]
    void ChangeName(string name)
    {
        if (view.isMine)
            return;
        nickname.text = name;
    }

    [RPC]
    public void Hit(float dmg)
    {
        if (!view.isMine)
            return;

        health -= dmg;
        if (health <= 0)
        {
            rigidBody.fixedAngle = false;
            print("Dead");
            isDead = true;
        }
    }

    bool isOnGround;
    void FixedUpdate()
    {
        if(view.isMine && !isDead)
            rigidBody.velocity = new Vector2(direction * Speed, rigidBody.velocity.y);

        isOnGround = Physics2D.OverlapCircle(WhatsGroundPos.position, 0.5f, whatIsGround);
    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            var pos = transform.position;
            stream.Serialize(ref pos);

            var rot = transform.rotation;
            stream.Serialize(ref rot);

            var d = direction;
            stream.Serialize(ref d);

        }
        else
        {
            var pos = new Vector3();
            stream.Serialize(ref pos);
            correctPos = pos;

            var rot = new Quaternion();
            stream.Serialize(ref rot);
            correctRot = rot;

            int d = 0;
            stream.Serialize(ref d);
            direction = d;
        }
    }

    void Update () {
        if (view.isMine)
        {
            if (isDead)
            {
                deadTimer += Time.deltaTime;
                if (deadTimer >= TimeBeingDeadBeforeRespawn)
                {
                    networkScript.SpawnPlayerAndDeleteOldOne();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.K))
                    Hit(100);
                if (Input.GetKey(KeyCode.A))
                    direction = -1;
                else if (Input.GetKey(KeyCode.D))
                    direction = 1;
                else
                    direction = 0;

                if (direction != 0)
                    lookingAtDirection = (int)direction;

                if (isOnGround && Input.GetKeyDown(KeyCode.W))
                {
                    rigidBody.AddForce(new Vector2(0, JumpForce));
                }

                if (Input.GetKey(KeyCode.Space) && attackTimer >= AttackSpeed)
                {
                    attackTimer = 0;
                    view.RPC("SpawnBullet", RPCMode.All, transform.position, lookingAtDirection);
                }
                else if (attackTimer < AttackSpeed)
                    attackTimer += Time.deltaTime;
            }
        }
        else if(!view.isMine)
        {
            transform.position = Vector2.Lerp(transform.position, correctPos, Time.deltaTime * 7);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctRot, Time.deltaTime * 7);
        }
    }

    [RPC]
    void SpawnBullet(Vector3 pos, float direction)
    {
        GameObject b = (GameObject)Instantiate(bullet, pos + new Vector3(direction * 0.8f,0), Quaternion.identity);
        b.GetComponent<Rigidbody2D>().velocity = new Vector2(BulletSpeed * direction, 0);
    }
}
