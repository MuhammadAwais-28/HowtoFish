using System;
using UnityEngine;

namespace GlazUnderwaterFX
{
    [ExecuteInEditMode]
    [AddComponentMenu("GlazUnderwaterFX/Vortex Animated")]
    public class UwVortexAnimated : UwImageEffectBase
    {
        // Underwater params
        // Inspired from Vortex Image effect with time variation

        public Vector2 radius = new Vector2(0.4F, 0.4F);
        public float angle = 15;
        public Vector2 center = new Vector2(0.5F, 0.5F);

        public float _timeSpeed = 0.005f;
        private float _time = 0.0f;
        private float angleLerp;
        private bool posway = false;

        // Called by camera to apply image effect
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            _time += _timeSpeed;
            if(_time > 1.0f)
            {
                posway = !posway;
                _time = 0.0f;
            }
                
            if(posway)
                angleLerp = Mathf.Lerp(-angle, angle, _time);
            else
                angleLerp = Mathf.Lerp(angle, -angle, _time);

            UwImageEffects.RenderDistortion(material, source, destination, angleLerp, center, radius);
        }
    }
}
