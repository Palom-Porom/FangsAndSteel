// If this system is enabled, it will animate the vertexes of any entity that has AnimationStateData, AnimationCmdData, and the time and speed properties.
// NOTE (TODO) - currently it only accounts for the per-instance speed value. If you set the _MatSpeed parameter in the shader, bad things will happen
// TODO - I need to figure out how to fetch _MatSpeed efficiently (unfortunately, it's in a shared mesh renderer and to access it would require putting the loop on the main thread)
//
// To change an animation at runtime, set the values in AnimationCmdData to get your desired animation.
//
// Note - This system requires an AnimDbRefData singleton in the scene.
//        Place an AnimationDbAuthoring component onto a gameobject in the subscene to create this singleton.
//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------//

using Unity.Entities; // for SystemBase
using Unity.Burst;

namespace AnimCooker
{
    
    [BurstCompile]
    public partial struct AnimationJob : IJobEntity
    {
        public float DeltaTime;
        public AnimDbRefData AnimDb;

        public void Execute(Entity entity, ref MaterialClipIndex clipIndexProp, ref MaterialCurrentTime curTimeProp, ref AnimationStateData state, ref AnimationCmdData cmd, ref MaterialAnimationSpeed speed)
        {
            if (cmd.Cmd == AnimationCmd.PlayOnce) {
                // we received a play once command, so change the clip and set the state mode.
                clipIndexProp.clipIndex = cmd.ClipIndex;
                curTimeProp.time = 0f;
                state.Mode = AnimationPlayMode.PlayOnce;
                cmd.Cmd = AnimationCmd.None; // indicates command has been processed
                if (cmd.Speed > 0f) { speed.multiplier = cmd.Speed; }
            } else if (cmd.Cmd == AnimationCmd.SetPlayForever) {
                // we received a command to change the play-forever clip, so change state to reflect it
                // and put it into effect if there isn't a play-once command currently executing.
                state.ForeverClipIndex = cmd.ClipIndex;
                if (state.Mode == AnimationPlayMode.PlayForever) {
                                                      clipIndexProp.clipIndex = state.ForeverClipIndex;
                    curTimeProp.time = 0f;
                }
                cmd.Cmd = AnimationCmd.None; // indicates command has been processed
            } else if (cmd.Cmd == AnimationCmd.PlayOnceAndStop) {
                // we recieved a play-once-and-stop command, so start a play-once-and-stop operation
                state.Mode = AnimationPlayMode.PlayOnceAndStop;
                curTimeProp.time = 0f;
                clipIndexProp.clipIndex = cmd.ClipIndex;
                cmd.Cmd = AnimationCmd.None; // reset (cmd processed)
                if (cmd.Speed > 0f) { speed.multiplier = cmd.Speed; }
            } else if (cmd.Cmd == AnimationCmd.Stop) {
                state.Mode = AnimationPlayMode.Stopped;
                cmd.Cmd = AnimationCmd.None; // reset (cmd processed)
            } else if (state.Mode != AnimationPlayMode.Stopped) {
                // logic here means that no command was sent,
                // so this is where we set the _CurTime property each frame
                AnimDbEntry clip = AnimDb.GetClip(state.ModelIndex, (int)clipIndexProp.clipIndex);
                // end time is (interval * frame count) / speed multipliers
                float endTime = clip.Interval * (clip.EndFrame - clip.BeginFrame + 1) / speed.multiplier;
                if ((curTimeProp.time + DeltaTime) >= endTime) { // if clip finished playing
                    if (state.Mode == AnimationPlayMode.PlayForever) {
                        curTimeProp.time = 0f; // reset to beginning
                    } else if (state.Mode == AnimationPlayMode.PlayOnce) {
                        // transition back to forever mode
                        curTimeProp.time = 0f; // show first frame of the forever clip
                        state.Mode = AnimationPlayMode.PlayForever;
                        clipIndexProp.clipIndex = state.ForeverClipIndex;
                        state.LastPlayedClipIndex = state.CurrentClipIndex;
                    } else if (state.Mode == AnimationPlayMode.PlayOnceAndStop) {
                        state.Mode = AnimationPlayMode.Stopped;
                        curTimeProp.time = endTime - clip.Interval;
                        state.LastPlayedClipIndex = state.CurrentClipIndex;
                    }
                }
                if (state.Mode != AnimationPlayMode.Stopped) {
                    curTimeProp.time += DeltaTime;
                }
            }
            state.CurrentClipIndex = (byte)clipIndexProp.clipIndex;
        }
    }

    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AnimationSystem : ISystem
    {
        void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AnimDbRefData>();
        }

        [BurstCompile]
        void OnUpdate(ref SystemState state)
        {
            AnimationJob job = new AnimationJob();
            job.AnimDb = SystemAPI.GetSingleton<AnimDbRefData>();
            job.DeltaTime = SystemAPI.Time.DeltaTime;
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
} // namespace