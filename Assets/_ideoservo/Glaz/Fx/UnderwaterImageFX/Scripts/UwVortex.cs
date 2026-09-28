using System;
using UnityEngine;

namespace GlazUnderwaterFX
{
    [ExecuteInEditMode]
    [AddComponentMenu("GlazUnderwaterFX/Vortex")]
    public class UwVortex : UwImageEffectBase
    {
        // Underwater param
        // Best result with a low value angle
        public float angle = 15.0f;

        public Vector2 radius = new Vector2(0.4F, 0.4F);
        public Vector2 center = new Vector2(0.5F, 0.5F);

        // Called by camera to apply image effect
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            UwImageEffects.RenderDistortion(material, source, destination, angle, center, radius);
        }
    }
}
