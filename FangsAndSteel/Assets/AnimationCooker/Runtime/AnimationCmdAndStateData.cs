// This file holds definitions for AnimationStateData and AnimationCmdData and some enums
//--------------------------------------------------------------------------------------------------//

using Unity.Entities;

namespace AnimCooker
{
    public enum AnimationPlayMode : byte { 
        PlayForever, // the forever clip is playing (usually idle)
        PlayOnceAndStop, // a clip is currently being played and then it will stop
        PlayOnce, // a clip is currently playing once and then it will go back to the forever state
        Stopped // no clips are being played
    }

    // holds information about the current animation state for an entity
    public struct AnimationStateData : IComponentData
    {
        public byte ForeverClipIndex; // animation goes back to this after PlayOnce finishes
        public AnimationPlayMode Mode; // current animation play mode
        public byte ModelIndex; // holds the current model (so you can look up clip info in the database)
        public byte CurrentClipIndex; // holds the index of the current animation that is being played
        public byte LastPlayedClipIndex; // holds the clip index of the last played clip (gets set whenever a played clip finishes)
    }

    public enum AnimationCmd : byte {
        None, // once the command is seviced, it will be reset to None
        PlayOnce, // play the specified clip index once and then go back to the playForever index
        SetPlayForever, // set the specified clip as the clip that should play forever
        PlayOnceAndStop, // play the specified clip once and then stop all animation
        Stop // stop animation immediately
    }

    // Holds an animation command for an entity
    // Set cmd to one of the commands
    // Speed --> Applies to PlayOnce and PlayOnceAndStop only. If speed is negative, then no speed change is made, otherwise the speed will be applied when the command executes.
    // ClipIndex --> Applies to PlayOnce, SetPlayForever, and PlayOnceAndStop
    public struct AnimationCmdData : IComponentData
    {
        public AnimationCmd Cmd;
        public byte ClipIndex;
        public float Speed;
    }
} // namespace