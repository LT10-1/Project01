﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController _cc;
    public float MoveSpeed = 5f;
    private Vector3 _movementVelocity;
    private PlayerInput _playerInput;
    private float _verticalVelocity;
    public float Gravity = -9.8f;
    private Quaternion _targetRotation;
    private Animator _animator;

    public bool IsPlayer = true;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private Transform TargetPlayer;

    // Health
    private Health _health;

    private DamageCaster _damageCaster;


    // Player slides
    private float attackStartTime;
    public float AttackSlideDuration = 0.4f;
    public float AttackSlideSpeed = 0.06f;
    public enum CharacterState
    {
        Normal, Attacking
    }
    public CharacterState CurrentState;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _damageCaster = GetComponentInChildren<DamageCaster>();

        if (!IsPlayer)
        {
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            TargetPlayer = GameObject.FindWithTag("Player").transform;
            _navMeshAgent.speed = MoveSpeed;
        }
        else
        {
            _playerInput = GetComponent<PlayerInput> ();
        }
    }

    private void CalculatePlayerMovement()
    {
        

        if(_playerInput.MouseButtonDown && _cc.isGrounded)
        {
            SwitchStateTo(CharacterState.Attacking);
            return;
        }

        _movementVelocity.Set(_playerInput.HorizontalInput, 0f, _playerInput.VerticalInput);
        _movementVelocity.Normalize();
        _movementVelocity = Quaternion.Euler(0, -45f, 0) * _movementVelocity;
        _animator.SetFloat("Speed", _movementVelocity.magnitude);
        _movementVelocity *= MoveSpeed * Time.deltaTime;

        

        if (_movementVelocity != Vector3.zero)
        {
            _targetRotation = Quaternion.LookRotation(_movementVelocity);
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, Time.deltaTime * 10f);

            _animator.SetBool("AirBorne", !_cc.isGrounded);
        }
    }

    private void CalculateEnemyMovement()
    {
        if (CurrentState == CharacterState.Attacking)
        {
            _navMeshAgent.SetDestination(transform.position);
            _animator.SetFloat("Speed", 0f);
        }
        else if(Vector3.Distance(TargetPlayer.position, transform.position) >= _navMeshAgent.stoppingDistance)
        {
            _navMeshAgent.SetDestination(TargetPlayer.position);
            _animator.SetFloat("Speed", 0.2f);

        }
        else
        {
            _navMeshAgent.SetDestination(transform.position);
            _animator.SetFloat("Speed", 0f);
            SwitchStateTo(CharacterState.Attacking);
        }
    }

    private void FixedUpdate()
    {
        switch (CurrentState)
        {
            case CharacterState.Normal: 
                if(IsPlayer)
                    CalculatePlayerMovement();
                else CalculateEnemyMovement();
                break;
            case CharacterState.Attacking: 

                if(IsPlayer)
                    _movementVelocity = Vector3.zero;
                    if(Time.time < attackStartTime + AttackSlideDuration)
                {
                    float timePassed = Time.time - attackStartTime;
                    float lerpTime = timePassed / AttackSlideDuration;
                    _movementVelocity = Vector3.Lerp(transform.forward * AttackSlideSpeed, Vector3.zero, lerpTime);
                }

                break;
            
        }


        if(IsPlayer)
            CalculatePlayerMovement();
        else 
            CalculateEnemyMovement();

        if (IsPlayer)
        {
             if (_cc.isGrounded == false)
                        _verticalVelocity = Gravity;
                    else
                        _verticalVelocity = Gravity * 0.3f;
                    _movementVelocity += _verticalVelocity * Vector3.up * Time.deltaTime;


                    _cc.Move(_movementVelocity);
        }

       
    }

    private void SwitchStateTo(CharacterState newState)
    {
        if (IsPlayer)
        {
             //Clear Cache
                    _playerInput.MouseButtonDown = false;
        }
       

        // Exiting State
       switch (CurrentState) { 
            case CharacterState.Normal: break;
            case CharacterState.Attacking: break;    
        }


        // Entering State
        switch (newState)
        {
            case CharacterState.Normal: break;
            case CharacterState.Attacking:
                if(!IsPlayer)
                {
                    Quaternion newRotation = Quaternion.LookRotation(TargetPlayer.position - transform.position);
                    transform.rotation = newRotation;
                }
                _animator.SetTrigger("Attack");
                
                  if(IsPlayer)
                    attackStartTime = Time.time;

                break;
        }
        CurrentState = newState;    

        Debug.Log("Switch To "+ CurrentState);
    }

    public void AttackAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void ApplyDamage(int damage, Vector3 attackerPos = new Vector3())
    {
        if (_health != null)
        {
            _health.ApplyDamage(damage);
        }
    }
    public void EnableDamageCaster()
    {
        _damageCaster.EnableDamageCaster();
    }
    public void DisableDamageCaster()
    {
        _damageCaster.DisableDamageCaster();
    }
}
