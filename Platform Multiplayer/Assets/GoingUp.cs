using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GoingUp : MonoBehaviour {


    public float Speed = 0.5f;
    RectTransform pos;
    Text text;
	// Use this for initialization
	void Start () {

        text = GetComponent<Text>();
        pos = GetComponent<RectTransform>();

        pos.anchoredPosition = new Vector2(364, 86);
	}
	
	// Update is called once per frame
	void Update () {

        var y = pos.anchoredPosition.y;
        y += Speed;
        pos.anchoredPosition = new Vector2(pos.anchoredPosition.x, y);
        var c = text.color;
        c.a -= 0.01f;
        text.color = c;


	}
}
