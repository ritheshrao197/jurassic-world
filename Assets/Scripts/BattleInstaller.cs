using UnityEngine;
using DinosBattle.Core.Interfaces;
using DinosBattle.Infrastructure.EventBus;
using DinosBattle.Infrastructure.ServiceLocator;
using DinosBattle.Input;
using DinosBattle.Systems.Combat;
using DinosBattle.Systems.Turn;
using DinosBattle.Systems.Animation;
using DinosBattle.Systems.Spawn;

namespace DinosBattle.Infrastructure
{
    [DefaultExecutionOrder(-200)]
    public class BattleInstaller : MonoBehaviour
    {
        [SerializeField] private UnityAnimationHandler animationHandler;
        [SerializeField] private PlayerInputHandler    playerInputHandler;
        [SerializeField] private TargetSelectMode      targetSelectMode = TargetSelectMode.Random;

        public BattleServiceLocator Locator { get; private set; }

        private void Awake()
        {
            Locator = new BattleServiceLocator();

            var bus = new BattleEventBus();
            Locator.Register<BattleEventBus>(bus);

            ITargetSelector selector = targetSelectMode == TargetSelectMode.LowestHp
                ? (ITargetSelector) new LowestHpTargetSelector()
                : new RandomTargetSelector();
            Locator.Register<ITargetSelector>(selector);

            Locator.Register<IDamageCalculator>(new StandardDamageCalculator());
            Locator.Register<ITurnOrderStrategy>(new SpeedBasedTurnOrder());
            Locator.Register<ITurnSystem>(new TurnSystem());
            Locator.Register<CombatResolver>(new CombatResolver(
                Locator.Get<IDamageCalculator>(), selector, bus));

            if (animationHandler == null)
                animationHandler = gameObject.AddComponent<UnityAnimationHandler>();
            Locator.Register<IAnimationHandler>(animationHandler);

            if (playerInputHandler == null)
                playerInputHandler = gameObject.AddComponent<PlayerInputHandler>();
            Locator.Register<PlayerInputHandler>(playerInputHandler);

            Locator.Register<CombatUnitFactory>(new CombatUnitFactory(animationHandler));

            BattleServices.SetActive(Locator);
        }

        private void OnDestroy()
        {
            BattleServices.Clear();
            Locator.Clear();
        }

        public enum TargetSelectMode { Random, LowestHp }
    }
}
