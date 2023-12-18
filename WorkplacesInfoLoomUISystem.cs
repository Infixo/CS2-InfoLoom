using System.Runtime.CompilerServices;
using Colossal.UI.Binding;
using Game.Buildings;
using Game.Common;
using Game.Companies;
using Game.Prefabs;
using Game.Tools;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Scripting;
using Game;
using Game.UI.InGame;

namespace InfoLoom;

// This system is based on game's WorkplacesInfoviewUISystem

[CompilerGenerated]
public class WorkplacesInfoLoomUISystem : InfoviewUISystemBase
{
    private enum Result
    {
        Workplaces,
        Employees,
        Count
    }

    [BurstCompile]
    private struct CalculateWorkplaceDataJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityHandle;

        [ReadOnly]
        public BufferTypeHandle<Employee> m_EmployeeHandle;

        [ReadOnly]
        public ComponentTypeHandle<WorkProvider> m_WorkProviderHandle;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> m_PropertyRenterHandle;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> m_PrefabRefHandle;

        [ReadOnly]
        public ComponentLookup<PrefabRef> m_PrefabRefFromEntity;

        [ReadOnly]
        public ComponentLookup<WorkplaceData> m_WorkplaceDataFromEntity;

        [ReadOnly]
        public ComponentLookup<SpawnableBuildingData> m_SpawnableBuildingFromEntity;

        public NativeArray<int> m_IntResults;

        public NativeArray<EmploymentData> m_EmploymentDataResults;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray = chunk.GetNativeArray(m_EntityHandle);
            NativeArray<PrefabRef> nativeArray2 = chunk.GetNativeArray(ref m_PrefabRefHandle);
            NativeArray<PropertyRenter> nativeArray3 = chunk.GetNativeArray(ref m_PropertyRenterHandle);
            NativeArray<WorkProvider> nativeArray4 = chunk.GetNativeArray(ref m_WorkProviderHandle);
            BufferAccessor<Employee> bufferAccessor = chunk.GetBufferAccessor(ref m_EmployeeHandle);
            for (int i = 0; i < nativeArray.Length; i++)
            {
                int buildingLevel = 1;
                WorkProvider workProvider = nativeArray4[i];
                DynamicBuffer<Employee> employees = bufferAccessor[i];
                PrefabRef prefabRef = nativeArray2[i];
                WorkplaceData workplaceData = m_WorkplaceDataFromEntity[prefabRef.m_Prefab];
                if (chunk.Has(ref m_PropertyRenterHandle))
                {
                    PropertyRenter propertyRenter = nativeArray3[i];
                    PrefabRef prefabRef2 = m_PrefabRefFromEntity[propertyRenter.m_Property];
                    if (m_SpawnableBuildingFromEntity.HasComponent(prefabRef2.m_Prefab))
                    {
                        buildingLevel = m_SpawnableBuildingFromEntity[prefabRef2.m_Prefab].m_Level;
                    }
                }
                EmploymentData workplacesData = EmploymentData.GetWorkplacesData(workProvider.m_MaxWorkers, buildingLevel, workplaceData.m_Complexity);
                EmploymentData employeesData = EmploymentData.GetEmployeesData(employees, workplacesData.total - employees.Length);
                m_IntResults[0] += workplacesData.total;
                m_IntResults[1] += employees.Length;
                m_EmploymentDataResults[0] += workplacesData;
                m_EmploymentDataResults[1] += employeesData;
            }
        }

        void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
        }
    }

    private struct TypeHandle
    {
        [ReadOnly]
        public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

        [ReadOnly]
        public BufferTypeHandle<Employee> __Game_Companies_Employee_RO_BufferTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<WorkProvider> __Game_Companies_WorkProvider_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<WorkplaceData> __Game_Prefabs_WorkplaceData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<SpawnableBuildingData> __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Companies_Employee_RO_BufferTypeHandle = state.GetBufferTypeHandle<Employee>(isReadOnly: true);
            __Game_Companies_WorkProvider_RO_ComponentTypeHandle = state.GetComponentTypeHandle<WorkProvider>(isReadOnly: true);
            __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PropertyRenter>(isReadOnly: true);
            __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(isReadOnly: true);
            __Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(isReadOnly: true);
            __Game_Prefabs_WorkplaceData_RO_ComponentLookup = state.GetComponentLookup<WorkplaceData>(isReadOnly: true);
            __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup = state.GetComponentLookup<SpawnableBuildingData>(isReadOnly: true);
        }
    }

    private const string kGroup = "workplaces";

    private EntityQuery m_WorkplaceQuery;

    private EntityQuery m_WorkplaceModifiedQuery;

    private GetterValueBinding<EmploymentData> m_EmployeesData;

    private GetterValueBinding<EmploymentData> m_WorkplacesData;

    private GetterValueBinding<int> m_Workplaces;

    private GetterValueBinding<int> m_Workers;

    private NativeArray<int> m_IntResults;

    private NativeArray<EmploymentData> m_EmploymentDataResults;

    private TypeHandle __TypeHandle;

    protected override bool Active
    {
        get
        {
            if (!base.Active && !m_EmployeesData.active)
            {
                return m_WorkplacesData.active;
            }
            return true;
        }
    }

    protected override bool Modified => !m_WorkplaceModifiedQuery.IsEmptyIgnoreFilter;

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        m_WorkplaceQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[3]
            {
                ComponentType.ReadOnly<Employee>(),
                ComponentType.ReadOnly<WorkProvider>(),
                ComponentType.ReadOnly<PrefabRef>()
            },
            Any = new ComponentType[2]
            {
                ComponentType.ReadOnly<PropertyRenter>(),
                ComponentType.ReadOnly<Building>()
            },
            None = new ComponentType[1] { ComponentType.ReadOnly<Temp>() }
        });
        m_WorkplaceModifiedQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[3]
            {
                ComponentType.ReadOnly<Employee>(),
                ComponentType.ReadOnly<WorkProvider>(),
                ComponentType.ReadOnly<PrefabRef>()
            },
            Any = new ComponentType[3]
            {
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Created>(),
                ComponentType.ReadOnly<Updated>()
            },
            None = new ComponentType[1] { ComponentType.ReadOnly<Temp>() }
        });
        m_IntResults = new NativeArray<int>(2, Allocator.Persistent);
        m_EmploymentDataResults = new NativeArray<EmploymentData>(2, Allocator.Persistent);
        AddBinding(m_WorkplacesData = new GetterValueBinding<EmploymentData>("workplaces", "workplacesData", () => (!m_EmploymentDataResults.IsCreated || m_EmploymentDataResults.Length != 2) ? default(EmploymentData) : m_EmploymentDataResults[0], new ValueWriter<EmploymentData>()));
        AddBinding(m_EmployeesData = new GetterValueBinding<EmploymentData>("workplaces", "employeesData", () => (!m_EmploymentDataResults.IsCreated || m_EmploymentDataResults.Length != 2) ? default(EmploymentData) : m_EmploymentDataResults[1], new ValueWriter<EmploymentData>()));
        AddBinding(m_Workplaces = new GetterValueBinding<int>("workplaces", "workplaces", () => (m_IntResults.IsCreated && m_IntResults.Length == 2) ? m_IntResults[0] : 0));
        AddBinding(m_Workers = new GetterValueBinding<int>("workplaces", "employees", () => (m_IntResults.IsCreated && m_IntResults.Length == 2) ? m_IntResults[1] : 0));
    }

    [Preserve]
    protected override void OnDestroy()
    {
        m_IntResults.Dispose();
        m_EmploymentDataResults.Dispose();
        base.OnDestroy();
    }

    protected override void PerformUpdate()
    {
        ResetResults();
        __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_WorkplaceData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Companies_WorkProvider_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Companies_Employee_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
        CalculateWorkplaceDataJob jobData = default(CalculateWorkplaceDataJob);
        jobData.m_EntityHandle = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
        jobData.m_EmployeeHandle = __TypeHandle.__Game_Companies_Employee_RO_BufferTypeHandle;
        jobData.m_WorkProviderHandle = __TypeHandle.__Game_Companies_WorkProvider_RO_ComponentTypeHandle;
        jobData.m_PropertyRenterHandle = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;
        jobData.m_PrefabRefHandle = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
        jobData.m_PrefabRefFromEntity = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup;
        jobData.m_WorkplaceDataFromEntity = __TypeHandle.__Game_Prefabs_WorkplaceData_RO_ComponentLookup;
        jobData.m_SpawnableBuildingFromEntity = __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
        jobData.m_IntResults = m_IntResults;
        jobData.m_EmploymentDataResults = m_EmploymentDataResults;
        JobChunkExtensions.Schedule(jobData, m_WorkplaceQuery, base.Dependency).Complete();
        m_EmployeesData.Update();
        m_WorkplacesData.Update();
        m_Workplaces.Update();
        m_Workers.Update();
    }

    private void ResetResults()
    {
        for (int i = 0; i < 2; i++)
        {
            m_EmploymentDataResults[i] = default(EmploymentData);
            m_IntResults[i] = 0;
        }
    }

    private int GetWorkplaces()
    {
        if (!m_IntResults.IsCreated || m_IntResults.Length != 2)
        {
            return 0;
        }
        return m_IntResults[0];
    }

    private int GetWorkers()
    {
        if (!m_IntResults.IsCreated || m_IntResults.Length != 2)
        {
            return 0;
        }
        return m_IntResults[1];
    }

    private EmploymentData GetWorkplacesData()
    {
        if (!m_EmploymentDataResults.IsCreated || m_EmploymentDataResults.Length != 2)
        {
            return default(EmploymentData);
        }
        return m_EmploymentDataResults[0];
    }

    private EmploymentData GetEmployeesData()
    {
        if (!m_EmploymentDataResults.IsCreated || m_EmploymentDataResults.Length != 2)
        {
            return default(EmploymentData);
        }
        return m_EmploymentDataResults[1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void __AssignQueries(ref SystemState state)
    {
    }

    protected override void OnCreateForCompiler()
    {
        base.OnCreateForCompiler();
        __AssignQueries(ref base.CheckedStateRef);
        __TypeHandle.__AssignHandles(ref base.CheckedStateRef);
    }

    [Preserve]
    public WorkplacesInfoLoomUISystem()
    {
    }
}
