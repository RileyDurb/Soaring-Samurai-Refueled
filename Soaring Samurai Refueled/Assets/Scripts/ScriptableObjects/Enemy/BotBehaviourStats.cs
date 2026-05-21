using MBT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotBehaviourStats", menuName = "Scripts/ScriptableObjects/Enemy/BotBehaviourStats")]

public class BotBehaviourStats : ScriptableObject
{
    [System.Serializable]
    public class StrafeBehaviourStats
    {
        public List<Vector2> OffsetDirectionOptions = new List<Vector2>{
        Vector2.up,
        Vector2.right,
        Vector2.down,
        Vector2.left
        };

        public float OffsetDistance = 5.0f;
        public float AngleOfOptions = 90.0f;
    }
    

    public StrafeBehaviourStats mStrafeStats = new StrafeBehaviourStats();

    [Header("Attack Stats")]
    public RandomBin AttackOptionsRandomBin = new RandomBin();


    public void InitializeVariablesOntoBehaviourTree(MonoBehaviourTree playerBotTree, Blackboard blackboard)
    {
        blackboard.GetVariable<RandomBinVariable>("AttackOptionsRandomBin").Value = AttackOptionsRandomBin;
    }
}
