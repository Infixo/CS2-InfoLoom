using System.Runtime.CompilerServices;
using Colossal.UI.Binding;
using Game.Agents;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Companies;
using Game.Simulation;
using Game.Tools;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Scripting;
using Game.UI;
using Game;
using Game.Prefabs;
//using Unity.Core;
//using UnityEngine.Rendering;

namespace PopStruct;

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
        public ComponentTypeHandle<Student> m_StudentType;

        [ReadOnly]
        public ComponentTypeHandle<Worker> m_WorkerType;

        [ReadOnly]
        public BufferTypeHandle<HouseholdCitizen> m_HouseholdCitizenHandle;

        [ReadOnly]
        public ComponentTypeHandle<Household> m_HouseholdType;

        [ReadOnly]
        public ComponentLookup<Worker> m_WorkerFromEntity;

        [ReadOnly]
        public ComponentLookup<HealthProblem> m_HealthProblems;

        [ReadOnly]
        public ComponentLookup<Citizen> m_Citizens;

        [ReadOnly]
        public ComponentLookup<Student> m_Students;

        public TimeData m_TimeData;

        //public uint m_UpdateFrameIndex;
        
        public uint m_SimulationFrame;

        public NativeArray<int> m_Totals;

        public NativeArray<PopulationAtAgeInfo> m_Results;

        // this job is based on AgingJob
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(m_EntityType);
            NativeArray<Citizen> citizens = chunk.GetNativeArray(ref m_CitizenType);
            NativeArray<Student> students = chunk.GetNativeArray(ref m_StudentType);
            NativeArray<Worker> workers = chunk.GetNativeArray(ref m_WorkerType);
            bool isStudent = chunk.Has(ref m_StudentType); // are there students in this chunk?
            bool isWorker = chunk.Has(ref m_WorkerType); // are there workers in this chunk?
            int day = TimeSystem.GetDay(m_SimulationFrame, m_TimeData);
            //Plugin.Log($"day {day} chunk: {entities.Length} entities, {citizens.Length} citizens, {isStudent} {students.Length} students, {isWorker} {workers.Length} workers");
            if (!chunk.Has(ref m_CitizenType))
            {
                Plugin.Log($"NOCITIZENS in the chunk");
                return;
            }
            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                //Plugin.Log($"{entity}");
                Citizen citizen = citizens[i];
                //Plugin.Log($"{citizen}");
                // citizen data
                bool isCommuter = ((citizen.m_State & CitizenFlags.Commuter) != CitizenFlags.None);
                bool isTourist = ((citizen.m_State & CitizenFlags.Commuter) != CitizenFlags.None);
                // we don't know if it is already settled?
                m_Totals[0]++;
                if (isTourist) m_Totals[2]++;
                else if (isCommuter) m_Totals[3]++;
                else m_Totals[1]++; // locals
                if (isTourist || isCommuter)
                    continue; // not local, go for the next
                // get age
                int ageInDays = day - citizen.m_BirthDay;
                if (ageInDays > m_Totals[6]) m_Totals[6] = ageInDays; // oldest cim
                ageInDays = ageInDays/10; // INFIXO: TODO
                PopulationAtAgeInfo info = m_Results[ageInDays];
                // process at-age info
                info.Total++;
                // process a student
                if (isStudent)
                {
                    m_Totals[4]++;
                    Student student = students[i];
                    // check what school level
                    switch (students[i].m_Level)
                    {
                        case 1: info.School1++; break;
                        case 2: info.School2++; break;
                        case 3: info.School3++; break;
                        case 4: info.School4++; break;
                        default:
                            Plugin.Log($"WARNING: incorrect school level, {students[i].m_Level}");
                            break;
                    }
                }
                // process a worker
                if (isWorker)
                {
                    // we can check the level here? what is it actually?
                    m_Totals[5]++;
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


        // 
        public void Execute_Households(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            //Plugin.Log($"Execute: {chunk.Count} entities");
            BufferAccessor<HouseholdCitizen> bufferAccessor = chunk.GetBufferAccessor(ref m_HouseholdCitizenHandle);
            NativeArray<Household> nativeArray = chunk.GetNativeArray(ref m_HouseholdType);
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            for (int i = 0; i < bufferAccessor.Length; i++)
            {
                DynamicBuffer<HouseholdCitizen> dynamicBuffer = bufferAccessor[i];
                if ((nativeArray[i].m_Flags & HouseholdFlags.MovedIn) == 0)
                {
                    continue;
                }
                for (int j = 0; j < dynamicBuffer.Length; j++)
                {
                    Entity citizen = dynamicBuffer[j].m_Citizen;
                    Citizen citizen2 = m_Citizens[citizen];
                    if ((m_HealthProblems.HasComponent(citizen) && CitizenUtils.IsDead(m_HealthProblems[citizen])) || (citizen2.m_State & (CitizenFlags.Tourist | CitizenFlags.Commuter)) != 0)
                    {
                        continue;
                    }
                    switch (citizen2.GetAge())
                    {
                    case CitizenAge.Child:
                        num++;
                        break;
                    case CitizenAge.Teen:
                        num2++;
                        if (m_Students.HasComponent(citizen))
                        {
                            num6++;
                        }
                        break;
                    case CitizenAge.Adult:
                        num3++;
                        if (m_Students.HasComponent(citizen))
                        {
                            num6++;
                        }
                        break;
                    case CitizenAge.Elderly:
                        num4++;
                        break;
                    }
                    if (m_WorkerFromEntity.HasComponent(citizen))
                    {
                        num5++;
                    }
                }
            }
            // Work with a local variable to avoid CS0206 error
            PopulationAtAgeInfo elem0 = m_Results[0];
            elem0.School1 = num;
            elem0.School2 = num2;
            elem0.School3 = num3;
            elem0.School4 = num4;
            m_Results[0] = elem0;
            //m_Results[0].Total += ageInDays;
            //m_Results[1].Total += num2;
            //m_Results[2].Total += num3;
            //m_Results[3].Total += num4;
            //m_Results[4].Total += num5;
            //m_Results[6].Total += num6;
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

        public BufferTypeHandle<HouseholdCitizen> __Game_Citizens_HouseholdCitizen_RW_BufferTypeHandle;

        public ComponentTypeHandle<Household> __Game_Citizens_Household_RW_ComponentTypeHandle;

        [ReadOnly]
        public ComponentLookup<Worker> __Game_Citizens_Worker_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Citizen> __Game_Citizens_Citizen_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<HealthProblem> __Game_Citizens_HealthProblem_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Student> __Game_Citizens_Student_RO_ComponentLookup;

        [ReadOnly]
        public ComponentTypeHandle<WorkProvider> __Game_Companies_WorkProvider_RO_ComponentTypeHandle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Citizens_Citizen_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Citizen>(isReadOnly: true);
            __Game_Citizens_Student_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Student>(isReadOnly: true);
            __Game_Citizens_Worker_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Worker>(isReadOnly: true);

            //__Game_Simulation_UpdateFrame_SharedComponentTypeHandle = state.GetSharedComponentTypeHandle<UpdateFrame>();
            //__Game_Buildings_Student_RO_BufferLookup = state.GetBufferLookup<Game.Buildings.Student>(isReadOnly: true);
            //__Game_Citizens_TravelPurpose_RO_ComponentLookup = state.GetComponentLookup<TravelPurpose>(isReadOnly: true);
            //__Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(isReadOnly: true);
            //__Game_Citizens_HouseholdMember_RW_ComponentLookup = state.GetComponentLookup<HouseholdMember>();
            //__Game_Citizens_HouseholdCitizen_RW_BufferLookup = state.GetBufferLookup<HouseholdCitizen>();
            //__Game_Prefabs_ArchetypeData_RO_ComponentLookup = state.GetComponentLookup<ArchetypeData>(isReadOnly: true);


            __Game_Citizens_HouseholdCitizen_RW_BufferTypeHandle = state.GetBufferTypeHandle<HouseholdCitizen>();
            __Game_Citizens_Household_RW_ComponentTypeHandle = state.GetComponentTypeHandle<Household>();
            __Game_Citizens_Worker_RO_ComponentLookup = state.GetComponentLookup<Worker>(isReadOnly: true);
            __Game_Citizens_Citizen_RO_ComponentLookup = state.GetComponentLookup<Citizen>(isReadOnly: true);
            __Game_Citizens_HealthProblem_RO_ComponentLookup = state.GetComponentLookup<HealthProblem>(isReadOnly: true);
            __Game_Citizens_Student_RO_ComponentLookup = state.GetComponentLookup<Student>(isReadOnly: true);
            __Game_Companies_WorkProvider_RO_ComponentTypeHandle = state.GetComponentTypeHandle<WorkProvider>(isReadOnly: true);
        }
    }

    private const string kGroup = "populationInfo";

    //private CityStatisticsSystem m_CityStatisticsSystem;

    private CitySystem m_CitySystem;

    private SimulationSystem m_SimulationSystem;

    //private ValueBinding<int> m_Population;

    //private ValueBinding<int> m_Employed;

    //private ValueBinding<int> m_Jobs;

    //private ValueBinding<float> m_Unemployment;

    //private ValueBinding<int> m_BirthRate;

    //private ValueBinding<int> m_DeathRate;

    //private ValueBinding<int> m_MovedIn;

    //private ValueBinding<int> m_MovedAway;

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

    private NativeArray<PopulationAtAgeInfo> m_Results; // final results, will be filled via jobs and then written as output

    private TypeHandle __TypeHandle;

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        //m_CityStatisticsSystem = base.World.GetOrCreateSystemManaged<CityStatisticsSystem>();
        m_CitySystem = base.World.GetOrCreateSystemManaged<CitySystem>();
        m_SimulationSystem = base.World.GetOrCreateSystemManaged<SimulationSystem>();
        m_TimeDataQuery = GetEntityQuery(ComponentType.ReadOnly<TimeData>());
        /*
        m_HouseholdQuery = GetEntityQuery(
            // include
            ComponentType.ReadOnly<Household>(), // flags: 0 - none, tourist, commuter, MovedIn - 4
            ComponentType.ReadOnly<HouseholdCitizen>(), // entity citizen
            ComponentType.ReadOnly<Citizen>(), // main citizen data, inc. birthday
            ComponentType.ReadOnly<Worker>(), // where cim works
            ComponentType.ReadOnly<Student>(), // where studies, at what level
            // exclude
            ComponentType.Exclude<PropertySeeker>(), // current and best, why excluded? are they "outside" of city?
            ComponentType.Exclude<TouristHousehold>(), // hotel, when leaves
            ComponentType.Exclude<CommuterHousehold>(), // where from
            ComponentType.Exclude<MovingAway>()); // empty component
        */
        m_CitizenQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[1] { ComponentType.ReadOnly<Citizen>() },
            //Any = new ComponentType[2] { ComponentType.ReadOnly<Student>(), ComponentType.ReadOnly<Worker>() },
            None = new ComponentType[2] { ComponentType.ReadOnly<Deleted>(), ComponentType.ReadOnly<Temp>() }
        });
        RequireForUpdate(m_CitizenQuery);
        //m_WorkProviderQuery = GetEntityQuery(ComponentType.ReadOnly<WorkProvider>(), ComponentType.Exclude<PropertySeeker>(), ComponentType.Exclude<Native>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Temp>());
        //m_WorkProviderModifiedQuery = GetEntityQuery(ComponentType.ReadOnly<WorkProvider>(), ComponentType.ReadOnly<Created>(), ComponentType.ReadOnly<Deleted>(), ComponentType.ReadOnly<Updated>(), ComponentType.Exclude<Temp>());

        AddBinding(m_uiTotals = new RawValueBinding(kGroup, "structureTotals", delegate (IJsonWriter binder)
        {
            // city level info
            binder.ArrayBegin(m_Totals.Length);
            for (int i = 0; i < m_Totals.Length; i++)
                binder.Write(m_Totals[i]);
            /* this display [ xxx, xxx, xxx, xxx, xxx ], there are no property names
            binder.PropertyName("citizens");
            binder.Write(m_Totals[0]);
            binder.PropertyName("locals");
            binder.Write(m_Totals[1]);
            binder.PropertyName("tourists");
            binder.Write(m_Totals[2]);
            binder.PropertyName("commuters");
            binder.Write(m_Totals[3]);
            binder.PropertyName("students");
            binder.Write(m_Totals[4]);
            binder.PropertyName("workers");
            binder.Write(m_Totals[5]);
            */
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
        m_Totals = new NativeArray<int>(7, Allocator.Persistent);
        m_Results = new NativeArray<PopulationAtAgeInfo>(20, Allocator.Persistent); // INFIXO: TODO
        Plugin.Log("ProductionStructureUISystem created.");
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
        if (m_SimulationSystem.frameIndex % 128 != 77)
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

        PopulationStructureJob structureJob = default(PopulationStructureJob);
        //structureJob.m_BecomeTeenCounter = m_BecomeTeenCounter.ToConcurrent();
        //structureJob.m_BecomeAdultCounter = m_BecomeAdultCounter.ToConcurrent();
        //structureJob.m_BecomeElderCounter = m_BecomeElderCounter.ToConcurrent();
        structureJob.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
        structureJob.m_CitizenType = __TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle;
        structureJob.m_StudentType = __TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle;
        structureJob.m_WorkerType = __TypeHandle.__Game_Citizens_Worker_RO_ComponentTypeHandle;
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


        // main job that processess the households, so we can skip cims that have not yet moved in
        /*
        __TypeHandle.__Game_Citizens_Student_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HealthProblem_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Worker_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Household_RW_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferTypeHandle.Update(ref base.CheckedStateRef);
        PopulationStructureJob jobData = default(PopulationStructureJob);
        jobData.m_HouseholdCitizenHandle = __TypeHandle.__Game_Citizens_HouseholdCitizen_RW_BufferTypeHandle;
        jobData.m_HouseholdType = __TypeHandle.__Game_Citizens_Household_RW_ComponentTypeHandle;
        jobData.m_WorkerFromEntity = __TypeHandle.__Game_Citizens_Worker_RO_ComponentLookup;
        jobData.m_Citizens = __TypeHandle.__Game_Citizens_Citizen_RO_ComponentLookup;
        jobData.m_HealthProblems = __TypeHandle.__Game_Citizens_HealthProblem_RO_ComponentLookup;
        jobData.m_Students = __TypeHandle.__Game_Citizens_Student_RO_ComponentLookup;
        jobData.m_Results = m_Results;
        JobChunkExtensions.Schedule(jobData, m_HouseholdQuery, base.Dependency).Complete();
        */

        /* another job counts workplaces, so not useful
        __TypeHandle.__Game_Companies_WorkProvider_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        WorkProviderJob jobData2 = default(WorkProviderJob);
        jobData2.m_WorkProviderHandle = __TypeHandle.__Game_Companies_WorkProvider_RO_ComponentTypeHandle;
        jobData2.m_Results = m_Results;
        JobChunkExtensions.Schedule(jobData2, m_WorkProviderQuery, base.Dependency).Complete();
        */

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

        //UpdateStatistics();
        //Plugin.Log("jobs",true);
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
}
