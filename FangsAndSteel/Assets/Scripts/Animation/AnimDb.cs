using System.Collections.Generic;
using AnimCooker;
public static class AnimDb
{
	public static void Populate(DoubleDictionary<string, AnimDbEntry> dict)
	{
	dict["FirstRobot", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 20, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Rest", ModelIndex = 0, ClipIndex = 0 };
	dict["FirstRobot", "Move"] = new AnimDbEntry { BeginFrame = 21, EndFrame = 41, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Move", ModelIndex = 0, ClipIndex = 1 };
	dict["FirstRobot", "Attack"] = new AnimDbEntry { BeginFrame = 42, EndFrame = 59, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Attack", ModelIndex = 0, ClipIndex = 2 };
	dict["FirstRobot", "Recharge"] = new AnimDbEntry { BeginFrame = 60, EndFrame = 80, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Recharge", ModelIndex = 0, ClipIndex = 3 };
	dict["FirstRobot", "Death_1"] = new AnimDbEntry { BeginFrame = 81, EndFrame = 101, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Death_1", ModelIndex = 0, ClipIndex = 4 };
	dict["FirstRobot", "Death"] = new AnimDbEntry { BeginFrame = 102, EndFrame = 115, Interval = 0.07692308f, ModelName = "FirstRobot", ClipName = "Death", ModelIndex = 0, ClipIndex = 5 };

	//dict["Second_robot", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 15, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Rest", ModelIndex = 6, ClipIndex = 0 };
	//dict["Second_robot", "Move"] = new AnimDbEntry { BeginFrame = 16, EndFrame = 37, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Move", ModelIndex = 6, ClipIndex = 1 };
	//dict["Second_robot", "Attack"] = new AnimDbEntry { BeginFrame = 38, EndFrame = 57, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Attack", ModelIndex = 6, ClipIndex = 2 };
	//dict["Second_robot", "Death"] = new AnimDbEntry { BeginFrame = 58, EndFrame = 73, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Death", ModelIndex = 6, ClipIndex = 3 };
	//dict["Second_robot", "Rest_Deployed"] = new AnimDbEntry { BeginFrame = 74, EndFrame = 93, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Rest_Deployed", ModelIndex = 6, ClipIndex = 4 };
	//dict["Second_robot", "Undeploy"] = new AnimDbEntry { BeginFrame = 94, EndFrame = 108, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Undeploy", ModelIndex = 6, ClipIndex = 5 };
	//dict["Second_robot", "Death_Deployed"] = new AnimDbEntry { BeginFrame = 110, EndFrame = 124, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Death_Deployed", ModelIndex = 6, ClipIndex = 6 };
	//dict["Second_robot", "Deploy"] = new AnimDbEntry { BeginFrame = 125, EndFrame = 144, Interval = 0.1f, ModelName = "Second_robot", ClipName = "Deploy", ModelIndex = 6, ClipIndex = 7 };

    dict["Tank_Hands", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 5, Interval = 0.125f, ModelName = "Tank_Hands", ClipName = "Rest", ModelIndex = 1, ClipIndex = 0 };
    dict["Tank_Hands", "Move"] = new AnimDbEntry { BeginFrame = 6, EndFrame = 11, Interval = 0.125f, ModelName = "Tank_Hands", ClipName = "Move", ModelIndex = 1, ClipIndex = 1 };
    dict["Tank_Hands", "Death"] = new AnimDbEntry { BeginFrame = 12, EndFrame = 31, Interval = 0.125f, ModelName = "Tank_Hands", ClipName = "Death", ModelIndex = 1, ClipIndex = 2 };
    dict["Tank_Hands", "Attack"] = new AnimDbEntry { BeginFrame = 32, EndFrame = 46, Interval = 0.125f, ModelName = "Tank_Hands", ClipName = "Attack", ModelIndex = 1, ClipIndex = 3 };
    dict["Tank_Hands", "Recharge"] = new AnimDbEntry { BeginFrame = 47, EndFrame = 87, Interval = 0.125f, ModelName = "Tank_Hands", ClipName = "Recharge", ModelIndex = 1, ClipIndex = 4 };
    dict["Tank_Hull", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 5, Interval = 0.125f, ModelName = "Tank_Hull", ClipName = "Rest", ModelIndex = 2, ClipIndex = 0 };
    dict["Tank_Hull", "Move"] = new AnimDbEntry { BeginFrame = 6, EndFrame = 11, Interval = 0.125f, ModelName = "Tank_Hull", ClipName = "Move", ModelIndex = 2, ClipIndex = 1 };
    dict["Tank_Hull", "Death"] = new AnimDbEntry { BeginFrame = 12, EndFrame = 31, Interval = 0.125f, ModelName = "Tank_Hull", ClipName = "Death", ModelIndex = 2, ClipIndex = 2 };
    dict["Tank_Hull", "Attack"] = new AnimDbEntry { BeginFrame = 32, EndFrame = 46, Interval = 0.125f, ModelName = "Tank_Hull", ClipName = "Attack", ModelIndex = 2, ClipIndex = 3 };
    dict["Tank_Hull", "Recharge"] = new AnimDbEntry { BeginFrame = 47, EndFrame = 87, Interval = 0.125f, ModelName = "Tank_Hull", ClipName = "Recharge", ModelIndex = 2, ClipIndex = 4 };
    dict["Tank_Turret", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 8, Interval = 0.09090909f, ModelName = "Tank_Turret", ClipName = "Rest", ModelIndex = 3, ClipIndex = 0 };
    dict["Tank_Turret", "Move"] = new AnimDbEntry { BeginFrame = 9, EndFrame = 17, Interval = 0.09090909f, ModelName = "Tank_Turret", ClipName = "Move", ModelIndex = 3, ClipIndex = 1 };
    dict["Tank_Turret", "Death"] = new AnimDbEntry { BeginFrame = 18, EndFrame = 44, Interval = 0.09090909f, ModelName = "Tank_Turret", ClipName = "Death", ModelIndex = 3, ClipIndex = 2 };
    dict["Tank_Turret", "Attack"] = new AnimDbEntry { BeginFrame = 45, EndFrame = 64, Interval = 0.09090909f, ModelName = "Tank_Turret", ClipName = "Attack", ModelIndex = 3, ClipIndex = 3 };
    dict["Tank_Turret", "Recharge"] = new AnimDbEntry { BeginFrame = 65, EndFrame = 121, Interval = 0.09090909f, ModelName = "Tank_Turret", ClipName = "Recharge", ModelIndex = 3, ClipIndex = 4 };
    dict["Tank_Hands_Fixed", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 5, Interval = 0.125f, ModelName = "Tank_Hands_Fixed", ClipName = "Rest", ModelIndex = 4, ClipIndex = 0 };
    dict["Tank_Hands_Fixed", "Move"] = new AnimDbEntry { BeginFrame = 6, EndFrame = 11, Interval = 0.125f, ModelName = "Tank_Hands_Fixed", ClipName = "Move", ModelIndex = 4, ClipIndex = 1 };
    dict["Tank_Hands_Fixed", "Death"] = new AnimDbEntry { BeginFrame = 12, EndFrame = 31, Interval = 0.125f, ModelName = "Tank_Hands_Fixed", ClipName = "Death", ModelIndex = 4, ClipIndex = 2 };
    dict["Tank_Hands_Fixed", "Attack"] = new AnimDbEntry { BeginFrame = 32, EndFrame = 46, Interval = 0.125f, ModelName = "Tank_Hands_Fixed", ClipName = "Attack", ModelIndex = 4, ClipIndex = 3 };
    dict["Tank_Hands_Fixed", "Recharge"] = new AnimDbEntry { BeginFrame = 47, EndFrame = 87, Interval = 0.125f, ModelName = "Tank_Hands_Fixed", ClipName = "Recharge", ModelIndex = 4, ClipIndex = 4 };
    dict["Artillery_Hull", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 4, Interval = 0.1666667f, ModelName = "Artillery_Hull", ClipName = "Rest", ModelIndex = 5, ClipIndex = 0 };
    dict["Artillery_Hull", "Move"] = new AnimDbEntry { BeginFrame = 5, EndFrame = 14, Interval = 0.1666667f, ModelName = "Artillery_Hull", ClipName = "Move", ModelIndex = 5, ClipIndex = 1 };
    dict["Artillery_Hull", "Death"] = new AnimDbEntry { BeginFrame = 15, EndFrame = 20, Interval = 0.1666667f, ModelName = "Artillery_Hull", ClipName = "Death", ModelIndex = 5, ClipIndex = 2 };
    dict["Artillery_Hull", "Attack"] = new AnimDbEntry { BeginFrame = 21, EndFrame = 71, Interval = 0.1666667f, ModelName = "Artillery_Hull", ClipName = "Attack", ModelIndex = 5, ClipIndex = 3 };
    dict["Artillery_Hull", "Recharge"] = new AnimDbEntry { BeginFrame = 72, EndFrame = 147, Interval = 0.1666667f, ModelName = "Artillery_Hull", ClipName = "Recharge", ModelIndex = 5, ClipIndex = 4 };
    dict["Artillery_Hull", "Deploy"] = new AnimDbEntry { BeginFrame = 148, EndFrame = 165, Interval = 0.1666667f, ModelName = "Artillery_Hull", ClipName = "Deploy", ModelIndex = 5, ClipIndex = 5 };
    dict["Artillery_Hull", "Undeploy"] = new AnimDbEntry { BeginFrame = 166, EndFrame = 183, Interval = 0.1666667f, ModelName = "Artillery_Hull", ClipName = "Undeploy", ModelIndex = 5, ClipIndex = 6 };
    dict["Artillery_Hull", "Rest_Deployed"] = new AnimDbEntry { BeginFrame = 184, EndFrame = 188, Interval = 0.1666667f, ModelName = "Artillery_Hull", ClipName = "Deployed_Rest", ModelIndex = 5, ClipIndex = 7 };
    dict["Artillery_Hull", "Death_Deployed"] = new AnimDbEntry { BeginFrame = 189, EndFrame = 193, Interval = 0.1666667f, ModelName = "Artillery_Hull", ClipName = "Deployed_Death", ModelIndex = 5, ClipIndex = 8 };
    dict["Artillery_Turret", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 4, Interval = 0.1428571f, ModelName = "Artillery_Turret", ClipName = "Rest", ModelIndex = 6, ClipIndex = 0 };
    dict["Artillery_Turret", "Move"] = new AnimDbEntry { BeginFrame = 5, EndFrame = 15, Interval = 0.1428571f, ModelName = "Artillery_Turret", ClipName = "Move", ModelIndex = 6, ClipIndex = 1 };
    dict["Artillery_Turret", "Death"] = new AnimDbEntry { BeginFrame = 16, EndFrame = 22, Interval = 0.1428571f, ModelName = "Artillery_Turret", ClipName = "Death", ModelIndex = 6, ClipIndex = 2 };
    dict["Artillery_Turret", "Attack"] = new AnimDbEntry { BeginFrame = 23, EndFrame = 81, Interval = 0.1428571f, ModelName = "Artillery_Turret", ClipName = "Attack", ModelIndex = 6, ClipIndex = 3 };
    dict["Artillery_Turret", "Recharge"] = new AnimDbEntry { BeginFrame = 82, EndFrame = 169, Interval = 0.1428571f, ModelName = "Artillery_Turret", ClipName = "Recharge", ModelIndex = 6, ClipIndex = 4 };
    dict["Artillery_Turret", "Deploy"] = new AnimDbEntry { BeginFrame = 170, EndFrame = 190, Interval = 0.1428571f, ModelName = "Artillery_Turret", ClipName = "Deploy", ModelIndex = 6, ClipIndex = 5 };
    dict["Artillery_Turret", "Undeploy"] = new AnimDbEntry { BeginFrame = 191, EndFrame = 211, Interval = 0.1428571f, ModelName = "Artillery_Turret", ClipName = "Undeploy", ModelIndex = 6, ClipIndex = 6 };
    dict["Artillery_Turret", "Rest_Deployed"] = new AnimDbEntry { BeginFrame = 212, EndFrame = 216, Interval = 0.1428571f, ModelName = "Artillery_Turret", ClipName = "Deployed_Rest", ModelIndex = 6, ClipIndex = 7 };
    dict["Artillery_Turret", "Death_Deployed"] = new AnimDbEntry { BeginFrame = 217, EndFrame = 222, Interval = 0.1428571f, ModelName = "Artillery_Turret", ClipName = "Deployed_Death", ModelIndex = 6, ClipIndex = 8 };
    dict["Robot_AntyTank_3", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 4, Interval = 0.04166667f, ModelName = "Robot_AntyTank_3", ClipName = "Rest", ModelIndex = 7, ClipIndex = 0 };
    dict["Robot_AntyTank_3", "Move"] = new AnimDbEntry { BeginFrame = 5, EndFrame = 44, Interval = 0.04166667f, ModelName = "Robot_AntyTank_3", ClipName = "Move", ModelIndex = 7, ClipIndex = 1 };
    dict["Robot_AntyTank_3", "Death"] = new AnimDbEntry { BeginFrame = 45, EndFrame = 99, Interval = 0.04166667f, ModelName = "Robot_AntyTank_3", ClipName = "Death", ModelIndex = 7, ClipIndex = 2 };
    dict["Robot_AntyTank_3", "Recharge"] = new AnimDbEntry { BeginFrame = 100, EndFrame = 199, Interval = 0.04166667f, ModelName = "Robot_AntyTank_3", ClipName = "Recharge", ModelIndex = 7, ClipIndex = 3 };
    dict["Robot_AntyTank_3", "Attack"] = new AnimDbEntry { BeginFrame = 200, EndFrame = 319, Interval = 0.04166667f, ModelName = "Robot_AntyTank_3", ClipName = "Attack", ModelIndex = 7, ClipIndex = 4 };
    dict["Robot_AntyTank_Blue", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = -1, Interval = 0.3333333f, ModelName = "Robot_AntyTank_Blue", ClipName = "Rest", ModelIndex = 8, ClipIndex = 0 };
    dict["Robot_AntyTank_Blue", "Move"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 4, Interval = 0.3333333f, ModelName = "Robot_AntyTank_Blue", ClipName = "Move", ModelIndex = 8, ClipIndex = 1 };
    dict["Robot_AntyTank_Blue", "Death"] = new AnimDbEntry { BeginFrame = 5, EndFrame = 10, Interval = 0.3333333f, ModelName = "Robot_AntyTank_Blue", ClipName = "Death", ModelIndex = 8, ClipIndex = 2 };
    dict["Robot_AntyTank_Blue", "Recharge"] = new AnimDbEntry { BeginFrame = 11, EndFrame = 22, Interval = 0.3333333f, ModelName = "Robot_AntyTank_Blue", ClipName = "Recharge", ModelIndex = 8, ClipIndex = 3 };
    dict["Robot_AntyTank_Blue", "Attack"] = new AnimDbEntry { BeginFrame = 23, EndFrame = 37, Interval = 0.3333333f, ModelName = "Robot_AntyTank_Blue", ClipName = "Attack", ModelIndex = 8, ClipIndex = 4 };

    dict["Second_robot_1", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 2, Interval = 0.05263158f, ModelName = "Second_robot_1", ClipName = "Rest", ModelIndex = 9, ClipIndex = 0 };
    dict["Second_robot_1", "Move"] = new AnimDbEntry { BeginFrame = 3, EndFrame = 33, Interval = 0.05263158f, ModelName = "Second_robot_1", ClipName = "Move", ModelIndex = 9, ClipIndex = 1 };
    dict["Second_robot_1", "Attack"] = new AnimDbEntry { BeginFrame = 34, EndFrame = 36, Interval = 0.05263158f, ModelName = "Second_robot_1", ClipName = "Attack", ModelIndex = 9, ClipIndex = 2 };
    dict["Second_robot_1", "Death"] = new AnimDbEntry { BeginFrame = 37, EndFrame = 86, Interval = 0.05263158f, ModelName = "Second_robot_1", ClipName = "Death", ModelIndex = 9, ClipIndex = 3 };
    dict["Second_robot_1", "Deploy"] = new AnimDbEntry { BeginFrame = 87, EndFrame = 119, Interval = 0.05263158f, ModelName = "Second_robot_1", ClipName = "Deploy", ModelIndex = 9, ClipIndex = 4 };
    dict["Second_robot_1", "Rest_Deployed"] = new AnimDbEntry { BeginFrame = 120, EndFrame = 122, Interval = 0.05263158f, ModelName = "Second_robot_1", ClipName = "Rest_Deployed", ModelIndex = 9, ClipIndex = 5 };
    dict["Second_robot_1", "Death_Deployed"] = new AnimDbEntry { BeginFrame = 123, EndFrame = 155, Interval = 0.05263158f, ModelName = "Second_robot_1", ClipName = "Death_Deployed", ModelIndex = 9, ClipIndex = 6 };
    dict["Second_robot_1", "Undeploy"] = new AnimDbEntry { BeginFrame = 156, EndFrame = 205, Interval = 0.05263158f, ModelName = "Second_robot_1", ClipName = "Undeploy", ModelIndex = 9, ClipIndex = 7 };
    dict["Second_robot_1", "Recharge"] = new AnimDbEntry { BeginFrame = 206, EndFrame = 303, Interval = 0.05263158f, ModelName = "Second_robot_1", ClipName = "Recharge", ModelIndex = 9, ClipIndex = 8 };
    dict["Second_robot_Blue", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = -1, Interval = 0.25f, ModelName = "Second_robot_Blue", ClipName = "Rest", ModelIndex = 10, ClipIndex = 0 };
    dict["Second_robot_Blue", "Move"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 5, Interval = 0.25f, ModelName = "Second_robot_Blue", ClipName = "Move", ModelIndex = 10, ClipIndex = 1 };
    dict["Second_robot_Blue", "Attack"] = new AnimDbEntry { BeginFrame = 6, EndFrame = 5, Interval = 0.25f, ModelName = "Second_robot_Blue", ClipName = "Attack", ModelIndex = 10, ClipIndex = 2 };
    dict["Second_robot_Blue", "Death"] = new AnimDbEntry { BeginFrame = 6, EndFrame = 15, Interval = 0.25f, ModelName = "Second_robot_Blue", ClipName = "Death", ModelIndex = 10, ClipIndex = 3 };
    dict["Second_robot_Blue", "Deploy"] = new AnimDbEntry { BeginFrame = 16, EndFrame = 22, Interval = 0.25f, ModelName = "Second_robot_Blue", ClipName = "Deploy", ModelIndex = 10, ClipIndex = 4 };
    dict["Second_robot_Blue", "Rest_Deployed"] = new AnimDbEntry { BeginFrame = 23, EndFrame = 22, Interval = 0.25f, ModelName = "Second_robot_Blue", ClipName = "Rest_Deployed ", ModelIndex = 10, ClipIndex = 5 };
    dict["Second_robot_Blue", "Death_Deployed"] = new AnimDbEntry { BeginFrame = 23, EndFrame = 29, Interval = 0.25f, ModelName = "Second_robot_Blue", ClipName = "Death_Deployed", ModelIndex = 10, ClipIndex = 6 };
    dict["Second_robot_Blue", "Undeploy"] = new AnimDbEntry { BeginFrame = 30, EndFrame = 39, Interval = 0.25f, ModelName = "Second_robot_Blue", ClipName = "Undeploy", ModelIndex = 10, ClipIndex = 7 };
    dict["Second_robot_Blue", "Recharge"] = new AnimDbEntry { BeginFrame = 40, EndFrame = 59, Interval = 0.25f, ModelName = "Second_robot_Blue", ClipName = "Recharge", ModelIndex = 10, ClipIndex = 8 };
    dict["FirstRobot Blue", "Rest"] = new AnimDbEntry { BeginFrame = 0, EndFrame = 9, Interval = 0.1666667f, ModelName = "FirstRobot Blue", ClipName = "Rest", ModelIndex = 11, ClipIndex = 0 };
    dict["FirstRobot Blue", "Move"] = new AnimDbEntry { BeginFrame = 10, EndFrame = 19, Interval = 0.1666667f, ModelName = "FirstRobot Blue", ClipName = "Move", ModelIndex = 11, ClipIndex = 1 };
    dict["FirstRobot Blue", "Attack"] = new AnimDbEntry { BeginFrame = 20, EndFrame = 27, Interval = 0.1666667f, ModelName = "FirstRobot Blue", ClipName = "Atack", ModelIndex = 11, ClipIndex = 2 };
    dict["FirstRobot Blue", "Recharge"] = new AnimDbEntry { BeginFrame = 28, EndFrame = 37, Interval = 0.1666667f, ModelName = "FirstRobot Blue", ClipName = "Recharge", ModelIndex = 11, ClipIndex = 3 };
    dict["FirstRobot Blue", "Death_1"] = new AnimDbEntry { BeginFrame = 38, EndFrame = 47, Interval = 0.1666667f, ModelName = "FirstRobot Blue", ClipName = "Death_1", ModelIndex = 11, ClipIndex = 4 };
    dict["FirstRobot Blue", "Death_2"] = new AnimDbEntry { BeginFrame = 48, EndFrame = 54, Interval = 0.1666667f, ModelName = "FirstRobot Blue", ClipName = "Death_2", ModelIndex = 11, ClipIndex = 5 };
    }
public enum Model { FirstRobot, /*Second_robot*/ Tank_Hands, Tank_Hull, Tank_Turret, Tank_Hands_Fixed, Artillery_Hull, Artillery_Turret, Robot_AntyTank_3, Robot_AntyTank_Blue, Second_robot_1, Second_robot_Blue, FirstRobotBlue }
public enum FirstRobot { Rest, Move, Attack, Recharge, Death_1, Death_2}
    //public enum Second_robot { Rest, Move, Attack, Death, Rest_Deployed, Deploy, Death_Deployed, Undeploy }
    public enum Tank_Hands { Armature_Tank_HandsHands_Rest, Armature_Tank_HandsHands_Move, Armature_Tank_HandsHands_Death, Armature_Tank_HandsHands_Attack, Armature_Tank_HandsHands_Recharge }
    public enum Tank_Hull { Armature_Tank_HullHull_Rest, Armature_Tank_HullHull_Move, Armature_Tank_HullHull_Death, Armature_Tank_HullHull_Attack, Armature_Tank_HullHull_Recharge }
    public enum Tank_Turret { Armature_Tank_TurretTurret_Rest, Armature_Tank_TurretTurret_Move, Armature_Tank_TurretTurret_Death, Armature_Tank_TurretTurret_Attack, Armature_Tank_TurretTurret_Recharge }
    public enum Tank_Hands_Fixed { Armature_Tank_HandsHands_Rest, Armature_Tank_HandsHands_Move, Armature_Tank_HandsHands_Death, Armature_Tank_HandsHands_Attack, Armature_Tank_HandsHands_Recharge }
    public enum Artillery_Hull { Armature_Arta_HullHull_Rest, Armature_Arta_HullHull_Move, Armature_Arta_HullHull_Death, Armature_Arta_HullHull_Attack, Armature_Arta_HullHull_Recharge, Armature_Arta_HullHull_Deploy, Armature_Arta_HullHull_Undeploy, Armature_Arta_HullHull_Deployed_Rest, Armature_Arta_HullHull_Deployed_Death }
    public enum Artillery_Turret { Armature_Arta_TurretTurret_Rest, Armature_Arta_TurretTurret_Move, Armature_Arta_TurretTurret_Death, Armature_Arta_TurretTurret_Attack, Armature_Arta_TurretTurret_Recharge, Armature_Arta_TurretTurret_Deploy, Armature_Arta_TurretTurret_Undeploy, Armature_Arta_TurretTurret_Deployed_Rest, Armature_Arta_TurretTurret_Deployed_Death }
    public enum Robot_AntyTank_3 { ArmatureRest, Armaturemovement, ArmatureDeath, ArmatureRecharge, ArmatureAttack }
    public enum Robot_AntyTank_Blue { ArmatureRest, Armaturemovement, ArmatureDeath, ArmatureRecharge, ArmatureAttack }
    public enum Second_robot_1 { MG2_Rest, MG2_Move, MG2_Atack, MG2_Death, MG2_Deploy, MG2_Rest_Deployed, MG2_Death_Deployed, MG2_Stand_Up, MG2_Recharge }
    public enum Second_robot_Blue { Armature001BM2_Rest, Armature001BM2_Move, Armature001BM2_Atack, Armature001BM2_Death, Armature001BM2_Deploy, Armature001BM2_Rest_Deployed, Armature001BM2_Death_Deployed, Armature001BM2_Stand_Up, Armature001BM2_Recharge }
    public enum FirstRobotBlue { ArmatureArmatureArmatureRest, ArmatureArmatureArmatureMove, ArmatureArmatureArmatureAtack, ArmatureArmatureArmatureRecharge, ArmatureArmatureArmatureDeath_1, ArmatureArmatureArmatureDeath_2 }
}