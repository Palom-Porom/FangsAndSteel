// Place this on an empty object somewhere in a SUBscene to bake it into the subscene
// which will allow you to access the database from ISystem and jobs without touching managed code.
// If there is only one in the scene (recommended), you can access it as a singleton.
//
// There are several convenience functions within the singleton.
//
// Example of use:
//    [BurstCompile]
//    public partial struct MySystem : ISystem
//    {
//       void OnCreate(ref SystemState state)
//       {
//          state.RequireForUpdate<AnimDbRefData>();
//       }
//       [BurstCompile]
//       void OnUpdate(ref SystemState state)
//       {
//          AnimDbRefData db = SystemAPI.GetSingleton<AnimDbRefData>();
//          UnityEngine.Debug.Log($"There are {db.GetModelCount()} models in the database.");
//       }
//    }

using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AnimCooker
{
    public struct AnimDbData
    {
        // this array-of-arrays holds all the animation clip data
        // the first index is the model, and the second is the clip
        public BlobArray<BlobArray<AnimDbEntry>> Data;
    }

    public struct AnimDbRefData : IComponentData
    {
        public BlobAssetReference<AnimDbData> Ref;

        public AnimDbRefData(AnimationDatabase db)
        {
            using BlobBuilder builder = new BlobBuilder(Allocator.Temp); // using ensures that it's auto-disposed
            ref AnimDbData blobAsset = ref builder.ConstructRoot<AnimDbData>();

            // outer array
            BlobBuilderArray<BlobArray<AnimDbEntry>> models = builder.Allocate(ref blobAsset.Data, db.GetModelCount());

            for (int m = 0; m < db.GetModelCount(); m++) {
                // the clips array (for this model only)
                BlobBuilderArray<AnimDbEntry> clips = builder.Allocate(ref models[m], db.GetClipCount(m));
                for (int c = 0; c < db.GetClipCount(m); c++) {
                    clips[c] = db.GetClip(m, c);
                }
            }

            Ref = builder.CreateBlobAssetReference<AnimDbData>(Allocator.Persistent);
        }

        // returns the specified clip
        // No safety checks!
        // If you are using enums (like AnimDb.Model) or are sure that
        // the indexes are valid, this method is fast.
        public AnimDbEntry GetClip(int modelIndex, int clipIndex) { return Ref.Value.Data[modelIndex][clipIndex]; }

        public bool TryGetClip(int modelIndex, int clipIndex, out AnimDbEntry entry)
        {
            entry = default(AnimDbEntry);
            if ((modelIndex >= Ref.Value.Data.Length) || (modelIndex < 0)) { return false; }
		    if ((clipIndex >= Ref.Value.Data[modelIndex].Length) || (modelIndex < 0)) { return false; }
            entry = Ref.Value.Data[modelIndex][clipIndex];
            return true;
        }

        public int GetModelCount() { return Ref.Value.Data.Length; }

        public int GetClipCount(int modelIndex) { return Ref.Value.Data[modelIndex].Length; }

        // This function will return an array of all clips with names that partially contain the specified text.
        // For example, if you search for "Run" in AllClips{ Dog{ Run, Bark, Idle }, Cat{ Run, Meow, Attack}, Bird { Fly, Tweet, Idle}}
	    // the function will returns the clips corresponding with: { "Dog_Run", "Cat_Run", XXX }
        // Note that XXX is an uninitialized entry because Bird does not have a "Run" animation.
        // You my access the resulting array via a model index.
        // You must dispose the resulting array when you are done with it if you make it persistent.
        // This function is not super-fast, so it's recommended that you run it once and then cache the result.
        // clipText --> a clip of text to search for in all clip names for all models.
        // allocator --> the allocator to use for the returned list.
        public NativeArray<AnimDbEntry> FindClipsThatContain(FixedString128Bytes clipText, Allocator allocator = Allocator.Persistent)
        {
            int modelCount = GetModelCount();
            NativeArray<AnimDbEntry> arr = new NativeArray<AnimDbEntry>(modelCount, allocator, NativeArrayOptions.ClearMemory);
            for (int m = 0; m < modelCount; m++) {
                for (int c = 0; c < Ref.Value.Data[m].Length; c++) {
                    if (Ref.Value.Data[m][c].ClipName.Contains(clipText)) {
                        arr[m] = Ref.Value.Data[m][c];
                    }
                }
            }
            return arr;
        }

        // This function will return a list of all clips with names that match the specified text.
        // For example, if you search for "Run" in AllClips{ Dog{ RunFast, RunSlow, Bark, Idle }, Cat{ Run, Meow, Attack}, Bird { Fly, Tweet, Idle}}
        // the function will returns the clips corresponding with: { "Dog_RunSlow", "Cat_Run", XXX }
        // Note that because the Dog has two animations that contain "Run", only the second one is used.
        // Note that XXX is an uninitialized entry because Bird does not have a "Run" animation.
        // You my access the resulting array via a model index.
        // You must dispose the resulting list when you are done with it if you make it persistent.
        // This function is not super-fast, so it's recommended that you run it once and then cache the result.
        // clipText --> a clip of text to search for in all clip names for all models.
        // allocator --> the allocator to use for the returned list.
        public NativeArray<AnimDbEntry> FindClips(FixedString128Bytes clipText, Allocator allocator = Allocator.Persistent)
        {
            int modelCount = GetModelCount();
            NativeArray<AnimDbEntry> arr = new NativeArray<AnimDbEntry>(modelCount, allocator, NativeArrayOptions.ClearMemory);
            for (int m = 0; m < modelCount; m++) {
                for (int c = 0; c < Ref.Value.Data[m].Length; c++) {
                    if (Ref.Value.Data[m][c].ClipName == clipText) {
                        arr[m] = Ref.Value.Data[m][c];
                    }
                }
            }
            return arr;
        }

        // This function will return the first clip that partially or fully contains the specified text.
        // For example, if you search for "Run" in { "Dog_Run", "Dog_Idle", "Dog_Attack", "Cat_Run", "Cat_Attack" }
        // the function will give you the clip that corresponds with "Dog_Run" because it's the first match.
        // If the function fails, it will return false and entry will be set to the default value.
        // This function is not super-fast, so it's recommended that you run it once and then cache the result.
        // clipText --> a clip of text to search for in all clip names for all models.
        // entry --> the output entry if one is found.
        public bool FindFirstClipThatContains(FixedString128Bytes clipText, out AnimDbEntry entry)
        {
            entry = default(AnimDbEntry);
            int modelCount = GetModelCount();
            for (int m = 0; m < modelCount; m++) {
                for (int c = 0; c < Ref.Value.Data[m].Length; c++) {
                    if (Ref.Value.Data[m][c].ClipName.Contains(clipText)) {
                        entry = Ref.Value.Data[m][c];
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class AnimationDbAuthoring : MonoBehaviour
    {
        // nothing needs to be here - the data comes from AnimationDatabase
        // which is static read-only data that gets loaded via reflection.
    }

    public class AnimationDbBaker : Baker<AnimationDbAuthoring>
    {
        public override void Bake(AnimationDbAuthoring authoring)
        {
            AnimationDatabase db = AnimationDatabase.GetDb();

            Entity entity = GetEntity(authoring, TransformUsageFlags.None);
            AddComponent(entity, new AnimDbRefData(db));
        }
    }
} // namespace