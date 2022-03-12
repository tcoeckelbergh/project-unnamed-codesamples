using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilitiesHolder : MonoBehaviour
{
    public Ability ability;
    float cooldownTime;
    float activeTime;

    enum AbilityState
    {
        ready,
        active,
        cooldown
    }

    AbilityState state = AbilityState.ready;
    public KeyCode key;
    
    public event EventHandler OnAbilityReady;
    public event EventHandler OnAbilityActive;
    public event EventHandler<OnAbilityOnCooldownEventArgs> OnAbilityOnCooldown;
    public class OnAbilityOnCooldownEventArgs : EventArgs
    {
        public float cooldownTimer;
    }

    private void Start()
    {
        if (ability) ability.Initialize(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (!ability)
            return; 

        switch (state)
        {
            case AbilityState.ready:
                if (Input.GetKeyDown(key))
                {
                    ability.Activate(this.gameObject);
                    SwapState(AbilityState.active);
                    activeTime = ability.activeTime;
                }
                break;
            case AbilityState.active:
               
                // If the ability is a click and hold
                if (activeTime == -1)
                {
                    if (Input.GetKey(key))
                    {
                        ability.UpdateAbility(gameObject);
                    }
                    else
                    {
                        activeTime = 0;
                    }
                }
                else if (activeTime > 0)
                {
                    activeTime -= Time.deltaTime;
                }    
                else
                {
                    cooldownTime = ability.cooldownTime;
                    SwapState(AbilityState.cooldown);
                }
                break;

            case AbilityState.cooldown:
                ability.Deactivate(gameObject);

                if (cooldownTime > 0)
                    cooldownTime -= Time.deltaTime;
                else
                    SwapState(AbilityState.ready);
                break;
        }
    }

    private void SwapState(AbilityState newState)
    {
        // Swap the state and invoke the appropriate event
        state = newState;
        switch (state)
        {
            case AbilityState.ready:
                OnAbilityReady?.Invoke(this, EventArgs.Empty);
                break;
            case AbilityState.active:
                OnAbilityActive?.Invoke(this, EventArgs.Empty);
                break;
            case AbilityState.cooldown:
                OnAbilityOnCooldown?.Invoke(this, new OnAbilityOnCooldownEventArgs { cooldownTimer = cooldownTime });
                break;
        }
    }
}
