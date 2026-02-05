using CodeBase.Enemy;
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAttack _attack;

    private void Awake()
    {
        _attack = GetComponentInParent<EnemyAttack>();
    }

    // викликається Animation Event
    public void OnAttack()
    {
   

        _attack.OnAttack();
    }

    // викликається Animation Event
    public void OnAttackEnded()
    {


        _attack.OnAttackEnded();
    }
}