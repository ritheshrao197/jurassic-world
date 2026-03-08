using System.Collections;

namespace DinosBattle
{
    // ── Commands ──────────────────────────────────────────────────────────────

    public interface IBattleCommand { IEnumerator Execute(); }
}