using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject MainMenuObjectReference;

    // Start is called before the first frame update
    void Start()
    {
        LevelScopeManagers.Instance.GetComponent<MenuManager>().PushPauseMenu(MainMenuObjectReference);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
