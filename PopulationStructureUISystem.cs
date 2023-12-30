using System.Runtime.CompilerServices;
using Colossal.UI.Binding;
using Game.Agents;
using Game.Citizens;
using Game.Common;
using Game.Simulation;
using Game.Tools;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Scripting;
using Game.UI;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System;
using Game.Buildings;

namespace InfoLoom;

// This System is based on PopulationInfoviewUISystem by CO
[CompilerGenerated]
public class PopulationStructureUISystem : UISystemBase
{
    /// <summary>
    /// Holds info about population at Age
    /// </summary>
    private struct PopulationAtAgeInfo
    {
        public int Age;
        public int Total; // asserion: Total is a sum of the below parts
        public int School1; // elementary school
        public int School2; // high school
        public int School3; // college
        public int School4; // university
        public int Work; // working
        public int Other; // not working, not student
        public PopulationAtAgeInfo(int _age) { Age = _age; }
    }

    private static void WriteData(IJsonWriter writer, PopulationAtAgeInfo info)
    {
        writer.TypeBegin("populationAtAgeInfo");
        writer.PropertyName("age");
        writer.Write(info.Age);
        writer.PropertyName("total");
        writer.Write(info.Total);
        writer.PropertyName("school1");
        writer.Write(info.School1);
        writer.PropertyName("school2");
        writer.Write(info.School2);
        writer.PropertyName("school3");
        writer.Write(info.School3);
        writer.PropertyName("school4");
        writer.Write(info.School4);
        writer.PropertyName("work");
        writer.Write(info.Work);
        writer.PropertyName("other");
        writer.Write(info.Other);
        writer.TypeEnd();
    }

    [BurstCompile]
    private struct PopulationStructureJob : IJobChunk
    {
        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<Citizen> m_CitizenType;

        [ReadOnly]
        public ComponentTypeHandle<Game.Citizens.Student> m_StudentType;

        [ReadOnly]
        public ComponentTypeHandle<Worker> m_WorkerType;

        //[ReadOnly]
        //public BufferTypeHandle<HouseholdCitizen> m_HouseholdCitizenHandle;

        //[ReadOnly]
        //public ComponentTypeHandle<Household> m_HouseholdType;

        [ReadOnly]
        public ComponentTypeHandle<HealthProblem> m_HealthProblemType;

        [ReadOnly]
        public ComponentTypeHandle<HouseholdMember> m_HouseholdMemberType;

        //[ReadOnly]
        //public ComponentLookup<Worker> m_WorkerFromEntity;

        //[ReadOnly]
        //public ComponentLookup<HealthProblem> m_HealthProblems;

        [ReadOnly]
        public ComponentLookup<MovingAway> m_MovingAways;

        //[ReadOnly]
        //public ComponentLookup<Citizen> m_Citizens;

        //[ReadOnly]
        //public ComponentLookup<Student> m_Students;

        [ReadOnly]
        public ComponentLookup<Household> m_Households;

        [ReadOnly]
        public ComponentLookup<HomelessHousehold> m_HomelessHouseholds;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> m_PropertyRenters;

        public TimeData m_TimeData;

        //public uint m_UpdateFrameIndex;
        
        public uint m_SimulationFrame;

        public NativeArray<int> m_Totals;

        public NativeArray<PopulationAtAgeInfo> m_Results;

        // this job is based on AgingJob
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            //PopulationStructureUISystem.LogChunk(chunk);
            // access data in the chunk
            NativeArray<Entity> entities = chunk.GetNativeArray(m_EntityType);
            NativeArray<Citizen> citizenArray = chunk.GetNativeArray(ref m_CitizenType);
            NativeArray<Game.Citizens.Student> studentArray = chunk.GetNativeArray(ref m_StudentType);
            NativeArray<Worker> workerArray = chunk.GetNativeArray(ref m_WorkerType);
            NativeArray<HealthProblem> healthProblemArray = chunk.GetNativeArray(ref m_HealthProblemType);
            NativeArray<HouseholdMember> householdMemberArray = chunk.GetNativeArray(ref m_HouseholdMemberType);
            bool isStudent = chunk.Has(ref m_StudentType); // are there students in this chunk?
            bool isWorker = chunk.Has(ref m_WorkerType); // are there workers in this chunk?
            bool isHealthProblem = chunk.Has(ref m_HealthProblemType); // for checking dead cims
            int day = TimeSystem.GetDay(m_SimulationFrame, m_TimeData);
            //Plugin.Log($"day {day} chunk: {entities.Length} entities, {citizens.Length} citizens, {isStudent} {students.Length} students, {isWorker} {workers.Length} workers");

            for (int i = 0; i < citizenArray.Length; i++)
            {
                Entity entity = entities[i];
                //Plugin.Log($"{entity}");
                //List<ComponentType> list = PopulationStructureUISystem.ListEntityComponents();
                Citizen citizen = citizenArray[i];
                Entity household = householdMemberArray[i].m_Household;

                // skip: non-existing households (technical), and with flag not set
                if (!m_Households.HasComponent(household) || m_Households[household].m_Flags == HouseholdFlags.None)
                {
                    continue;
                }

                // skip but count dead citizens
                if (isHealthProblem && CitizenUtils.IsDead(healthProblemArray[i]))
                {
                    m_Totals[8]++; // dead
                    continue;
                }

                // citizen data
                bool isCommuter = ((citizen.m_State & CitizenFlags.Commuter) != CitizenFlags.None);
                bool isTourist = ((citizen.m_State & CitizenFlags.Tourist) != CitizenFlags.None);
                bool isMovedIn = ((m_Households[household].m_Flags & HouseholdFlags.MovedIn) != HouseholdFlags.None);

                // are components in sync with flags?
                //if (isTourist || m_TouristHouseholds.HasComponent(household))
                //Plugin.Log($"{entity.Index}: tourist {isTourist} {isTouristHousehold} {m_Households[household].m_Flags}");
                //if (isCommuter || m_CommuterHouseholds.HasComponent(household))
                //Plugin.Log($"{entity.Index}: commuter {isCommuter} {isCommuterHousehold} {m_Households[household].m_Flags}");
                // Infixo: notes for the future
                // Tourists: citizen flag is always set, component sometimes exists, sometimes not
                //           most of them don't have MovedIn flag set, just Tourist flag in household
                //           usually Tourist household flag is correlated with TouristHousehold component, but NOT always
                //           MovedIn tourists DON'T have TouristHousehold component - why??? where do they stay?
                //           tl;dr CitizenFlags.Tourist is the only reliable way
                // Commuters: very similar logic, CitizenFlag is always SET
                //            CommuterHousehold component is present when household flag is Commuter
                //                                        is not present when flag is MovedIn

                // count All, Tourists and Commuters
                m_Totals[0]++; // all
                if (isTourist) m_Totals[2]++; // tourists
                else if (isCommuter) m_Totals[3]++; // commuters
                if (isTourist || isCommuter)
                    continue; // not local, go for the next

                // skip but count citizens moving away
                // 231230 moved after Tourist & Commuter check, so it will show only Locals that are moving away (more important info)
                if (m_MovingAways.HasComponent(household))
                {
                    m_Totals[7]++; // moving aways
                    continue;
                }

                // finally, count local population
                if (isMovedIn) m_Totals[1]++; // locals; game Population is: MovedIn, not Tourist & Commuter, not dead
                else
                {
                    // skip glitches e.g. there is a case with Tourist household, but citizen is NOT Tourist
                    //Plugin.Log($"Warning: unknown citizen {citizen.m_State} household {m_Households[household].m_Flags}");
                    continue;
                }

                // homeless citizens, already MovingAway are not included (they are gone, anyway)
                if (m_HomelessHouseholds.HasComponent(household) || !m_PropertyRenters.HasComponent(household))
                {
                    m_Totals[9]++; // homeless
                }

                // get age
                int ageInDays = day - citizen.m_BirthDay;
                if (ageInDays > m_Totals[6]) m_Totals[6] = ageInDays; // oldest cim
                //ageInDays = ageInDays/2; // INFIXO: TODO
                if (ageInDays>109) ageInDays = 109;
                PopulationAtAgeInfo info = m_Results[ageInDays];
                // process at-age info
                info.Total++;
                // process a student
                if (isStudent)
                {
                    m_Totals[4]++; // students
                    Game.Citizens.Student student = studentArray[i];
                    // check what school level
                    switch (studentArray[i].m_Level)
                    {
                        case 1: info.School1++; break;
                        case 2: info.School2++; break;
                        case 3: info.School3++; break;
                        case 4: info.School4++; break;
                        default:
                            Plugin.Log($"WARNING: incorrect school level, {studentArray[i].m_Level}");
                            break;
                    }
                }
                // process a worker
                if (isWorker)
                {
                    // we can check the level here? what is it actually?
                    m_Totals[5]++; // workers
                    info.Work++;
                }
                // not a student, not a worker
                if (!isStudent && !isWorker)
                {
                    info.Other++;
                }
                m_Results[ageInDays] = info;
                //Plugin.Log($"{ageInDays} yo, school {isStudent}, work {isWorker}");
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
        public ComponentTypeHandle<Game.Citizens.Citizen> __Game_Citizens_Citizen_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Game.Citizens.Student> __Game_Citizens_Student_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Game.Citizens.Worker> __Game_Citizens_Worker_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<HealthProblem> __Game_Citizens_HealthProblem_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<HouseholdMember> __Game_Citizens_HouseholdMember_RO_ComponentTypeHandle;

        //public BufferTypeHandle<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RW_BufferTypeHandle;

        //public ComponentTypeHandle<Household> __Game_Citizens_Household_RW_ComponentTypeHandle;

        //[ReadOnly]
        //public ComponentLookup<Worker> __Game_Citizens_Worker_RO_ComponentLookup;

        //[ReadOnly]
        //public ComponentLookup<Citizen> __Game_Citizens_Citizen_RO_ComponentLookup;

        //[ReadOnly]
        //public ComponentLookup<HealthProblem> __Game_Citizens_HealthProblem_RO_ComponentLookup;

        //[ReadOnly]
        //public ComponentLookup<Student> __Game_Citizens_Student_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<MovingAway> __Game_Agents_MovingAway_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<HomelessHousehold> __Game_Citizens_HomelessHousehold_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            // components
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Citizens_Citizen_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Citizen>(isReadOnly: true);
            __Game_Citizens_Student_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Student>(isReadOnly: true);
            __Game_Citizens_Worker_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Worker>(isReadOnly: true);
            __Game_Citizens_HealthProblem_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HealthProblem>(isReadOnly: true);
            __Game_Citizens_HouseholdMember_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HouseholdMember>(isReadOnly: true);

            // lookups
            __Game_Agents_MovingAway_RO_ComponentLookup = state.GetComponentLookup<MovingAway>(isReadOnly: true);
            __Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(isReadOnly: true);
            __Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(isReadOnly: true);
            __Game_Citizens_HomelessHousehold_RO_ComponentLookup = state.GetComponentLookup<HomelessHousehold>(isReadOnly: true);
        }
    }

    private const string kGroup = "populationInfo";

    //private CitySystem m_CitySystem;

    private SimulationSystem m_SimulationSystem;

    private RawValueBinding m_uiTotals;

    private RawValueBinding m_uiResults;

    private EntityQuery m_TimeDataQuery;
    
    //private EntityQuery m_HouseholdQuery;

    private EntityQuery m_CitizenQuery;

    //private EntityQuery m_WorkProviderQuery;

    //private EntityQuery m_WorkProviderModifiedQuery;

    private NativeArray<int> m_Totals; // final results, totals at city level
    // 0 - num citizens in the city 0 = 1+2+3
    // 1 - num locals
    // 2 - num tourists
    // 3 - num commuters
    // 4 - num students (in locals) 4 <= 1
    // 5 - num workers (in locals) 5 <= 1
    // 6 - oldest cim
    // 7 - moving aways
    // 8 - dead cims
    // 9 - homeless citizens

    private NativeArray<PopulationAtAgeInfo> m_Results; // final results, will be filled via jobs and then written as output

    private TypeHandle __TypeHandle;

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();

        //m_CitySystem = base.World.GetOrCreateSystemManaged<CitySystem>();
        m_SimulationSystem = base.World.GetOrCreateSystemManaged<SimulationSystem>();
        m_TimeDataQuery = GetEntityQuery(ComponentType.ReadOnly<TimeData>());

        m_CitizenQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[1] { ComponentType.ReadOnly<Citizen>() },
            None = new ComponentType[2] { ComponentType.ReadOnly<Deleted>(), ComponentType.ReadOnly<Temp>() }
        });
        RequireForUpdate(m_CitizenQuery);

        AddBinding(m_uiTotals = new RawValueBinding(kGroup, "structureTotals", delegate (IJsonWriter binder)
        {
            // city level info
            binder.ArrayBegin(m_Totals.Length);
            for (int i = 0; i < m_Totals.Length; i++)
                binder.Write(m_Totals[i]);
            binder.ArrayEnd();
        }));

        AddBinding(m_uiResults = new RawValueBinding(kGroup, "structureDetails", delegate(IJsonWriter binder)
        {
            binder.ArrayBegin(m_Results.Length);
            for (int i = 0; i < m_Results.Length; i++)
            {
                WriteData(binder, m_Results[i]);
            }
            binder.ArrayEnd();
        }));

        // TEST
        AddUpdateBinding(new GetterValueBinding<int>(kGroup, "oldest_citizen", () => {
            return m_Totals[6];
        }));

        // allocate memory for results
        m_Totals = new NativeArray<int>(10, Allocator.Persistent);
        m_Results = new NativeArray<PopulationAtAgeInfo>(110, Allocator.Persistent); // INFIXO: TODO
        Plugin.Log("PopulationStructureUISystem created.");
    }

    [Preserve]
    protected override void OnDestroy()
    {
        m_Totals.Dispose();
        m_Results.Dispose();
        base.OnDestroy();
    }

    protected override void OnUpdate()
    {
        if (m_SimulationSystem.frameIndex % 128 != 44)
            return;

        //Plugin.Log($"OnUpdate at frame {m_SimulationSystem.frameIndex}");
        base.OnUpdate();
        ResetResults();

        // code based on AgingJob

        //uint updateFrame = SimulationUtils.GetUpdateFrame(m_SimulationSystem.frameIndex, 1, 16);
        //__TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        //__TypeHandle.__Game_Buildings_Student_RO_BufferLookup.Update(ref base.CheckedStateRef);
        //__TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Worker_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HealthProblem_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);

        __TypeHandle.__Game_Agents_MovingAway_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HomelessHousehold_RO_ComponentLookup.Update(ref base.CheckedStateRef);

        PopulationStructureJob structureJob = default(PopulationStructureJob);
        structureJob.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
        structureJob.m_CitizenType = __TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle;
        structureJob.m_StudentType = __TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle;
        structureJob.m_WorkerType = __TypeHandle.__Game_Citizens_Worker_RO_ComponentTypeHandle;
        structureJob.m_MovingAways = __TypeHandle.__Game_Agents_MovingAway_RO_ComponentLookup;
        structureJob.m_HealthProblemType = __TypeHandle.__Game_Citizens_HealthProblem_RO_ComponentTypeHandle;
        structureJob.m_HouseholdMemberType = __TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle;
        structureJob.m_Households = __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
        structureJob.m_PropertyRenters = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup; 
        structureJob.m_HomelessHouseholds = __TypeHandle.__Game_Citizens_HomelessHousehold_RO_ComponentLookup;
        //structureJob.m_UpdateFrameType = __TypeHandle.__Game_Simulation_UpdateFrame_SharedComponentTypeHandle;
        //structureJob.m_Students = __TypeHandle.__Game_Buildings_Student_RO_BufferLookup;
        //structureJob.m_Purposes = __TypeHandle.__Game_Citizens_TravelPurpose_RO_ComponentLookup;
        //structureJob.m_MoveFromHomeQueue = m_MoveFromHomeQueue.AsParallelWriter();
        structureJob.m_SimulationFrame = m_SimulationSystem.frameIndex;
        structureJob.m_TimeData = m_TimeDataQuery.GetSingleton<TimeData>();
        //structureJob.m_UpdateFrameIndex = updateFrame;
        //structureJob.m_CommandBuffer = m_EndFrameBarrier.CreateCommandBuffer().AsParallelWriter();
        //structureJob.m_DebugAgeAllCitizens = s_DebugAgeAllCitizens;
        //PopulationStructureJob jobData = structureJob;
        //base.Dependency = JobChunkExtensions.ScheduleParallel(jobData, m_CitizenGroup, base.Dependency);
        //m_EndFrameBarrier.AddJobHandleForProducer(base.Dependency); // Infixo: frame barrier is not used because we wait to complete the job
        structureJob.m_Totals = m_Totals;
        structureJob.m_Results = m_Results;
        JobChunkExtensions.Schedule(structureJob, m_CitizenQuery, base.Dependency).Complete();

        //int ageInDays = m_Results[2];
        //int num2 = m_Results[1];
        //int num3 = m_Results[4];
        //int newValue = m_Results[5];
        //int num4 = num2 + ageInDays - m_Results[6];
        //float newValue2 = (((float)num4 > 0f) ? ((float)(num4 - num3) / (float)num4 * 100f) : 0f);
        //m_Jobs.Update(newValue);
        //m_Employed.Update(num3);
        //m_Unemployment.Update(newValue2);
        //Population componentData = base.EntityManager.GetComponentData<Population>(m_CitySystem.City);
        //m_Population.Update(componentData.m_Population);

        /* DEBUG
        Plugin.Log($"results: {m_Totals[0]} {m_Totals[1]} {m_Totals[2]} {m_Totals[3]} students {m_Totals[4]} workers {m_Totals[5]}");
        for (int i = 0; i < m_Results.Length; i++)
        {
            PopulationAtAgeInfo info = m_Results[i];
            Plugin.Log($"...[{i}]: {info.Age} {info.Total} students {info.School1} {info.School2} {info.School3} {info.School4} workers {info.Work} other {info.Other}");
        }
        */

        m_uiTotals.Update();
        m_uiResults.Update();

        //InspectComponentsInQuery(m_CitizenQuery);

        /*
        //UpdateStatistics();
        //Plugin.Log("jobs",true);
        if (!dumped)
        {
            Plugin.Log("Chunks with Citizen component");
            Entities.WithAll<Citizen>().ForEach((ArchetypeChunk chunk) =>
            {
                Plugin.Log("CHUNK START");
                // Iterate over the component types in the chunk
                var componentTypes = chunk.Archetype.GetComponentTypes();

                // Log the component types
                foreach (var componentType in componentTypes)
                {
                    Plugin.Log($"Component Type in Chunk: {componentType.GetManagedType()}");
                }

                // Your processing logic here
            }).Schedule();
            dumped = true;
        }
        */
    }

    private void ResetResults()
    {
        for (int i = 0; i < m_Totals.Length; i++)
        {
            m_Totals[i] = 0;
        }
        for (int i = 0; i < m_Results.Length; i++)
        {
            m_Results[i] = new PopulationAtAgeInfo(i);
        }
        //Plugin.Log("reset",true);
    }

    /*
    private void UpdateAgeData(IJsonWriter binder)
    {
        int children = m_Results[0];
        int teens = m_Results[1];
        int adults = m_Results[2];
        int seniors = m_Results[3];
        UpdateAgeDataBinding(binder, children, teens, adults, seniors);
    }
    */
    /*
    private static void UpdateAgeDataBinding(IJsonWriter binder, NativeArray<PopulationAtAgeInfo> m_Results)
    {
        binder.TypeBegin("populationStructure");
        binder.PropertyName("values");
        binder.ArrayBegin(m_Results.Length);

        for (int i = 0; i < m_Results.Length; i++)
        {
            WriteData(binder, m_Results[i]);
        }

        binder.ArrayEnd();
        binder.TypeEnd();
    }
    */
    /* Infixo: not used
    private void UpdateStatistics()
    {
        m_BirthRate.Update(m_CityStatisticsSystem.GetStatisticValue(StatisticType.BirthRate));
        m_DeathRate.Update(m_CityStatisticsSystem.GetStatisticValue(StatisticType.DeathRate));
        m_MovedIn.Update(m_CityStatisticsSystem.GetStatisticValue(StatisticType.CitizensMovedIn));
        m_MovedAway.Update(m_CityStatisticsSystem.GetStatisticValue(StatisticType.CitizensMovedAway));
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
    public PopulationStructureUISystem()
    {
    }

    public static void LogChunk(in ArchetypeChunk chunk)
    {
        var componentTypes = chunk.Archetype.GetComponentTypes();
        Plugin.Log($"chunk: {chunk.Count}, {string.Join(", ", componentTypes.Select(ct => ct.GetType().GetTypeInfo().FullName        ))}");
    }

    public string[] ListEntityComponents(Entity entity)
    {
        var componentTypes = new List<ComponentType>();

        if (!EntityManager.Exists(entity))
            throw new ArgumentException("Entity does not exist.");

        //using (NativeArray<ComponentType> types = EntityManager.GetComponentTypes(entity, Allocator.Temp))
        //{
            //foreach (var type in types)
            //{
                //componentTypes.Add(type);
            //}
        //}

        NativeArray<ComponentType> NativeArray = EntityManager.GetComponentTypes(entity, Allocator.Temp);
        string[] ToReturn = NativeArray.Select(T => T.GetManagedType().Name).ToArray();
        NativeArray.Dispose();
        return ToReturn;

        //return componentTypes;
    }

    public void InspectComponentsInQuery(EntityQuery query)
    {
        Dictionary<string, int> CompDict = new Dictionary<string, int>();
        NativeArray<Entity> entities = m_CitizenQuery.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < entities.Length; i++)
        {
            Entity entity = entities[i];
            string[] comps = ListEntityComponents(entity);
            foreach (string comp in comps)
            {
                if (CompDict.ContainsKey(comp)) CompDict[comp]++;
                else CompDict.Add(comp, 1);
            }
        }
        entities.Dispose();
        // show the dictionary
        Plugin.Log("=== Components in selected chunks ===");
        foreach (var pair in CompDict)
        {
            Plugin.Log($"{pair.Key} {pair.Value}");
        }
    }
}
