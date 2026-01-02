using System;
using CodeBase.Hero;
using UnityEngine;

public class HeroDeath : MonoBehaviour
{
    public static event Action DeathEvent;

    private HeroHealth _health;

    private void Awake()
    {
        _health = GetComponent<HeroHealth>();
    }

    private void OnEnable()
    {
        _health.DeathEvent  += OnDied;
    }

    private void OnDisable()
    {
        _health.DeathEvent  -= OnDied;
    }

    private void OnDied()
    {
        
        DeathEvent?.Invoke();
        Destroy(transform.root.gameObject);
    }
}