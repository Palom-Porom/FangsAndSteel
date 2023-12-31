using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(BeginSimulationEntityCommandBufferSystem))]
[UpdateBefore(typeof(VariableRateSimulationSystemGroup))]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
public partial class NoJobsSystemGroup : ComponentSystemGroup { }


[UpdateAfter(typeof(FillVisionMapTextureSystem))]
public partial class StaticUISystemGroup : ComponentSystemGroup{ }


[UpdateAfter(typeof(StaticUISystemGroup))]
public partial class ControlsSystemGroup : ComponentSystemGroup { }

[UpdateAfter(typeof(ControlsSystemGroup))]
public partial class UnitsSystemGroup : ComponentSystemGroup { }

[UpdateAfter(typeof(UnitsSystemGroup))]
public partial class AfterUnitsSystemGroup : ComponentSystemGroup { }
