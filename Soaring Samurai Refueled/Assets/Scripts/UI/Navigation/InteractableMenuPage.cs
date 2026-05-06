using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableMenuPage : MonoBehaviour
{

    [SerializeField] GameObject mFirstItemToFocus = null;

    public GameObject FirstItemToFocus {  get { return mFirstItemToFocus; } }
}
