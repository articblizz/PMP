using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour {


    public float Damage = 50;

	// Use this for initialization
	void Start () {

        Destroy(gameObject, 3);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter2D(Collision2D col)
    {
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        print(col.name + ", " + col.tag);
        if (col.tag == "Player" && Network.isServer)
        {
            var plyScript = col.transform.parent.GetComponent<PlayerScript>();
            if (plyScript != null)
            {
                print("Sendhit");

                if (plyScript.view.viewID.isMine)
                {
                    //print("LOL IM IMMORTAL NOOOOB");
                    plyScript.Hit(Damage);
                }
                else
                    plyScript.view.RPC("Hit", plyScript.view.owner, Damage);
            }
        }
        Destroy(gameObject);
    }
}
