using IntrovertStudios.Messaging;
using TheFirstPerson;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public UIManager UIManagerPrefab;
    public GameManager gameManagerPrefab;

    public override void InstallBindings()
    {
        /*      ~~~~~       MANAGERS        ~~~~~       */
        Container.Bind<GameManager>()
            .FromComponentInNewPrefab(gameManagerPrefab)
            .AsSingle()
            .NonLazy();

        Container.Bind<UIManager>()
            .FromComponentInNewPrefab(UIManagerPrefab)
            .AsSingle()
            .NonLazy();

        Container.Bind<GameEventManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();

        Container.Bind<ItemManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();
    }
}