﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HowButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    Image image;
    Sprite spriteHover;
    Sprite spriteNormal;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        spriteHover = Resources.Load<Sprite>("HowToPlay_Over");
        spriteNormal = Resources.Load<Sprite>("HowToPlay_base");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = spriteHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = spriteNormal;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.parent.GetComponent<MenuScript>().SwitchScene("HowToPlay");
    }
}
