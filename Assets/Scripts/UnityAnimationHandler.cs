using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DinosBattle.Core;
using DinosBattle.Core.Enums;
using DinosBattle.Core.Interfaces;

namespace DinosBattle.Systems.Animation
{
    /// <summary>
    /// Central IAnimationHandler — delegates ALL animation + sound work
    /// to the DinoAnimator component on each dinosaur's prefab.
    ///
    /// This class no longer owns Animator parameters or sound clips.
    /// It is purely a router: "unit X needs animation Y" → find that unit's
    /// DinoAnimator → call Play() → wait for it.
    ///
    /// Each dinosaur prefab has its own DinoAnimator with its own:
    ///   - Animator Controller parameter names
    ///   - Sound clips + volumes
    ///   - Procedural motion settings
    ///
    /// If a prefab has no DinoAnimator the unit is silently skipped (no crash).
    /// </summary>
    public class UnityAnimationHandler : MonoBehaviour, IAnimationHandler
    {
        public bool IsPlaying { get; private set; }

        // Map from UnitId → DinoAnimator found on that unit's instantiated prefab
        private readonly Dictionary<string, DinoAnimator> _dinoAnimators
            = new Dictionary<string, DinoAnimator>();

        // ── Registration — called by CombatUnitFactory after spawning ─────────

        /// <summary>
        /// Called once per unit after its model is instantiated.
        /// Finds the DinoAnimator on the prefab and stores it.
        /// </summary>
        public void RegisterUnit(CombatUnit unit)
        {
            if (unit?.ModelInstance == null) return;

            var dinoAnim = unit.ModelInstance.GetComponentInChildren<DinoAnimator>();

            if (dinoAnim == null)
            {
                Debug.LogWarning(
                    $"[AnimationHandler] {unit.Name}: no DinoAnimator found on prefab. " +
                    $"Animations will be skipped. Add DinoAnimator.cs to the prefab root.");
                return;
            }

            _dinoAnimators[unit.UnitId] = dinoAnim;
            Debug.Log($"[AnimationHandler] Registered DinoAnimator for {unit.Name}.");
        }

        // ── IAnimationHandler ─────────────────────────────────────────────────

        public IEnumerator PlayAnimation(AnimationType type, CombatUnit subject,
                                          CombatUnit target = null)
        {
            if (subject == null) yield break;

            if (!_dinoAnimators.TryGetValue(subject.UnitId, out var dinoAnim) || dinoAnim == null)
            {
                // No DinoAnimator — yield a tiny pause so the battle doesn't rush through
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            IsPlaying = true;

            Transform targetTransform = target?.ModelInstance != null
                ? target.ModelInstance.transform
                : null;

            yield return dinoAnim.Play(type, targetTransform);

            IsPlaying = false;
        }
    }
}