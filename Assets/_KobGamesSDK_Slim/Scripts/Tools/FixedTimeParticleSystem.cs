using UnityEngine;

namespace KobGamesSDKSlim
{
    public class FixedTimeParticleSystem : MonoBehaviour
    {
        public ParticleSystem ps;
        private bool playing = false;

        public void Play()
        {
            ps.Simulate(0.0f, true, true);
            playing = true;
        }

        public void Stop()
        {
            playing = false;
        }

        public void FixedUpdate()
        {
            if (playing)
                ps.Simulate(Time.fixedDeltaTime, true, false);
        }
    }
}
