using UnityEngine;
using DinosBattle.Combat;
using DinosBattle.Battle;
using DinosBattle.Input;

namespace DinosBattle
{
    // Registers all battle services before any other script runs.
    // Attach to a persistent GameObject in the BattleScene.
    [DefaultExecutionOrder(-200)]
    public class BattleInstaller : MonoBehaviour
    {
        [SerializeField] private TargetMode targetMode = TargetMode.Random;

        private void Awake()
        {
            var bus      = new EventBus();
            var selector = targetMode == TargetMode.LowestHp
                ? (ITargetSelector) new LowestHpTargetSelector()
                : new RandomTargetSelector();

            ServiceLocator.Register(bus);
            ServiceLocator.Register(selector);
            ServiceLocator.Register<IDamageCalculator>(new StandardDamageCalculator());
            ServiceLocator.Register(new CombatResolver(
            ServiceLocator.Get<IDamageCalculator>(), selector, bus));
            ServiceLocator.Register(new TurnSystem());
            ServiceLocator.Register(new UnitFactory());
            ServiceLocator.Register(GetOrAdd<PlayerInputHandler>());
        }

        private void OnDestroy() => ServiceLocator.Clear();

        private T GetOrAdd<T>() where T : Component =>
            GetComponent<T>() ?? gameObject.AddComponent<T>();

        public enum TargetMode { Random, LowestHp }
    }
}
