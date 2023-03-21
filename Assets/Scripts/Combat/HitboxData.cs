using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "HitboxData", menuName = "IntoDarkness/HitboxData", order = 0)]
public class HitboxData : ScriptableObject {
    [SerializeField] float mightScale;
    public float MightScale{
        get => mightScale;
        private set => mightScale = value;
    }

    [SerializeField] float ingenuityScale;
    public float IngenuityScale{
        get => mightScale;
        private set => mightScale = value;
    }

    [SerializeField] int baseDamage;
    public int BaseDamage {
        get => baseDamage;
        private set => baseDamage = value;
    }

    [SerializeField] bool isHeal;
    public bool IsHeal {
        get => isHeal;
        private set => isHeal = value;
    }
}