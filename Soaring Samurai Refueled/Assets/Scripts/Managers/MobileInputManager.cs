using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class MobileInputManager : MonoBehaviour
{
    [SerializeField] GameObject MobileControllerPrefab;

    List<GameObject> mSpawnedControlsObjects = new List<GameObject>();

    public Dictionary<int, GameObject> mSpawnedControlsPerPlayer = new Dictionary<int, GameObject>();

    int mNumMobileControllersSpawned = 0;
    // Start is called before the first frame update
    void Start()
    {
        PlayerInputManager.instance.playerJoinedEvent.AddListener(RejectExtraMobileControllers);
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
            GameObject newMobileControls = LevelScopeManagers.Instance.GetComponent<HUDManager>().AddControlsItem(MobileControllerPrefab);
            mSpawnedControlsObjects.Add(newMobileControls);
            int playerIndex = inputManager.playerCount;
            mSpawnedControlsPerPlayer.Add(playerIndex, newMobileControls);


            GameObject joystick = newMobileControls.transform.Find("MobileJoystick").gameObject;
            OnScreenStick joystickComp = joystick.GetComponent<OnScreenStick>();

            //InputDeviceDescription newMobileDeviceDescription = new InputDeviceDescription();
            //joystickComp.control.device.displayName = "Test";
            //newMobileDeviceDescription.version = "Virtual";
            //newMobileDeviceDescription.deviceClass = "Gamepad";

            //GameObject northButton = GameObject.Find("UpRightAttackButton");
            //OnScreenButton buttonCom = northButton.GetComponent<OnScreenButton>();

            inputManager.JoinPlayer(playerIndex, -1, "MobileVirtualGamePad", joystickComp.control.device);

            mNumMobileControllersSpawned++;

        }

    }

    void RejectExtraMobileControllers(PlayerInput newPlayer)
    {
        if (newPlayer.devices[0].description.product == "Virtual")
        {
            Destroy(newPlayer);
        }
    }

    //void CheckForNewPlayerAdd(System.IObservable<InputControl> addNewPlayer)
    //{
    //    PlayerInputManager inputMan = PlayerInputManager.instance;
    //    if (PlayerInputManager.instance.playerCount >= PlayerInputManager.instance.maxPlayerCount)
    //    {
    //        return;
    //    }

    //    if (addNewPlayer.device)

    //        inputMan.JoinPlayer(inputMan.playerCount, -1, "", addNewPlayer.device);
    //}
}
