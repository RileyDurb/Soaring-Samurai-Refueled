using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSpriteView : MonoBehaviour
{
    // Editor accessible variables
    [SerializeField ]bool ShowInGame = false;

    public bool SetShowInGame {
        set 
        {
            ShowInGame = value;
            if (mLastShowInGame != ShowInGame)
            {
                SetShowVisibility();
            }
        }
    }

    bool mLastShowInGame = false;
    // Start is called before the first frame update
    void Start()
    {
        SetShowVisibility();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void SetShowVisibility()
    {
        // Sets visibility of each child to match the editor checkbox
        SpriteRenderer[] childRenderers = transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in childRenderers)
        {
            renderer.enabled = ShowInGame;
        }
        mLastShowInGame = ShowInGame;
    }
}

