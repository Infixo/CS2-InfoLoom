using Colossal.UI.Binding;
using Game.Simulation;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Scripting;

namespace InfoLoom;

[CompilerGenerated]
public class BuildingDemandUISystem : UISystemBase
{
    // systems to get the data from
    private SimulationSystem m_SimulationSystem;
    private ResidentialDemandSystem m_ResidentialDemandSystem;
    private CommercialDemandSystem m_CommercialDemandSystem;
    private IndustrialDemandSystem m_IndustrialDemandSystem;

    // ui bindings
    private RawValueBinding m_uiBuildingDemand;
    //private RawValueBinding m_uiCompanyDemand;

    // building demands
    private NativeArray<int> m_BuildingDemand;
    // 0 - low res (ResidentialDemandSystem.BuildingDemand.z)
    // 1 - med res (ResidentialDemandSystem.BuildingDemand.y)
    // 2 - high res (ResidentialDemandSystem.BuildingDemand.x)
    // 3 - commercial (CommercialDemandSystem.m_BuildingDemand)
    // 4 - industry (IndustrialDemandSystem.m_IndustrialBuildingDemand)
    // 5 - storage (IndustrialDemandSystem.m_StorageBuildingDemand)
    // 6 - office (IndustrialDemandSystem.m_OfficeBuildingDemand)

    // company demands
    /*
    private NativeArray<int> m_CompanyDemand;
    // 0 - low res
    // 1 - med res
    // 2 - high res
    // 3 - commercial (CommercialDemandSystem.m_BuildingDemand)
    // 4 - industry (IndustrialDemandSystem.m_IndustrialCompanyDemand)
    // 5 - storage (IndustrialDemandSystem.m_StorageCompanyDemand)
    // 6 - office (IndustrialDemandSystem.m_OfficeCompanyDemand)
    */

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        // get access to other systems
        m_SimulationSystem = base.World.GetOrCreateSystemManaged<SimulationSystem>();
        m_ResidentialDemandSystem = base.World.GetOrCreateSystemManaged<ResidentialDemandSystem>();
        m_CommercialDemandSystem = base.World.GetOrCreateSystemManaged<CommercialDemandSystem>();
        m_IndustrialDemandSystem = base.World.GetOrCreateSystemManaged<IndustrialDemandSystem>();

        // ui binding doe building demand data
        AddBinding(m_uiBuildingDemand = new RawValueBinding("cityInfo", "ilBuildingDemand", delegate (IJsonWriter binder)
        {
            binder.ArrayBegin(m_BuildingDemand.Length);
            for (int i = 0; i < m_BuildingDemand.Length; i++)
                binder.Write(m_BuildingDemand[i]);
            binder.ArrayEnd();
        }));

        // allocate storage
        m_BuildingDemand = new NativeArray<int>(7, Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        if (m_SimulationSystem.frameIndex % 128 != 77)
            return;

        //Plugin.Log($"OnUpdate at frame {m_SimulationSystem.frameIndex}");
        base.OnUpdate();

        m_BuildingDemand[0] = m_ResidentialDemandSystem.buildingDemand.z;
        m_BuildingDemand[1] = m_ResidentialDemandSystem.buildingDemand.y;
        m_BuildingDemand[2] = m_ResidentialDemandSystem.buildingDemand.x;
        m_BuildingDemand[3] = m_CommercialDemandSystem.buildingDemand;
        m_BuildingDemand[4] = m_IndustrialDemandSystem.industrialBuildingDemand;
        m_BuildingDemand[5] = m_IndustrialDemandSystem.storageBuildingDemand;
        m_BuildingDemand[6] = m_IndustrialDemandSystem.officeBuildingDemand;

        m_uiBuildingDemand.Update();
    }

    [Preserve]
    protected override void OnDestroy()
    {
        m_BuildingDemand.Dispose();
        base.OnDestroy();
    }

}
