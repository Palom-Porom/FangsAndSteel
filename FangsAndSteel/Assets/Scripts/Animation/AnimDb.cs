using System.Collections.Generic;
using AnimCooker;
public static class AnimDb
{
	public static void Populate(DoubleDictionary<string, AnimDbEntry> dict)
	{
	dict["FirstRobot", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 20, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Rest", ModelIndex = 5, ClipIndex = 0 };
	dict["FirstRobot", "Movement"] = new AnimDbEntry { BeginFrame = 21, EndFrame = 41, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Movement", ModelIndex = 5, ClipIndex = 1 };
	dict["FirstRobot", "Attack"] = new AnimDbEntry { BeginFrame = 42, EndFrame = 59, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Attack", ModelIndex = 5, ClipIndex = 2 };
	dict["FirstRobot", "Recharge"] = new AnimDbEntry { BeginFrame = 60, EndFrame = 80, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Recharge", ModelIndex = 5, ClipIndex = 3 };
	dict["FirstRobot", "Death_1"] = new AnimDbEntry { BeginFrame = 81, EndFrame = 101, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Death_1", ModelIndex = 5, ClipIndex = 4 };
	dict["FirstRobot", "Death_2"] = new AnimDbEntry { BeginFrame = 102, EndFrame = 115, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Death_2", ModelIndex = 5, ClipIndex = 5 };

	dict["Second_robot", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 15, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Rest", ModelIndex = 6, ClipIndex = 0 };
	dict["Second_robot", "Movement"] = new AnimDbEntry { BeginFrame = 16, EndFrame = 37, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Movement", ModelIndex = 6, ClipIndex = 1 };
	dict["Second_robot", "Attack"] = new AnimDbEntry { BeginFrame = 38, EndFrame = 57, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Attack", ModelIndex = 6, ClipIndex = 2 };
	dict["Second_robot", "Death"] = new AnimDbEntry { BeginFrame = 58, EndFrame = 73, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Death", ModelIndex = 6, ClipIndex = 3 };
	dict["Second_robot", "Rest_Deployed"] = new AnimDbEntry { BeginFrame = 74, EndFrame = 93, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Rest_Deployed", ModelIndex = 6, ClipIndex = 4 };
	dict["Second_robot", "Deploy"] = new AnimDbEntry { BeginFrame = 94, EndFrame = 109, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Deploy", ModelIndex = 6, ClipIndex = 5 };
	dict["Second_robot", "Death_Deployed"] = new AnimDbEntry { BeginFrame = 110, EndFrame = 124, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Death_Deployed", ModelIndex = 6, ClipIndex = 6 };
	dict["Second_robot", "Undeploy"] = new AnimDbEntry { BeginFrame = 125, EndFrame = 144, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Undeploy", ModelIndex = 6, ClipIndex = 7 };
	}
public enum Model { FirstRobot, Second_robot }
public enum FirstRobot { Rest, Movement, Attack, Recharge, Death_1, Death_2}
public enum Second_robot { Rest, Movement, Attack, Death, Rest_Deployed, Deploy, Death_Deployed, Undeploy }
}