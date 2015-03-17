using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

    float direction;
    public float Speed = 10;

    public GameObject bullet;
    public float BulletSpeed = 20;
    float lookingAtDirection = 1;
    public static int playerID = 0;

    Vector3 correctPos;
    NetworkView view;
    Rigidbody2D rigidBody;



	// Use this for initialization
	void Start () {
        rigidBody = GetComponent<Rigidbody2D>();
        view = GetComponent<NetworkView>();

	}

    void FixedUpdate()
    {
        if(view.isMine)
            rigidBody.velocity = new Vector2(direction * Speed, rigidBody.velocity.y);

    }

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            var pos = transform.position;
            stream.Serialize(ref pos);

            var d = direction;
            stream.Serialize(ref d);
        }
        else
        {
            var pos = new Vector3();
            stream.Serialize(ref pos);
            correctPos = pos;

            int d = 0;
            stream.Serialize(ref d);
            direction = d;
        }
    }

    void SetID(int id)
    {
        playerID = id;
        gameObject.name = "Player " + id;
    }
	
	// Update is called once per frame
	void Update () {
        if (view.isMine)
        {
            //direction = Input.GetAxis("Horizontal");
            if (Input.GetKey(KeyCode.A))
                direction = -1;
            else if (Input.GetKey(KeyCode.D))
                direction = 1;
            else
                direction = 0;

            if (direction != 0)
                lookingAtDirection = (int)direction;
            print(lookingAtDirection);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                view.RPC("SpawnBullet", RPCMode.AllBuffered, transform.position, lookingAtDirection);
            }

        }
        else
        {
            transform.position = Vector2.Lerp(transform.position, correctPos, Time.deltaTime * 7);
        }
	}

    [RPC]
    void SpawnBullet(Vector3 pos, float direction)
    {
        GameObject b = (GameObject)Instantiate(bullet, pos + new Vector3(direction * 1.2f,0), Quaternion.identity);
        b.GetComponent<Rigidbody2D>().velocity = new Vector2(BulletSpeed * direction, 0);
    }
}
