using UnityEngine.InputSystem;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container
            .Bind<InputActionAsset>()
            .FromScriptableObjectResource("Input/InputSystem_Actions")
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<Cursor>()
            .FromComponentInNewPrefabResource("Input/Cursor")
            .AsSingle()
            .NonLazy();
        
        Container
            .BindInterfacesAndSelfTo<PlayerInputHandler>()
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<AnimationStates>()
            .AsSingle()
            .NonLazy();
    }
}