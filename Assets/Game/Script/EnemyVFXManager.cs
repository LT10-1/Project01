using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class EnemyVFXManager : MonoBehaviour
{
    public VisualEffect FootStep;
    public VisualEffect AttackVFX;
    public void PlayAttackVFX()
    {
        AttackVFX.SendEvent("OnPlay");
    }

    public void BrustFootStep()
    {
        FootStep.SendEvent("OnPlay");
    }
}
