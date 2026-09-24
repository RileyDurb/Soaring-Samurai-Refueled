using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectListHeader : MonoBehaviour
{
    [SerializeField] TMP_Text mHeaderText;

    public void SetText(string text)
    {
        mHeaderText.text = text;
    }
}
