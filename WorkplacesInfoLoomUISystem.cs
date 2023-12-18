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
using Game.UI;
using Game.Simulation; // TODO: use UIUpdateState and Advance() eventully...
using Game.UI.InGame; // EmploymentData TODO: remove when not used
using Game.Economy; // Resources

namespace InfoLoom;

// This system is based on game's WorkplacesInfoviewUISystem

[CompilerGenerated]
public class WorkplacesInfoLoomUISystem : UISystemBase
{
    private struct WorkplacesAtLevelInfo
    {
        public int Level;
        public int Total; // assertions: Total=Service+Commercial+Industry+Office, Total=Employee+Empty
        public int Service; // jobs in city services
        public int Commercial; // jobs in commercial zones
        public int Leisure; // jobs in commercial zones
        public int Extractor; // jobs in industrial zones
        public int Industrial; // jobs in industrial zones
        public int Office; // jobs in office zones
        public int Employee; // employees
        public int Open; // open positions
        public WorkplacesAtLevelInfo(int _level) { Level = _level; }
    }
    private static void WriteData(IJsonWriter writer, WorkplacesAtLevelInfo info)
    {
        writer.TypeBegin("workplacesAtLevelInfo");
        writer.PropertyName("level");
        writer.Write(info.Level);
        writer.PropertyName("total");
        writer.Write(info.Total);
        writer.PropertyName("service");
        writer.Write(info.Service);
        writer.PropertyName("commercial");
        writer.Write(info.Commercial);
        writer.PropertyName("leisure");
        writer.Write(info.Leisure);
        writer.PropertyName("extractor");
        writer.Write(info.Extractor);
        writer.PropertyName("industry");
        writer.Write(info.Industrial);
        writer.PropertyName("office");
        writer.Write(info.Office);
        writer.PropertyName("employee");
        writer.Write(info.Employee);
        writer.PropertyName("open");
        writer.Write(info.Open);
        writer.TypeEnd();
    }

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
        public BufferTypeHandle<Resources> m_ResourcesHandle;

        [ReadOnly]
        public ComponentTypeHandle<WorkProvider> m_WorkProviderHandle;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> m_PropertyRenterHandle;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> m_PrefabRefHandle;

        [ReadOnly]
        public ComponentTypeHandle<IndustrialCompany> m_IndustrialCompanyHandle;

        [ReadOnly]
        public ComponentTypeHandle<Game.Companies.ExtractorCompany> m_ExtractorCompanyHandle;

        [ReadOnly]
        public ComponentTypeHandle<CommercialCompany> m_CommercialCompanyHandle;

        [ReadOnly]
        public ComponentLookup<PrefabRef> m_PrefabRefFromEntity;

        [ReadOnly]
        public ComponentLookup<WorkplaceData> m_WorkplaceDataFromEntity;

        [ReadOnly]
        public ComponentLookup<SpawnableBuildingData> m_SpawnableBuildingFromEntity;

        [ReadOnly]
        public ComponentLookup<ResourceData> m_ResourceDatas; // this is for future use to access resource prefab data

        [ReadOnly]
        public ResourcePrefabs m_ResourcePrefabs;

        public NativeArray<int> m_IntResults;

        public NativeArray<EmploymentData> m_EmploymentDataResults;

        public NativeArray<WorkplacesAtLevelInfo> m_Results;

        // notes on offices - they are detected usually by resource weight :(
        //ResourceData resourceData2 = m_ResourceDatas[m_ResourcePrefabs[iterator.resource]];
        //bool flag4 = resourceData2.m_Weight == 0f;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> nativeArray = chunk.GetNativeArray(m_EntityHandle);
            NativeArray<PrefabRef> nativeArray2 = chunk.GetNativeArray(ref m_PrefabRefHandle);
            NativeArray<PropertyRenter> nativeArray3 = chunk.GetNativeArray(ref m_PropertyRenterHandle);
            NativeArray<WorkProvider> nativeArray4 = chunk.GetNativeArray(ref m_WorkProviderHandle);
            BufferAccessor<Employee> bufferAccessor = chunk.GetBufferAccessor(ref m_EmployeeHandle);
            BufferAccessor<Resources> bufferResources = chunk.GetBufferAccessor(ref m_ResourcesHandle);
            bool isExtractor = chunk.Has(ref m_ExtractorCompanyHandle);
            bool isIndustrial = chunk.Has(ref m_IndustrialCompanyHandle);
            bool isCommercial = chunk.Has(ref m_CommercialCompanyHandle);
            bool isService = !(isIndustrial || isCommercial);
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
                EmploymentData workplacesData = EmploymentData.GetWorkplacesData(workProvider.m_MaxWorkers, buildingLevel, workplaceData.m_Complexity); // this holds workplaces for each level
                EmploymentData employeesData = EmploymentData.GetEmployeesData(employees, workplacesData.total - employees.Length); // this holds employees for each level
                m_IntResults[0] += workplacesData.total;
                m_IntResults[1] += employees.Length;
                m_EmploymentDataResults[0] += workplacesData;
                m_EmploymentDataResults[1] += employeesData;

                // Office detection
                // This is a bit hacky approach based on produced resource
                // The game uses resource weight to distinguish Office companies from Industrial ones
                // It is however retrieved from Resource Prefab (ResourceData.m_Weight)
                // Here I am doing 2 simplifications until I'll learn a better way to do it
                // a) I am assuming that the last resource in the buffer is the one produced; Idk if this holds always
                // b) I don't retrieve prefab, just check for Software, Media, Telecom and Financial; there won't be new resources any time soon, so...
                DynamicBuffer<Resources> resources = bufferResources[i];
                //string res = "";
                bool isOffice = false;
                bool isMoney = false;
                Resource lastRes = Resource.NoResource;
                if (resources.Length > 0)
                {
                    lastRes = resources[resources.Length - 1].m_Resource;
                    //res += $" {compRes.m_Resource} {compRes.m_Amount}";
                    isOffice = (lastRes == Resource.Software || lastRes == Resource.Telecom || lastRes == Resource.Financial || lastRes == Resource.Media);
                    isMoney = lastRes == Resource.Money;
                }
                //Plugin.Log($"company {isCommercial}/{isMoney}/{isIndustrial}/{isExtractor}/{isOffice}:{lastRes} " +
                //    $"{workplacesData.uneducated} {workplacesData.poorlyEducated} {workplacesData.educated} {workplacesData.wellEducated} {workplacesData.highlyEducated}");

                WorkplacesAtLevelInfo ProcessLevel(WorkplacesAtLevelInfo info, int workplaces, int employees)
                {
                    //WorkplacesAtLevelInfo info0 = m_Results[level];
                    info.Total += workplaces;
                    if (isService) info.Service += workplaces;
                    if (isCommercial)
                    {
                        if (isMoney) info.Commercial += workplaces;
                        else info.Leisure += workplaces;
                    }
                    if (isIndustrial)
                    {
                        if (isExtractor) info.Extractor += workplaces;
                        else if(isOffice) info.Office += workplaces;
                        else info.Industrial += workplaces;
                    }
                    info.Employee += employees;
                    info.Open += workplaces - employees;
                    return info;
                }

                // Work with a local variable to avoid CS0206 error

                // uneducated
                /*
                WorkplacesAtLevelInfo info0 = m_Results[0];
                info0.Total += workplacesData.uneducated;
                if (isService) info0.Service += workplacesData.uneducated;
                if (isCommercial) info0.Commercial += workplacesData.uneducated;
                if (isIndustrial) info0.Industrial += workplacesData.uneducated;
                info0.Employee += employeesData.uneducated;
                info0.Open += workplacesData.uneducated - employeesData.uneducated;
                m_Results[0] = info0;
                */
                m_Results[0] = ProcessLevel(m_Results[0], workplacesData.uneducated, employeesData.uneducated);

                // poorlyEducated
                /*
                WorkplacesAtLevelInfo info1 = m_Results[1]; 
                info1.Total += workplacesData.poorlyEducated;
                if (isService) info1.Service += workplacesData.poorlyEducated;
                if (isCommercial) info1.Commercial += workplacesData.poorlyEducated;
                if (isIndustrial) info1.Industrial += workplacesData.poorlyEducated;
                info1.Employee += employeesData.poorlyEducated;
                info1.Open += workplacesData.poorlyEducated - employeesData.poorlyEducated;
                m_Results[1] = info1;
                */
                m_Results[1] = ProcessLevel(m_Results[1], workplacesData.poorlyEducated, employeesData.poorlyEducated);

                // educated
                /*
                WorkplacesAtLevelInfo info2 = m_Results[2];
                info2.Total += workplacesData.educated;
                if (isService) info2.Service += workplacesData.educated;
                if (isCommercial) info2.Commercial += workplacesData.educated;
                if (isIndustrial) info2.Industrial += workplacesData.educated;
                info2.Employee += employeesData.educated;
                info2.Open += workplacesData.educated - employeesData.educated;
                m_Results[2] = info2;
                */
                m_Results[2] = ProcessLevel(m_Results[2], workplacesData.educated, employeesData.educated);

                // wellEducated
                /*
                WorkplacesAtLevelInfo info3 = m_Results[3]; 
                info3.Total += workplacesData.wellEducated;
                if (isService) info3.Service += workplacesData.wellEducated;
                if (isCommercial) info3.Commercial += workplacesData.wellEducated;
                if (isIndustrial) info3.Industrial += workplacesData.wellEducated;
                info3.Employee += employeesData.wellEducated;
                info3.Open += workplacesData.wellEducated - employeesData.wellEducated;
                m_Results[3] = info3;
                */
                m_Results[3] = ProcessLevel(m_Results[3], workplacesData.wellEducated, employeesData.wellEducated);

                // highlyEducated
                /*
                WorkplacesAtLevelInfo info4 = m_Results[4]; 
                info4.Total += workplacesData.highlyEducated;
                if (isService) info4.Service += workplacesData.highlyEducated;
                if (isCommercial) info4.Commercial += workplacesData.highlyEducated;
                if (isIndustrial) info4.Industrial += workplacesData.highlyEducated;
                info4.Employee += employeesData.highlyEducated;
                info4.Open += workplacesData.highlyEducated - employeesData.highlyEducated;
                m_Results[4] = info4;
                */
                m_Results[4] = ProcessLevel(m_Results[4], workplacesData.highlyEducated, employeesData.highlyEducated);
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
        public BufferTypeHandle<Resources> __Game_Economy_Resources_RO_BufferTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<WorkProvider> __Game_Companies_WorkProvider_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Game.Companies.ExtractorCompany> __Game_Companies_ExtractorCompany_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<IndustrialCompany> __Game_Companies_IndustrialCompany_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<CommercialCompany> __Game_Companies_CommercialCompany_RO_ComponentTypeHandle;

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
            __Game_Economy_Resources_RO_BufferTypeHandle = state.GetBufferTypeHandle<Resources>(isReadOnly: true);
            __Game_Companies_WorkProvider_RO_ComponentTypeHandle = state.GetComponentTypeHandle<WorkProvider>(isReadOnly: true);
            __Game_Companies_ExtractorCompany_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Companies.ExtractorCompany>(isReadOnly: true);
            __Game_Companies_IndustrialCompany_RO_ComponentTypeHandle = state.GetComponentTypeHandle<IndustrialCompany>(isReadOnly: true);
            __Game_Companies_CommercialCompany_RO_ComponentTypeHandle = state.GetComponentTypeHandle<CommercialCompany>(isReadOnly: true);
            __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PropertyRenter>(isReadOnly: true);
            __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(isReadOnly: true);
            __Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(isReadOnly: true);
            __Game_Prefabs_WorkplaceData_RO_ComponentLookup = state.GetComponentLookup<WorkplaceData>(isReadOnly: true);
            __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup = state.GetComponentLookup<SpawnableBuildingData>(isReadOnly: true);
        }
    }

    private const string kGroup = "workplaces";

    private SimulationSystem m_SimulationSystem;

    private ResourceSystem m_ResourceSystem;

    private EntityQuery m_WorkplaceQuery;

    private EntityQuery m_WorkplaceModifiedQuery;

    //private GetterValueBinding<EmploymentData> m_EmployeesData;

    //private GetterValueBinding<EmploymentData> m_WorkplacesData;

    //private GetterValueBinding<int> m_Workplaces;

    //private GetterValueBinding<int> m_Workers;

    private RawValueBinding m_uiResults;

    private NativeArray<int> m_IntResults;

    private NativeArray<EmploymentData> m_EmploymentDataResults;

    private NativeArray<WorkplacesAtLevelInfo> m_Results;

    private TypeHandle __TypeHandle;

    /* not used
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
    */

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();

        m_SimulationSystem = base.World.GetOrCreateSystemManaged<SimulationSystem>(); // TODO: use UIUpdateState eventually
        m_ResourceSystem = base.World.GetOrCreateSystemManaged<ResourceSystem>();

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
        m_Results = new NativeArray<WorkplacesAtLevelInfo>(6, Allocator.Persistent); // there are 5 education levels + 1 for totals
        //AddBinding(m_WorkplacesData = new GetterValueBinding<EmploymentData>(kGroup, "ilWorkplacesData", () => (!m_EmploymentDataResults.IsCreated || m_EmploymentDataResults.Length != 2) ? default(EmploymentData) : m_EmploymentDataResults[0], new ValueWriter<EmploymentData>()));
        //AddBinding(m_EmployeesData = new GetterValueBinding<EmploymentData>(kGroup, "ilEmployeesData", () => (!m_EmploymentDataResults.IsCreated || m_EmploymentDataResults.Length != 2) ? default(EmploymentData) : m_EmploymentDataResults[1], new ValueWriter<EmploymentData>()));
        //AddBinding(m_Workplaces = new GetterValueBinding<int>(kGroup, "ilWorkplaces", () => (m_IntResults.IsCreated && m_IntResults.Length == 2) ? m_IntResults[0] : 0));
        //AddBinding(m_Workers = new GetterValueBinding<int>(kGroup, "ilEmployees", () => (m_IntResults.IsCreated && m_IntResults.Length == 2) ? m_IntResults[1] : 0));

        AddBinding(m_uiResults = new RawValueBinding(kGroup, "ilWorkplaces", delegate (IJsonWriter binder)
        {
            binder.ArrayBegin(m_Results.Length);
            for (int i = 0; i < m_Results.Length; i++)
                WriteData(binder, m_Results[i]);
            binder.ArrayEnd();
        }));

        Plugin.Log("WorkplacesInfoLoomUISystem created.");
    }

    [Preserve]
    protected override void OnDestroy()
    {
        m_IntResults.Dispose();
        m_EmploymentDataResults.Dispose();
        m_Results.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate() // original: PerformUpdate()
    {
        if (m_SimulationSystem.frameIndex % 128 != 77)
            return;

        ResetResults();

        // update handles
        __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_WorkplaceData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Companies_WorkProvider_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Companies_ExtractorCompany_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Companies_IndustrialCompany_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Companies_CommercialCompany_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Companies_Employee_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Economy_Resources_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);

        // prepare and schedule job
        CalculateWorkplaceDataJob jobData = default(CalculateWorkplaceDataJob);
        jobData.m_EntityHandle = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
        jobData.m_EmployeeHandle = __TypeHandle.__Game_Companies_Employee_RO_BufferTypeHandle;
        jobData.m_ResourcesHandle = __TypeHandle.__Game_Economy_Resources_RO_BufferTypeHandle; // Game.Economy.Resources
        jobData.m_WorkProviderHandle = __TypeHandle.__Game_Companies_WorkProvider_RO_ComponentTypeHandle;
        jobData.m_PropertyRenterHandle = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;
        jobData.m_ExtractorCompanyHandle = __TypeHandle.__Game_Companies_ExtractorCompany_RO_ComponentTypeHandle;
        jobData.m_IndustrialCompanyHandle = __TypeHandle.__Game_Companies_IndustrialCompany_RO_ComponentTypeHandle; // Game.Companies.IndustrialCompany
        jobData.m_CommercialCompanyHandle = __TypeHandle.__Game_Companies_CommercialCompany_RO_ComponentTypeHandle;
        jobData.m_PrefabRefHandle = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
        jobData.m_PrefabRefFromEntity = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup;
        jobData.m_WorkplaceDataFromEntity = __TypeHandle.__Game_Prefabs_WorkplaceData_RO_ComponentLookup;
        jobData.m_SpawnableBuildingFromEntity = __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
        jobData.m_IntResults = m_IntResults;
        jobData.m_EmploymentDataResults = m_EmploymentDataResults;
        jobData.m_Results = m_Results;
        jobData.m_ResourcePrefabs = m_ResourceSystem.GetPrefabs();
        JobChunkExtensions.Schedule(jobData, m_WorkplaceQuery, base.Dependency).Complete();

        // calculate totals
        WorkplacesAtLevelInfo totals = new WorkplacesAtLevelInfo(-1);
        for (int i = 0; i < 5; i++)
        {
            totals.Total += m_Results[i].Total;
            totals.Service += m_Results[i].Service;
            totals.Commercial += m_Results[i].Commercial;
            totals.Leisure += m_Results[i].Leisure;
            totals.Extractor += m_Results[i].Extractor;
            totals.Industrial += m_Results[i].Industrial;
            totals.Office += m_Results[i].Office;
            totals.Employee += m_Results[i].Employee;
            totals.Open += m_Results[i].Open;
        }
        m_Results[5] = totals;
        // update ui bindings
        //m_EmployeesData.Update();
        //m_WorkplacesData.Update();
        //m_Workplaces.Update();
        //m_Workers.Update();
        m_uiResults.Update();
        // DEBUG
        //Utils.InspectComponentsInChunks(EntityManager, m_WorkplaceQuery, "JOBS");
    }

    private void ResetResults()
    {
        for (int i = 0; i < 2; i++)
        {
            m_EmploymentDataResults[i] = default(EmploymentData);
            m_IntResults[i] = 0;
        }
        for (int i = 0; i < 6; i++) // there are 5 education levels + 1 for totals
        {
            m_Results[i] = new WorkplacesAtLevelInfo(i);
        }
    }

    /* not used
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
    */

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
