using IntrovertStudios.Messaging;
using TheFirstPerson;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        /*      ~~~~~       PLAYER        ~~~~~       */
        Container.Bind<PlayerController>()
            .FromComponentInHierarchy()
            .AsSingle();
        
        /*      ~~~~~       NPC        ~~~~~       */
        Container.Bind<NpcManager>()
            .FromNewComponentOnNewGameObject()
            .AsSingle()
            .NonLazy();

        Container.Bind<IMessageHub<NpcController.Message>>()
            .FromResolve();

        /*      ~~~~~       FACTORIES        ~~~~~       */
        Container.BindFactory<UnityEngine.Object, NpcController, NpcController.Factory>()
            .FromFactory<PrefabFactory<NpcController>>();

        Container.BindFactory<UnityEngine.Object, Projectile, Projectile.Factory>()
            .FromFactory<PrefabFactory<Projectile>>();
    }
}