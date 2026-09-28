using System;
using UnityEngine;

namespace GlazUnderwaterFX
{
    [ExecuteInEditMode]
    [AddComponentMenu("GlazUnderwaterFX/Wave Animated")]
    public class UwWaveAnimated : UwImageEffectBase
    {
        // Underwater
        // Wave effect (see dedicated shader)

        public float WaveSize = 5.0f;
        public float Amplitude = 0.005f;
        public float TimeSpeed = 0.05f;
        private float TimePhase = 0.0f;

        // Called by camera to apply image effect
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            TimePhase += TimeSpeed;

            material.SetFloat("_WaveSize", WaveSize);
            material.SetFloat("_Amplitude", Amplitude);
            material.SetFloat("_TimePhase", TimePhase);

            Graphics.Blit(source, destination, material);
        }

    }
}
