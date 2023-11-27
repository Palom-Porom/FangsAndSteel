// Attaching this class to a gameobject that is going to be converted to an entity
// will cause the resulting entity to have AnimationCollectionData, MaterialBeginFrame,
// and MaterialEndFrame components attached to it.
// You can then use those components in your systems to animate baked models.
//--------------------------------------------------------------------------------------------------//

using Unity.Entities;
using UnityEngine;

namespace AnimCooker
{
	public class AnimationClipAuthoring : MonoBehaviour
	{
		public string m_animationModelName; // example: "Horse", "Metalon"
		public string m_defaultAnimationName; // usually "Idle"

		public bool m_addMaterialAnimationSpeedProperty = true;
		public bool m_addMaterialCurrentTimeProperty = true;
		public bool m_addMaterialClipIndexProperty = true;

		public bool m_addMaterialAnimationStateProperty = true;
		public bool m_addMaterialAnimationCmdProperty = true;

		public void SetAnimationModel(string modelName) { m_animationModelName = modelName; }
		public void SetDefaultAnimation(string animName) { m_defaultAnimationName = animName; }
	}

	public class AnimationClipBaker : Baker<AnimationClipAuthoring>
	{
		public override void Bake(AnimationClipAuthoring authoring)
		{	
			AnimationDatabase db = AnimationDatabase.GetDb();
			int indexOfModel = db.GetModelIndex(authoring.m_animationModelName);
			if (indexOfModel >= 0) {

				// make an attempt to find the default clip and make that the default "forever" clip index
				byte defaultClipIndex = 0;
				if (db.GetClip(indexOfModel, authoring.m_defaultAnimationName, out AnimDbEntry defaultClip)) {
					defaultClipIndex = defaultClip.ClipIndex;
				}

				Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

				// add some per-instance components that can be used to set where the play loop starts and ends.
				if (authoring.m_addMaterialAnimationSpeedProperty) { AddComponent(entity, new MaterialAnimationSpeed() { multiplier = 1.0f }); }
				if (authoring.m_addMaterialCurrentTimeProperty) { AddComponent<MaterialCurrentTime>(entity); }
				if (authoring.m_addMaterialClipIndexProperty) { AddComponent<MaterialClipIndex>(entity); }
				if (authoring.m_addMaterialAnimationStateProperty) { AddComponent(entity, new AnimationStateData() { CurrentClipIndex = defaultClipIndex, ForeverClipIndex = defaultClipIndex, Mode = AnimationPlayMode.PlayForever, ModelIndex = (byte)indexOfModel }); }
				if (authoring.m_addMaterialAnimationCmdProperty) { AddComponent(entity, new AnimationCmdData() { ClipIndex = defaultClipIndex, Cmd = AnimationCmd.SetPlayForever, Speed = 1f }); }
			}
		}
	}
} // namespace