using System;
using CodeBase.Hero;
using UnityEngine;

public class HeroDeath : MonoBehaviour
{
    public static event Action DeathEvent;

    private HeroHealth _health;
    private Animator _animator;

    private bool _isDead;

    private void Awake()
    {
        _health = GetComponent<HeroHealth>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        _health.DeathEvent += OnDied;
    }

    private void OnDisable()
    {
        _health.DeathEvent -= OnDied;
    }

    private void OnDied()
    {
        _animator = GetComponentInChildren<Animator>();

        Debug.Log($"[HeroDeath] animator={( _animator ? _animator.name : "NULL")}, controller={( _animator && _animator.runtimeAnimatorController ? _animator.runtimeAnimatorController.name : "NULL")}");
        Debug.Log("[HeroDeath] PlayDeath");
        _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        _animator.SetBool("IsDead", true);

        Debug.Log($"[HeroDeath] IsDead param exists = {HasParam(_animator, "IsDead")}, IsDead now = {_animator.GetBool("IsDead")}");
    
        DeathEvent?.Invoke();
        
    }

    private bool HasParam(Animator a, string name)
    {
        foreach (var p in a.parameters)
            if (p.name == name) return true;
        return false;
    }



}