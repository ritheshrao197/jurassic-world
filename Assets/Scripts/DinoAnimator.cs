using System.Collections;
using UnityEngine;
using DinosBattle.Core.Enums;

namespace DinosBattle.Systems.Animation
{
    /// <summary>
    /// Add this to every dinosaur prefab root.
    /// Fill in the trigger names from YOUR Animator Controller.
    /// Handles its own animations and sounds — no shared config needed.
    /// </summary>
    public class DinoAnimator : MonoBehaviour
    {
        [Header("Animator")]
        public Animator animator;

        [Header("Animator Trigger Names (match your controller exactly)")]
        public string attackTrigger  = "Attack";
        public string hurtTrigger    = "Hurt";
        public string deathTrigger   = "Death";
        public string victoryTrigger = "Victory";
        public string idleTrigger    = "Idle";

        [Header("Sounds")]
        public AudioClip attackSound;
        public AudioClip hurtSound;
        public AudioClip deathSound;

        [Header("Clip Durations (seconds — match your animation clip lengths)")]
        public float attackDuration = 0.6f;
        public float hurtDuration   = 0.3f;
        public float deathDuration  = 0.8f;

        private AudioSource _audio;

        // Captured when the battle actually starts, not in Start()
        // because the factory moves the prefab to its spawn position AFTER Start() runs.
        private Vector3 _startPos;
        private bool    _startPosCaptured;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            _audio = GetComponent<AudioSource>();
            if (_audio == null)
                _audio = gameObject.AddComponent<AudioSource>();

            _audio.playOnAwake = false;
        }

        // Called by UnityAnimationHandler
        public IEnumerator Play(AnimationType type, Transform target = null)
        {
            // Lazily capture spawn position on first play — by then the factory
            // has already moved the prefab to its correct world position.
            if (!_startPosCaptured)
            {
                _startPos         = transform.position;
                _startPosCaptured = true;
            }

            switch (type)
            {
                case AnimationType.Attack:
                    TriggerAnim(attackTrigger);
                    PlaySound(attackSound);
                    yield return Lunge(target);
                    break;

                case AnimationType.Hurt:
                    TriggerAnim(hurtTrigger);
                    PlaySound(hurtSound);
                    yield return Shake();
                    break;

                case AnimationType.Death:
                    TriggerAnim(deathTrigger);
                    PlaySound(deathSound);
                    yield return new WaitForSeconds(deathDuration);
                    break;

                case AnimationType.Victory:
                    TriggerAnim(victoryTrigger);
                    yield return new WaitForSeconds(0.5f);
                    break;
                case AnimationType.Idle:
                    TriggerAnim(idleTrigger);
                    break;

                    // No trigger — just a placeholder for idle time between actions.   
            }
        }

        private void TriggerAnim(string triggerName)
        {
            if (animator != null && !string.IsNullOrEmpty(triggerName))
                animator.SetTrigger(triggerName);
        }

        private void PlaySound(AudioClip clip)
        {
            if (_audio != null && clip != null)
                _audio.PlayOneShot(clip);
        }

        // Lunge toward target then return. Duration matches attackDuration
        // so the Animator clip and procedural motion finish together.
        private IEnumerator Lunge(Transform target)
        {
            Vector3 dir  = target != null
                ? (target.position - _startPos).normalized
                : transform.forward;

            Vector3 dest     = _startPos + dir * 1.2f;
            float   halfTime = attackDuration * 0.4f;   // 40% forward, 60% return

            // Lunge forward
            float t = 0f;
            while (t < halfTime)
            {
                transform.position = Vector3.Lerp(_startPos, dest, t / halfTime);
                t += Time.deltaTime;
                yield return null;
            }

            // Hold briefly at impact point
            yield return new WaitForSeconds(0.05f);

            // Return to start — use remaining clip time
            float returnTime = attackDuration - halfTime - 0.05f;
            if (returnTime < 0.05f) returnTime = 0.05f;

            t = 0f;
            while (t < returnTime)
            {
                transform.position = Vector3.Lerp(dest, _startPos, t / returnTime);
                t += Time.deltaTime;
                yield return null;
            }

            transform.position = _startPos;
        }

        private IEnumerator Shake()
        {
            float t = 0f;
            while (t < hurtDuration)
            {
                float fade = 1f - (t / hurtDuration);
                float x    = Mathf.Sin(t * 80f) * 0.15f * fade;
                transform.position = _startPos + new Vector3(x, 0f, 0f);
                t += Time.deltaTime;
                yield return null;
            }
            transform.position = _startPos;
        }
    }
}