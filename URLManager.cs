using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class URLManager : MonoBehaviour, IPointerClickHandler
{
    TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int index = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
            if (index > -1)
            {
                Application.OpenURL(text.textInfo.linkInfo[index].GetLinkID());
            }
        }
    }
}