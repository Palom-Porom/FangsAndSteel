// This is a class that contains a static instance of itself (a "database" of animation clips).
// well... it's not really a database, but it can be used like one.
// It contains mainly information about animation clips - start frame, end frame, etc.
// It uses a generated C# file named AnimDb.cs to populate its contents.
// Internally, it stores data in multiple data structures, which makes writing to it slow, but
// makes reading fast and flexible.
// Enums are optional
//  
//
// WARNING ABOUT ENUMS: 
//   If you want to use enums to access your animations and your actions or model names
//   contain characters or names that are not compatible as enum values, then this class
//   will attempt to convert the string so that it's enum compatible. 
//   There's no guarantee that the conversion will work 100% of the time.
//   If a name does get converted, it might not correspond to the name used in the database.
//   So.... I suggest you keep your animation names simple 
//   (don't use funky special chars, don't start with numbers, don't use unity/c# keywords, don't use spaces)
//
// WARNING ABOUT WRITING:
//   You really shouldn't need to write to the database... that's usually done by the baking script.
//   If you absolutely need to write to it, and you don't save it, when you restart unity it'll lose whatever you wrote.
//   If you write to it, you are responsible for thread-safety because this class is not thread-safe by design.
//   Also, you should only use SetModelClips(), SetDictionary() and Clear() to write - otherwise the contents will get out of sync.
//
// To fetch the singleton database from anywhere:
//   AnimationDatabase db = AnimationDatabase.GetDb();
//
// To loop through the database using foreach:
//  foreach (var model in db) {
//     foreach (var clip in model.Value) {
//	      Debug.Log("Model: " + model.Key + ", Clip: " + clip.Key + ", Index: " + clip.index);
//     }
//  }
//
// To loop through the database using for-loops:
//  for (int m = 0; m < db.GetModelCount(); m++) {
//     for (int c = 0; c < db.GetClipCount(); c++) {
//        AnimDbEntry clip = db.GetClip(m, c);
//        Debug.Log("Model: " + clip.modelName + ", Clip index: " + clip.index);
//     }
//  }
//
// There are lots of ways to fetch information about a specific clip... here are a few:
//  Debug.Log("The first frame is: " + db.GetClip("Horse", "Walk").beginFrame);
//  Debug.Log("The first frame is: " + db.GetClip(0, "Idle").beginFrame;
//  Debug.Log("The first frame is: " + db.GetClip(AnimDb.Model.Horse, AnimDb.Horse.Run).beginFrame;
//  Debug.Log("The first frame is: " + db.GetClip("Horse", 0).beginFrame;
//
// Because AnimationDatabase is a class, you can't use it in ECS jobs unless the job is on the main thread.
// To get around this use GetNativeArray() and then access items via model-index and clip-index.
// See AnimationSystem for an example of this.
//--------------------------------------------------------------------------------------------------//

using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace AnimCooker
{

	// This struct is used to hold information about a clip.
	// It contains some redundant information (like modelName being repeated for each clip), but it's there for convenience
	// and since it's store in only one place in the database, it's not really wasting much space.
	public struct AnimDbEntry
	{
		public short BeginFrame; // the index of the first frame
		public short EndFrame; // the index of the last frame
		public float Interval; // based on the "sampled frame rate" (doesn't account for speed parameters)
		public FixedString128Bytes ClipName;
		public FixedString128Bytes ModelName;
		public byte ModelIndex;
		public byte ClipIndex;
		public short FrameCount;
		public float GetLength() { return Interval * FrameCount; }
		public bool IsValid() { return BeginFrame < EndFrame; }
	}

	public class AnimationDatabase : IEnumerable
	{
		static AnimationDatabase m_db = null; // the singleton instance of the database

		// holds a list of clips, where the clips are stored in a dictionary
		List<Dictionary<string, AnimDbEntry>> m_listOfDict = new List<Dictionary<string, AnimDbEntry>>();

		// holds a list of arrays
		// this must be unsafe because it is not possible to have a NativeList nested in a NativeList
		UnsafeList<UnsafeList<AnimDbEntry>> m_listOfArray = new UnsafeList<UnsafeList<AnimDbEntry>>(8, Allocator.Persistent);

		// the only reason we have this is to use its enumerator
		DoubleDictionary<string, AnimDbEntry> m_dictOfDict = new DoubleDictionary<string, AnimDbEntry>();

		Dictionary<string, int> m_modelIndexDict = new Dictionary<string, int>();
		List<string> m_modelNameList = new List<string>();


		// This function will return a static instance of the database (it is a singleton)
		// If one doesn't exist, it will use introspection to attempt to read in the data 
		// from a C# file called AnimDb. If it doesn't exists, a blank database will be returned.
		public static AnimationDatabase GetDb()
		{
			if (m_db != null) { return m_db; }
			// use reflection to attempt to create the database (AnimDb might not exist yet)
			System.Type dbType = System.Type.GetType("AnimDb");
			if (dbType != null) {
				System.Reflection.MethodInfo method = dbType.GetMethod("Populate");
				DoubleDictionary<string, AnimDbEntry> dict = new DoubleDictionary<string, AnimDbEntry>();
				if (method != null) { method.Invoke(null, new object[] { dict }); } // execute Populate() function
				m_db = new AnimationDatabase();
				m_db.SetDictionary(dict);
			}
			// if the above failed, then AnimDb hasn't been auto-generated yet,
			// so return a blank database instead
			if (m_db == null) { m_db = new AnimationDatabase(); }
			return m_db;
		}

		// returns -1 if the specified model name wasn't found
		public int GetModelIndex(string modelName)
		{
			if (!m_modelIndexDict.TryGetValue(modelName, out int idx)) { return -1; }
			return idx;
		}

		// returns "" if the specified model index wasn't found
		public string GetModelName(int modelIndex)
		{
			if (modelIndex >= m_modelNameList.Count) { return ""; }
			return m_modelNameList[modelIndex];
		}

		// Returns the number of models in the database.
		public int GetModelCount() { return m_modelIndexDict.Count; }

		// Returns the number of clips that the specified model has
		public int GetClipCount(string modelName)
		{
			int modelIndex = GetModelIndex(modelName);
			if (modelIndex < 0) { return 0; }
			return m_listOfDict[modelIndex].Count;
		}

		// Returns the number of clips that the specified model has
		public int GetClipCount(int modelIndex)
		{
			if (modelIndex >= m_listOfDict.Count) { return 0; }
			return m_listOfDict[modelIndex].Count;
		}

		// Returns true if the specified model name exists in the database.
		public bool ContainsModel(string modelName) { return m_modelIndexDict.Count > 0; }

		// Fetches a clip given the specified model name and clip name.
		// Returns true if the clip was found and false if it wasn't.
		public bool GetClip(string modelName, string clipName, out AnimDbEntry clip)
		{
			clip = default;
			return GetClip(GetModelIndex(modelName), clipName, out clip);
		}

		// Fetches a clip given the specified model index and clip name.
		// Returns true if the clip was found and false if it wasn't.
		public bool GetClip(int modelIndex, string clipName, out AnimDbEntry clip)
		{
			clip = default;
			if ((modelIndex >= m_listOfDict.Count) || (modelIndex < 0)) { return false; }
			return m_listOfDict[modelIndex].TryGetValue(clipName, out clip);
		}

		// Returns a clip given the specified model name and clip name.
		// If the specified clip doesn't exist, the default clip will be returned where beginFrame == endFrame.
		public AnimDbEntry GetClip(string modelName, string clipName)
		{
			if (!GetClip(modelName, clipName, out AnimDbEntry clip)) { return default; }
			return clip;
		}

		// Returns a clip given the specified model index and clip name.
		// If the specified clip doesn't exist, the default clip will be returned where beginFrame == endFrame.
		public AnimDbEntry GetClip(int modelIndex, string clipName)
		{
			if (!GetClip(modelIndex, clipName, out AnimDbEntry clip)) { return default; }
			return clip;
		}

		// Fetches a clip given a model index and a clip index.
		// Returns true if the clip was found and false if it wasn't.
		public bool GetClip(int modelIndex, int clipIndex, out AnimDbEntry clip)
		{
			clip = default;
			if ((modelIndex >= m_listOfArray.Length) || (modelIndex < 0)) { return false; }
			if (clipIndex >= m_listOfArray[modelIndex].Length) { return false; }
			clip = m_listOfArray[modelIndex][clipIndex];
			return true;
		}

		// Returns a clip given the specified model index and clip index.
		// If the specified clip doesn't exist, the default clip will be returned where beginFrame == endFrame.
		public AnimDbEntry GetClip(int modelIndex, int clipIndex)
		{
			if (!GetClip(modelIndex, clipIndex, out AnimDbEntry clip)) { return default; }
			return clip;
		}

		// returns a clip given the specified model index and clip index
		// returns true if the clip was found and false if it wasn't
		public bool GetClip(string modelName, int clipIndex, out AnimDbEntry clip)
		{
			return GetClip(GetModelIndex(modelName), clipIndex, out clip);
		}

		// Returns a clip given the specified model index and clip index.
		// If the specified clip doesn't exist, the default clip will be returned where beginFrame == endFrame.
		public AnimDbEntry GetClip(string modelName, int clipIndex)
		{
			return GetClip(GetModelIndex(modelName), clipIndex);
		}

		// Searches for the first clip name that contains the specified word (case insensitive)
		// For example, if your clips are { "Dog_Run", "Dog_Idle", "Dog_Attack" } and you search for "run",
		// the function will find the clip corresponding with "Dog_Run".
		public bool FindClipThatContains(int modelIndex, string text, out AnimDbEntry value)
		{
			value = default;
			if ((modelIndex < 0) || (modelIndex >= m_modelIndexDict.Count)) { return false; }

			foreach (var clip in m_listOfDict[modelIndex]) {
				// case insensitive search on the key (won't work for all languages)
				if (clip.Key.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) >= 0) {
					value = clip.Value;
					return true;
				}
			}
			return false;
		}

		// same as above, but this version is overloaded to take a string for the model name
		public bool FindClipThatContains(string modelName, string text, out AnimDbEntry value)
		{
			return FindClipThatContains(GetModelIndex(modelName), text, out value);
		}

		// fetch a dictionary of clips corresponding with the specified model name
		// if the model name doesn't exist, an empty dictionary is returned
		public Dictionary<string, AnimDbEntry> GetClipDictionary(string modelName)
		{
			int modelIndex = GetModelIndex(modelName);
			if (modelIndex < 0) { return default; }
			return m_listOfDict[modelIndex];
		}

		// fetch a list of clips corresponding with the specified model index
		// if the model index doesn't exist, an empty dictionary is returned
		public Dictionary<string, AnimDbEntry> GetClipDictionary(int modelIndex)
		{
			if ((modelIndex >= m_listOfDict.Count) || (modelIndex < 0)) { return default; }
			return m_listOfDict[modelIndex];
		}

		// [WRITE FUNCTION] set the animation clips for the specified model.
		// (usually this is only used by the bake function)
		public void SetModelClips(string modelName, Dictionary<string, AnimDbEntry> clips)
		{
			// search for the model index
			int modelIndex = GetModelIndex(modelName);

			// for m_dictOfList and m_arrayOfArray we need to create clipList and clipArr
			MakeClipArr(clips, out UnsafeList<AnimDbEntry> clipArr);

			if (modelIndex >= 0) {
				m_listOfDict[modelIndex] = clips;
				m_listOfArray[modelIndex] = clipArr;
				m_dictOfDict[modelName] = clips;
			} else {
				m_listOfDict.Add(clips);
				m_listOfArray.Add(clipArr);
				m_dictOfDict.Add(modelName, clips);
				m_modelIndexDict.Add(modelName, m_modelIndexDict.Count);
				m_modelNameList.Add(modelName);
			}
		}

		// [WRITE FUNCTION] set the underlying dictionary 
		// (usually this is only used by the bake function)
		public void SetDictionary(DoubleDictionary<string, AnimDbEntry> dict)
		{
			Clear();
			foreach (var model in dict) { SetModelClips(model.Key, model.Value); }
		}

		// [WRITE] deletes all items and frees memory
		// (usually this is only called by the bake function)
		public void Clear()
		{
			m_dictOfDict.Clear();
			m_listOfDict.Clear();
			for (int i = 0; i < m_listOfArray.Length; i++) { m_listOfArray[i].Dispose(); }
			m_listOfArray.Dispose();
			m_listOfArray = new UnsafeList<UnsafeList<AnimDbEntry>>(8, Allocator.Persistent);
			m_modelNameList.Clear();
			m_modelIndexDict.Clear();
		}

		// Fetches a 2D UnsafeList. 
		// I had to use the unsafe version because the safe version doesn't allow nesting.
		// I don't think it makes a copy, so you should NOT call dispose on the value.
		public void GetNativeDatabase(out UnsafeList<UnsafeList<AnimDbEntry>> listOfArray)
		{
			listOfArray = m_listOfArray;
		}

		// Fills listOfClips such that each entry corresponds with a model.
		// If the model has a clip name that contains clipText, then the entry will contain that clip,
		// but if not, then the entry will contain a default AnimDbEntry.
		// You must dispose of the array when you are done with it.
		// Example: var listOfClips = db.FindNativeClips("Run");
		// (return an array of Run animation items)
		// You can check if a clip is valid by: if (list[m].beginFrame != list[m].endFrame) {}
		// Note that if an particular model had a "RunForward" and a "RunBackward", 
		// Which one you get is unpredictable, so you should either narrow your search by searching
		// for "RunForward" instead, or rename your clips so that there is only one "Run".
		public NativeArray<AnimDbEntry> FindNativeClips(string clipText)
		{
			NativeArray<AnimDbEntry>  listOfClips = new NativeArray<AnimDbEntry>(m_db.GetModelCount(), Allocator.Persistent);
			for (int m = 0; m < m_modelIndexDict.Count; m++) {
				m_db.FindClipThatContains(m, clipText, out AnimDbEntry item);
				listOfClips[m] = item;
			}
			return listOfClips;
		}

		public IEnumerator<KeyValuePair<string, Dictionary<string, AnimDbEntry>>> GetEnumerator()
		{
			return m_dictOfDict.GetEnumerator();
		}


		// ################################## PRIVATE ##################################

		static void MakeClipArr(Dictionary<string, AnimDbEntry> clips, out UnsafeList<AnimDbEntry> clipArr)
		{
			clipArr = new UnsafeList<AnimDbEntry>(clips.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			foreach (var clip in clips) {
				clipArr.Add(clip.Value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)m_listOfDict).GetEnumerator();
		}
	}
} // namespace