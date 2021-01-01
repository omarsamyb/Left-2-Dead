using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI headerField;

    public TextMeshProUGUI contentField;

    public TextMeshProUGUI behaviourField;

    public LayoutElement layoutElement;

    public int charWrapLimit;

    public RectTransform rectTransform;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetText(string content, string header = "", string behaviour = ""){
        if(string.IsNullOrEmpty(header)){
            headerField.gameObject.SetActive(false);
        }
        else{
            headerField.gameObject.SetActive(true);
            headerField.text = header;
        }

        if(string.IsNullOrEmpty(behaviour)){
            behaviourField.gameObject.SetActive(false);
        }
        else{
            behaviourField.gameObject.SetActive(true);
            behaviourField.text = behaviour;
        }

        contentField.text = content;

        int headerLen = headerField.text.Length;
        int contentLen = contentField.text.Length;
        int behaviourLen = behaviourField.text.Length;

        layoutElement.enabled = (headerLen > charWrapLimit || contentLen > charWrapLimit || behaviourLen > charWrapLimit)? true : false;

    }

    private void Update()
    {
        

        Vector2 pos = Input.mousePosition;
        float pivotX = 0f;
        if(pos.x > Screen.width/2){
            pivotX = 1f;
        }
        float pivotY = 0f;
        if(pos.y > Screen.height/2){
            pivotY = 1f;
        }

        rectTransform.pivot = new Vector2(pivotX,pivotY);
        transform.position = pos;
    }
}
