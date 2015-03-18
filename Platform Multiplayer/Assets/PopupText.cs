using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PopupText : MonoBehaviour {

    public GameObject textPrefab;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void AddText(string text)
    {
        GameObject t = (GameObject)Instantiate(textPrefab);
        t.transform.SetParent(transform,true);
        t.GetComponent<Text>().text = text;
        Destroy(t, 3);
    }
}
