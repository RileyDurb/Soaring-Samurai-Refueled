using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundTimer : MonoBehaviour
{
    // Private variables
    MatchStateManager mMatchStateManagerRef;

    [SerializeField] TextMeshProUGUI mTimerText;
    // Start is called before the first frame update
    void Start()
    {
        mMatchStateManagerRef = LevelScopeManagers.Instance.GetComponent<MatchStateManager>();
    }

    // Update is called once per frame
    void Update()
    {
        int currRoundTime = mMatchStateManagerRef.CurrRoundTimeTrimmed;
        mTimerText.SetText(currRoundTime.ToString());
    }
}
