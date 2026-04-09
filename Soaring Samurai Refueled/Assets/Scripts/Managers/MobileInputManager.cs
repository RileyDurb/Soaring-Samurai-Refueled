using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

public class MobileInputManager : MonoBehaviour
{
    [SerializeField] GameObject MobileControllerPrefab;

    int mNumMobileControllersSpawned = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInputManager inputManager = LevelScopeManagers.Instance.GetComponent<PlayerInputManager>();

        if (inputManager.playerCount >= inputManager.maxPlayerCount || mNumMobileControllersSpawned >= 1)
        {
            return;
        }

        if (Input.touchCount > 0 || Input.GetKey(KeyCode.Equals))
        {
            GameObject newMobileControls = LevelScopeManagers.Instance.GetComponent<MenuManager>().PushControls(MobileControllerPrefab);

            GameObject joystick = newMobileControls.transform.Find("MobileJoystick").gameObject;
            OnScreenStick joystickComp = joystick.GetComponent<OnScreenStick>();

            //GameObject northButton = GameObject.Find("UpRightAttackButton");
            //OnScreenButton buttonCom = northButton.GetComponent<OnScreenButton>();

            inputManager.JoinPlayer(inputManager.playerCount, -1, "GamePad", joystickComp.control.device);

            mNumMobileControllersSpawned++;
        }
    }
}
