using MBT;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[AddComponentMenu("")]
[MBTNode("Services/UpdateRangeToAttack")]
public class Service_UpdateRangeToAttack : Service
{

    [SerializeField] GameObjectReference OpponentKey;
    [SerializeField] Vector2Reference AttackTargetDistanceVectorRef;

    [SerializeField] Vector2Reference MaxAttackRange = new Vector2Reference(new Vector2(4, 4)); // A good default safe value is 4 and 4
    [SerializeField] Vector2Reference MinAttackRange = new Vector2Reference(new Vector2(2, 3));
    [SerializeField] float OpponentSpeedForMaxRange = 12.0f;
    [SerializeField] float MaxIncomingOpponentAngle = 50.0f;

    PlayerCombatController mCombatController;

    public override void OnEnter()
    {
        mCombatController = behaviourTree.GetComponentInParent<PlayerCombatController>();

        base.OnEnter();
    }

    public override void Task()
    {
        // Potentially change distance we need to be from opponent before starting attack to be larger if opponent is coming towards us


        Vector2 currentOpponentVelocity = OpponentKey.Value.GetComponent<PhysicsApplier>().mUncappedDirectionalForces.GetVelocity();
        Vector2 vectorFromOpponentToUs = mCombatController.transform.position - OpponentKey.Value.transform.position;

        // Skip if opponent is nof moving, skip and use min attack range, which is the ideal attack range if our opponent doesn't require us to swing earlier
        if (currentOpponentVelocity.magnitude <= 0)
        {
            AttackTargetDistanceVectorRef.Value = MinAttackRange.Value;
            return;
        }

        // If angle of opponent's velocity is above max angle to be considered moving towards us, skip and use min attack range, which is the ideal attack range if our opponent doesn't require us to swing earlier
        float currOpponentToUsAngle = Vector2.Angle(currentOpponentVelocity.normalized, vectorFromOpponentToUs);
        if (currOpponentToUsAngle > MaxIncomingOpponentAngle)
        {
            AttackTargetDistanceVectorRef.Value = MinAttackRange.Value;
            return;
        }



        // Find how much of the max speed our opponent is going
        float percentFromMinToMaxRange = currentOpponentVelocity.magnitude / OpponentSpeedForMaxRange;
        percentFromMinToMaxRange = Mathf.Clamp(percentFromMinToMaxRange, 0.0f, 1.0f);

        // get a higher range the faster the opponent is moving, bounded by the min and max range. May not exactly be the perfect range except for the min and max ranges, because we can set those directly, but should be a decent estimate
        Vector2 currAttackRange = Vector2.Lerp(MinAttackRange.Value, MaxAttackRange.Value, percentFromMinToMaxRange);

        AttackTargetDistanceVectorRef.Value = currAttackRange;

        print("Curr Range before attack: " + AttackTargetDistanceVectorRef.Value);
        Debug.DrawLine(mCombatController.transform.position, mCombatController.transform.position + (vectorFromOpponentToUs.normalized * -currAttackRange.magnitude).ConvertTo<Vector3>(), Color.white, 10.0f);
    }
}