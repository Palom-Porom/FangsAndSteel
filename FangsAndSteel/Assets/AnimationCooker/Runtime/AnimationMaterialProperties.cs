// this class holds some "components" that represent material properties, which can be used
// to change per-instance properties of the playback shader.
//--------------------------------------------------------------------------------------------------//

using Unity.Entities;
using Unity.Rendering;

namespace AnimCooker
{
    [MaterialProperty("_ClipIdx", -1)]
    public struct MaterialClipIndex : IComponentData
    {
        public float clipIndex;
    }

    [MaterialProperty("_CurTime", -1)]
    public struct MaterialCurrentTime : IComponentData
    {
        public float time; // time in seconds relative to the beginning of the current clip
    }

    [MaterialProperty("_SpeedInst", -1)]
    public struct MaterialAnimationSpeed : IComponentData
    {
        public float multiplier;
    }
} // namespace