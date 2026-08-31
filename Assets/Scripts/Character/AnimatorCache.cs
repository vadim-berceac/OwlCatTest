using System;
using UnityEngine;

public class AnimatorCache : IDisposable
{
    private readonly ICharacterInput _characterInput;
    private readonly Animator _animator;
    private readonly AnimationStates _animationStates;
    private readonly Character _character;

    public bool IsOnInteract  
        => _animator.GetCurrentAnimatorStateInfo(0).shortNameHash 
           == _animationStates.InteractStateHash;

    public AnimatorCache(ICharacterInput characterInput, Animator animator,
        AnimationStates animationStates, Character character)
    {
        _characterInput = characterInput;
        _animator = animator;
        _animationStates = animationStates;
        _character = character;

        _characterInput.OnMove += OnMove;
        _characterInput.OnRun += OnRun;
        _characterInput.OnInteract += OnInteract;
        _character.OnRotationDirection += OnTurn;
    }

    public void Dispose()
    {
        _characterInput.OnMove -= OnMove;
        _characterInput.OnRun -= OnRun;
        _characterInput.OnInteract -= OnInteract;
        _character.OnRotationDirection -= OnTurn;
    }

    private void OnMove(Vector2 input)
    {
        _animator.SetFloat(_animationStates.MotionXHash, input.x);
        _animator.SetFloat(_animationStates.MotionYHash, input.y);
    }

    private void OnRun(bool run)
    {
        var speed = run ? 1f : 0f;
        _animator.SetFloat(_animationStates.MotionSpeedHash, speed);
    }

    private void OnInteract()
    {
        _animator.SetTrigger(_animationStates.InteractHash);
    }

    private void OnTurn(float turn)
    {
        _animator.SetFloat(_animationStates.TurnHash, turn);
    }
}