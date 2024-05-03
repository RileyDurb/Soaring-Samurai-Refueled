using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSpriteView : MonoBehaviour
{
    // Editor accessible variables
    public bool ShowInGame = false;


    // Start is called before the first frame update
    void Start()
    {
        // Sets visibility of each child to match the editor checkbox
        SpriteRenderer[] childRenderers = transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in childRenderers)
        {
            renderer.enabled = ShowInGame;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
