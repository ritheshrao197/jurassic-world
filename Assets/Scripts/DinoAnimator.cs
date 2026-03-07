using System.Collections;
using UnityEngine;

namespace DinosBattle.Animation
{
    // Add to every dinosaur prefab root.
    // Fill in trigger names to match your Animator Controller.
    public class DinoAnimator : MonoBehaviour
    {
        [Header("Animator")]
        public Animator animator;

        [Header("Trigger Names")]
        public string attackTrigger  = "Attack";
        public string hurtTrigger    = "Hurt";
        public string deathTrigger   = "Death";
        public string victoryTrigger = "Victory";

        [Header("Sounds")]
        public AudioClip attackSound;
        public AudioClip hurtSound;
        public AudioClip deathSound;

        [Header("Durations  (match your clip lengths)")]
        public float attackDuration = 0.6f;
        public float hurtDuration   = 0.3f;
        public float deathDuration  = 0.8f;

        private AudioSource _audio;
        private Vector3     _origin;
        private bool        _originCaptured;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
        }

        // Called by AttackCommand / AbilityCommand.
        // Origin is captured lazily on first call because the factory moves
        // the prefab to its spawn position after Awake() runs.
        public IEnumerator Play(AnimationType type, Transform target = null)
        {
            if (!_originCaptured) { _origin = transform.position; _originCaptured = true; }

            switch (type)
            {
                case AnimationType.Attack:
                case AnimationType.Ability:
                    Trigger(attackTrigger);
                    PlaySound(attackSound);
                    yield return Lunge(target);
                    break;

                case AnimationType.Hurt:
                    Trigger(hurtTrigger);
                    PlaySound(hurtSound);
                    yield return Shake();
                    break;

                case AnimationType.Death:
                    Trigger(deathTrigger);
                    PlaySound(deathSound);
                    yield return new WaitForSeconds(deathDuration);
                    break;

                case AnimationType.Victory:
                    Trigger(victoryTrigger);
                    yield return new WaitForSeconds(0.5f);
                    break;
            }
        }

        private void Trigger(string name)
        {
            if (animator != null && !string.IsNullOrEmpty(name))
                animator.SetTrigger(name);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null) _audio.PlayOneShot(clip);
        }

        // Lunges 1.2 units toward target then returns. Total time = attackDuration.
        private IEnumerator Lunge(Transform target)
        {
            Vector3 dir  = target != null ? (target.position - _origin).normalized : transform.forward;
            Vector3 dest = _origin + dir * 1.2f;

            float forward = attackDuration * 0.4f;
            float back    = Mathf.Max(0.05f, attackDuration - forward - 0.05f);

            yield return Move(_origin, dest, forward);
            yield return new WaitForSeconds(0.05f);
            yield return Move(dest, _origin, back);

            transform.position = _origin;
        }

        // Shakes horizontally, fading out over hurtDuration.
        private IEnumerator Shake()
        {
            for (float t = 0; t < hurtDuration; t += Time.deltaTime)
            {
                float x = Mathf.Sin(t * 80f) * 0.15f * (1f - t / hurtDuration);
                transform.position = _origin + new Vector3(x, 0f, 0f);
                yield return null;
            }
            transform.position = _origin;
        }

        private IEnumerator Move(Vector3 from, Vector3 to, float duration)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(from, to, t / duration);
                yield return null;
            }
        }
    }
}
