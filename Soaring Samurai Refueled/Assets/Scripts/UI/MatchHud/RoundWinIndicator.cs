using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundWinIndicator : MonoBehaviour
{
    [SerializeField] RoundInfoVisuals mVisualData;
    [SerializeField] GameObject ImageToSetIndicatorsOn;

    public PlayerCombatController OwningPlayer { set { mOwningPlayer = value; } }
    PlayerCombatController mOwningPlayer;

    public void Start()
    {
        LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerRoundWin += RecievePlayerWin;
        LevelScopeManagers.Instance.GetComponent<MatchStateManager>().OnInitMatch += ResetIndicator;
    }

    public void OnDestroy()
    {
        if (LevelScopeManagers.Instance != null)
        {
            LevelScopeManagers.Instance.GetComponent<MatchStateManager>().PlayerRoundWin -= RecievePlayerWin;
            LevelScopeManagers.Instance.GetComponent<MatchStateManager>().OnInitMatch -= ResetIndicator;
        }
    }

    public void SetRoundWinNumber(int numWins)
    {
        if (mVisualData.RoundWinSpriteVersions.Count == 0)
        {
            print("RoundWinIndicator: SetRoundWinNumber: RoundInfoVisuals scriptable object has no round win sprite versions set, need to add some for the indicator to change");
            return;
        }
        Sprite newSprite = null;

        if (mVisualData.RoundWinSpriteVersions.Count > numWins)
        {
            newSprite = mVisualData.RoundWinSpriteVersions[numWins];
        }
        else
        {
            newSprite = mVisualData.RoundWinSpriteVersions[mVisualData.RoundWinSpriteVersions.Count - 1];
        }

        ImageToSetIndicatorsOn.GetComponent<Image>().sprite = newSprite;
    }

    public void RecievePlayerWin(int winningPlayerID, int numCurrentRoundWins)
    {
        if (mOwningPlayer == null)
        {
            print("RoundWinIndicator:RecievePlayerWin: Owning player is null");
            return;
        }

        if (winningPlayerID == mOwningPlayer.PlayerIndex)
        {
            SetRoundWinNumber(numCurrentRoundWins);
        }
    }

    void ResetIndicator()
    {
        SetRoundWinNumber(0);
    }

}
