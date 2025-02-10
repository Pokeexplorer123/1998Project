using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCursor : MonoBehaviour
{
    public RectTransform rectTransform;
    public Image image;
    public bool isActive;
    //public static GameCursor instance;


    void Awake()
    {
        //instance = this;
        image.enabled = isActive;
    }

    void Update()
    {
    }

    public void SetActive(bool state)
    {
        //Debug.Log("Cursor set to " + state.ToString());
        isActive = state;
        image.enabled = isActive;
    }

    public void SetPosition(float x, float y)
    {
        this.rectTransform.anchoredPosition = new Vector2(x, y);
    }
}
