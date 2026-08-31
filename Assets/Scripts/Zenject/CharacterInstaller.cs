using UnityEngine;
using Zenject;

public class CharacterInstaller : MonoInstaller
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform tr;
    [SerializeField] private Collider col;
    [SerializeField] private Character character;

    public override void InstallBindings()
    {
        Container
            .BindInstance(animator)
            .AsSingle();

        Container
            .BindInstance(character)
            .AsSingle();
        
        Container
            .BindInstance(character.CharacterType) 
            .AsSingle();

        Container
            .BindInstance(tr)
            .AsSingle();
        
        Container
            .BindInstance(col)
            .AsSingle();
        
        Container
            .BindInterfacesAndSelfTo<AnimatorCache>()
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<ICharacterInput>()
            .FromMethod(ZenjectHelper.CreateCharacterInput)
            .AsSingle();
    }
}