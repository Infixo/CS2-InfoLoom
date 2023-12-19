using System.Runtime.CompilerServices;
using Colossal;
using Colossal.Collections;
using Colossal.Serialization.Entities;
using Game.Agents;
using Game.Buildings;
using Game.Citizens;
using Game.Common;
using Game.Debug;
using Game.Objects;
using Game.Tools;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Scripting;

namespace Game.Simulation;

[CompilerGenerated]
public class CountEmploymentSystem : GameSystemBase, IDefaultSerializable, ISerializable
{
    [BurstCompile]
    private struct CountEmploymentJob : IJobChunk
    {
        [ReadOnly]
        public ComponentTypeHandle<Citizen> m_CitizenType;

        [ReadOnly]
        public ComponentTypeHandle<Worker> m_WorkerType;

        [ReadOnly]
        public ComponentTypeHandle<Game.Citizens.Student> m_StudentType;

        [ReadOnly]
        public ComponentTypeHandle<HealthProblem> m_HealthProblemType;

        [ReadOnly]
        public ComponentTypeHandle<HouseholdMember> m_HouseholdMemberType;

        [ReadOnly]
        public ComponentLookup<OutsideConnection> m_OutsideConnections;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> m_PropertyRenters;

        [ReadOnly]
        public ComponentLookup<MovingAway> m_MovingAways;

        [ReadOnly]
        public ComponentLookup<Household> m_Households;

        public NativeCounter.Concurrent m_PotentialWorkersByEducation0;

        public NativeCounter.Concurrent m_PotentialWorkersByEducation1;

        public NativeCounter.Concurrent m_PotentialWorkersByEducation2;

        public NativeCounter.Concurrent m_PotentialWorkersByEducation3;

        public NativeCounter.Concurrent m_PotentialWorkersByEducation4;

        public NativeCounter.Concurrent m_Workers;

        public NativeCounter.Concurrent m_Adults;

        public NativeCounter.Concurrent m_Unemployed;

        public NativeCounter.Concurrent m_EmployableByEducation0;

        public NativeCounter.Concurrent m_EmployableByEducation1;

        public NativeCounter.Concurrent m_EmployableByEducation2;

        public NativeCounter.Concurrent m_EmployableByEducation3;

        public NativeCounter.Concurrent m_EmployableByEducation4;

        public NativeCounter.Concurrent m_UnemploymentByEducation0;

        public NativeCounter.Concurrent m_UnemploymentByEducation1;

        public NativeCounter.Concurrent m_UnemploymentByEducation2;

        public NativeCounter.Concurrent m_UnemploymentByEducation3;

        public NativeCounter.Concurrent m_UnemploymentByEducation4;

        private void AddPotential(int level)
        {
            switch (level)
            {
            case 0:
                m_PotentialWorkersByEducation0.Increment();
                break;
            case 1:
                m_PotentialWorkersByEducation1.Increment();
                break;
            case 2:
                m_PotentialWorkersByEducation2.Increment();
                break;
            case 3:
                m_PotentialWorkersByEducation3.Increment();
                break;
            case 4:
                m_PotentialWorkersByEducation4.Increment();
                break;
            }
        }

        private void AddEmployable(int level)
        {
            switch (level)
            {
            case 0:
                m_EmployableByEducation0.Increment();
                break;
            case 1:
                m_EmployableByEducation1.Increment();
                break;
            case 2:
                m_EmployableByEducation2.Increment();
                break;
            case 3:
                m_EmployableByEducation3.Increment();
                break;
            case 4:
                m_EmployableByEducation4.Increment();
                break;
            }
        }

        private void AddUnemployment(int level)
        {
            switch (level)
            {
            case 0:
                m_UnemploymentByEducation0.Increment();
                break;
            case 1:
                m_UnemploymentByEducation1.Increment();
                break;
            case 2:
                m_UnemploymentByEducation2.Increment();
                break;
            case 3:
                m_UnemploymentByEducation3.Increment();
                break;
            case 4:
                m_UnemploymentByEducation4.Increment();
                break;
            }
        }

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<HouseholdMember> nativeArray = chunk.GetNativeArray(ref m_HouseholdMemberType);
            if (!nativeArray.IsCreated)
            {
                return;
            }
            NativeArray<Citizen> nativeArray2 = chunk.GetNativeArray(ref m_CitizenType);
            NativeArray<Worker> nativeArray3 = chunk.GetNativeArray(ref m_WorkerType);
            NativeArray<HealthProblem> nativeArray4 = chunk.GetNativeArray(ref m_HealthProblemType);
            bool isCreated = nativeArray3.IsCreated;
            bool flag = chunk.Has(ref m_StudentType);
            bool isCreated2 = nativeArray4.IsCreated;
            for (int i = 0; i < chunk.Count; i++)
            {
                Citizen citizen = nativeArray2[i];
                CitizenAge age = citizen.GetAge();
                if (age == CitizenAge.Child || age == CitizenAge.Elderly)
                {
                    continue;
                }
                Entity household = nativeArray[i].m_Household;
                if ((isCreated2 && CitizenUtils.IsDead(nativeArray4[i])) || (citizen.m_State & (CitizenFlags.Tourist | CitizenFlags.Commuter)) != 0 || !m_Households.HasComponent(household) || (m_Households[household].m_Flags & HouseholdFlags.MovedIn) == 0 || m_MovingAways.HasComponent(household))
                {
                    continue;
                }
                m_Adults.Increment();
                if (isCreated)
                {
                    m_Workers.Increment();
                    int educationLevel = citizen.GetEducationLevel();
                    AddPotential(educationLevel);
                    Worker worker = nativeArray3[i];
                    if (m_OutsideConnections.HasComponent(worker.m_Workplace) || worker.m_Level < educationLevel)
                    {
                        AddEmployable(educationLevel);
                    }
                }
                else if (!flag && m_PropertyRenters.HasComponent(household))
                {
                    m_Unemployed.Increment();
                    int educationLevel2 = citizen.GetEducationLevel();
                    AddPotential(educationLevel2);
                    AddEmployable(educationLevel2);
                    AddUnemployment(educationLevel2);
                }
            }
        }

        void IJobChunk.Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            Execute(in chunk, unfilteredChunkIndex, useEnabledMask, in chunkEnabledMask);
        }
    }

    private struct SumEmploymentJob : IJob
    {
        public NativeCounter m_PotentialWorkersByEducation0;

        public NativeCounter m_PotentialWorkersByEducation1;

        public NativeCounter m_PotentialWorkersByEducation2;

        public NativeCounter m_PotentialWorkersByEducation3;

        public NativeCounter m_PotentialWorkersByEducation4;

        public NativeCounter m_WorkersTemp;

        public NativeCounter m_AdultsTemp;

        public NativeCounter m_UnemployedTemp;

        public NativeCounter m_EmployableByEducation0;

        public NativeCounter m_EmployableByEducation1;

        public NativeCounter m_EmployableByEducation2;

        public NativeCounter m_EmployableByEducation3;

        public NativeCounter m_EmployableByEducation4;

        public NativeCounter m_UnemploymentByEducation0;

        public NativeCounter m_UnemploymentByEducation1;

        public NativeCounter m_UnemploymentByEducation2;

        public NativeCounter m_UnemploymentByEducation3;

        public NativeCounter m_UnemploymentByEducation4;

        public NativeArray<int> m_PotentialWorkersByEducation;

        public NativeArray<int> m_EmployableByEducation;

        public NativeArray<int> m_UnemploymentByEducation;

        public NativeValue<int> m_Workers;

        public NativeValue<int> m_Adults;

        public NativeValue<int> m_Unemployed;

        public NativeValue<int> m_Unemployment;

        public void Execute()
        {
            m_Workers.value = m_WorkersTemp.Count;
            m_Adults.value = m_AdultsTemp.Count;
            m_Unemployed.value = m_UnemployedTemp.Count;
            m_EmployableByEducation[0] = m_EmployableByEducation0.Count;
            m_EmployableByEducation[1] = m_EmployableByEducation1.Count;
            m_EmployableByEducation[2] = m_EmployableByEducation2.Count;
            m_EmployableByEducation[3] = m_EmployableByEducation3.Count;
            m_EmployableByEducation[4] = m_EmployableByEducation4.Count;
            m_PotentialWorkersByEducation[0] = m_PotentialWorkersByEducation0.Count;
            m_PotentialWorkersByEducation[1] = m_PotentialWorkersByEducation1.Count;
            m_PotentialWorkersByEducation[2] = m_PotentialWorkersByEducation2.Count;
            m_PotentialWorkersByEducation[3] = m_PotentialWorkersByEducation3.Count;
            m_PotentialWorkersByEducation[4] = m_PotentialWorkersByEducation4.Count;
            m_UnemploymentByEducation[0] = Mathf.RoundToInt(100f * (float)m_UnemploymentByEducation0.Count / (float)(m_PotentialWorkersByEducation0.Count + 1));
            m_UnemploymentByEducation[1] = Mathf.RoundToInt(100f * (float)m_UnemploymentByEducation1.Count / (float)(m_PotentialWorkersByEducation1.Count + 1));
            m_UnemploymentByEducation[2] = Mathf.RoundToInt(100f * (float)m_UnemploymentByEducation2.Count / (float)(m_PotentialWorkersByEducation2.Count + 1));
            m_UnemploymentByEducation[3] = Mathf.RoundToInt(100f * (float)m_UnemploymentByEducation3.Count / (float)(m_PotentialWorkersByEducation3.Count + 1));
            m_UnemploymentByEducation[4] = Mathf.RoundToInt(100f * (float)m_UnemploymentByEducation4.Count / (float)(m_PotentialWorkersByEducation4.Count + 1));
            m_Unemployment.value = 100 * m_Unemployed.value / (m_Adults.value + 1);
            m_WorkersTemp.Count = 0;
            m_AdultsTemp.Count = 0;
            m_UnemployedTemp.Count = 0;
            m_EmployableByEducation0.Count = 0;
            m_EmployableByEducation1.Count = 0;
            m_EmployableByEducation2.Count = 0;
            m_EmployableByEducation3.Count = 0;
            m_EmployableByEducation4.Count = 0;
            m_PotentialWorkersByEducation0.Count = 0;
            m_PotentialWorkersByEducation1.Count = 0;
            m_PotentialWorkersByEducation2.Count = 0;
            m_PotentialWorkersByEducation3.Count = 0;
            m_PotentialWorkersByEducation4.Count = 0;
            m_UnemploymentByEducation0.Count = 0;
            m_UnemploymentByEducation1.Count = 0;
            m_UnemploymentByEducation2.Count = 0;
            m_UnemploymentByEducation3.Count = 0;
            m_UnemploymentByEducation4.Count = 0;
        }
    }

    private struct TypeHandle
    {
        [ReadOnly]
        public ComponentTypeHandle<Citizen> __Game_Citizens_Citizen_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<HealthProblem> __Game_Citizens_HealthProblem_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Worker> __Game_Citizens_Worker_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<Game.Citizens.Student> __Game_Citizens_Student_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<HouseholdMember> __Game_Citizens_HouseholdMember_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentLookup<MovingAway> __Game_Agents_MovingAway_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<OutsideConnection> __Game_Objects_OutsideConnection_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Game_Citizens_Citizen_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Citizen>(isReadOnly: true);
            __Game_Citizens_HealthProblem_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HealthProblem>(isReadOnly: true);
            __Game_Citizens_Worker_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Worker>(isReadOnly: true);
            __Game_Citizens_Student_RO_ComponentTypeHandle = state.GetComponentTypeHandle<Game.Citizens.Student>(isReadOnly: true);
            __Game_Citizens_HouseholdMember_RO_ComponentTypeHandle = state.GetComponentTypeHandle<HouseholdMember>(isReadOnly: true);
            __Game_Agents_MovingAway_RO_ComponentLookup = state.GetComponentLookup<MovingAway>(isReadOnly: true);
            __Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(isReadOnly: true);
            __Game_Objects_OutsideConnection_RO_ComponentLookup = state.GetComponentLookup<OutsideConnection>(isReadOnly: true);
            __Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(isReadOnly: true);
        }
    }

    private EntityQuery m_AllAdultGroup;

    [DebugWatchValue]
    private NativeValue<int> m_Workers;

    [DebugWatchValue]
    private NativeValue<int> m_Adults;

    [DebugWatchValue]
    private NativeValue<int> m_Unemployed;

    [DebugWatchValue]
    private NativeArray<int> m_EmployableByEducation;

    [DebugWatchValue]
    private NativeValue<int> m_Unemployment;

    [DebugWatchValue]
    private NativeArray<int> m_UnemploymentByEducation;

    private NativeArray<int> m_PotentialWorkersByEducation;

    private NativeCounter m_WorkersTemp;

    private NativeCounter m_AdultsTemp;

    private NativeCounter m_UnemployedTemp;

    private NativeCounter m_EmployableByEducation0;

    private NativeCounter m_EmployableByEducation1;

    private NativeCounter m_EmployableByEducation2;

    private NativeCounter m_EmployableByEducation3;

    private NativeCounter m_EmployableByEducation4;

    private NativeCounter m_UnemploymentByEducation0;

    private NativeCounter m_UnemploymentByEducation1;

    private NativeCounter m_UnemploymentByEducation2;

    private NativeCounter m_UnemploymentByEducation3;

    private NativeCounter m_UnemploymentByEducation4;

    private NativeCounter m_PotentialWorkersByEducation0;

    private NativeCounter m_PotentialWorkersByEducation1;

    private NativeCounter m_PotentialWorkersByEducation2;

    private NativeCounter m_PotentialWorkersByEducation3;

    private NativeCounter m_PotentialWorkersByEducation4;

    [DebugWatchDeps]
    private JobHandle m_WriteDependencies;

    private JobHandle m_ReadDependencies;

    private TypeHandle __TypeHandle;

    public override int GetUpdateInterval(SystemUpdatePhase phase)
    {
        return 16;
    }

    public NativeArray<int> GetEmployableByEducation(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_EmployableByEducation;
    }

    public NativeArray<int> GetUnemploymentByEducation(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_UnemploymentByEducation;
    }

    public NativeValue<int> GetUnemployment(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_Unemployment;
    }

    public void AddReader(JobHandle reader)
    {
        m_ReadDependencies = JobHandle.CombineDependencies(m_ReadDependencies, reader);
    }

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        m_AllAdultGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[1] { ComponentType.ReadOnly<Citizen>() },
            None = new ComponentType[3]
            {
                ComponentType.ReadOnly<Game.Citizens.Student>(),
                ComponentType.ReadOnly<Deleted>(),
                ComponentType.ReadOnly<Temp>()
            }
        });
        m_Workers = new NativeValue<int>(Allocator.Persistent);
        m_Adults = new NativeValue<int>(Allocator.Persistent);
        m_Unemployed = new NativeValue<int>(Allocator.Persistent);
        m_EmployableByEducation = new NativeArray<int>(5, Allocator.Persistent);
        m_UnemploymentByEducation = new NativeArray<int>(5, Allocator.Persistent);
        m_PotentialWorkersByEducation = new NativeArray<int>(5, Allocator.Persistent);
        m_Unemployment = new NativeValue<int>(Allocator.Persistent);
        m_WorkersTemp = new NativeCounter(Allocator.Persistent);
        m_AdultsTemp = new NativeCounter(Allocator.Persistent);
        m_UnemployedTemp = new NativeCounter(Allocator.Persistent);
        m_EmployableByEducation0 = new NativeCounter(Allocator.Persistent);
        m_EmployableByEducation1 = new NativeCounter(Allocator.Persistent);
        m_EmployableByEducation2 = new NativeCounter(Allocator.Persistent);
        m_EmployableByEducation3 = new NativeCounter(Allocator.Persistent);
        m_EmployableByEducation4 = new NativeCounter(Allocator.Persistent);
        m_UnemploymentByEducation0 = new NativeCounter(Allocator.Persistent);
        m_UnemploymentByEducation1 = new NativeCounter(Allocator.Persistent);
        m_UnemploymentByEducation2 = new NativeCounter(Allocator.Persistent);
        m_UnemploymentByEducation3 = new NativeCounter(Allocator.Persistent);
        m_UnemploymentByEducation4 = new NativeCounter(Allocator.Persistent);
        m_PotentialWorkersByEducation0 = new NativeCounter(Allocator.Persistent);
        m_PotentialWorkersByEducation1 = new NativeCounter(Allocator.Persistent);
        m_PotentialWorkersByEducation2 = new NativeCounter(Allocator.Persistent);
        m_PotentialWorkersByEducation3 = new NativeCounter(Allocator.Persistent);
        m_PotentialWorkersByEducation4 = new NativeCounter(Allocator.Persistent);
    }

    [Preserve]
    protected override void OnDestroy()
    {
        m_Workers.Dispose();
        m_Adults.Dispose();
        m_Unemployed.Dispose();
        m_EmployableByEducation.Dispose();
        m_Unemployment.Dispose();
        m_UnemploymentByEducation.Dispose();
        m_PotentialWorkersByEducation.Dispose();
        m_WorkersTemp.Dispose();
        m_AdultsTemp.Dispose();
        m_UnemployedTemp.Dispose();
        m_EmployableByEducation0.Dispose();
        m_EmployableByEducation1.Dispose();
        m_EmployableByEducation2.Dispose();
        m_EmployableByEducation3.Dispose();
        m_EmployableByEducation4.Dispose();
        m_UnemploymentByEducation0.Dispose();
        m_UnemploymentByEducation1.Dispose();
        m_UnemploymentByEducation2.Dispose();
        m_UnemploymentByEducation3.Dispose();
        m_UnemploymentByEducation4.Dispose();
        m_PotentialWorkersByEducation0.Dispose();
        m_PotentialWorkersByEducation1.Dispose();
        m_PotentialWorkersByEducation2.Dispose();
        m_PotentialWorkersByEducation3.Dispose();
        m_PotentialWorkersByEducation4.Dispose();
        base.OnDestroy();
    }

    public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
    {
        writer.Write(m_EmployableByEducation);
        writer.Write(m_Unemployment.value);
        writer.Write(m_UnemploymentByEducation);
    }

    public void Deserialize<TReader>(TReader reader) where TReader : IReader
    {
        reader.Read(m_EmployableByEducation);
        reader.Read(out int value);
        m_Unemployment.value = value;
        if (reader.context.version >= Version.unemploymentByEducation)
        {
            reader.Read(m_UnemploymentByEducation);
        }
    }

    public void SetDefaults(Context context)
    {
        m_Unemployment.value = 0;
    }

    [Preserve]
    protected override void OnUpdate()
    {
        __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Objects_OutsideConnection_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Agents_MovingAway_RO_ComponentLookup.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Worker_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_HealthProblem_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        __TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
        CountEmploymentJob jobData = default(CountEmploymentJob);
        jobData.m_CitizenType = __TypeHandle.__Game_Citizens_Citizen_RO_ComponentTypeHandle;
        jobData.m_HealthProblemType = __TypeHandle.__Game_Citizens_HealthProblem_RO_ComponentTypeHandle;
        jobData.m_WorkerType = __TypeHandle.__Game_Citizens_Worker_RO_ComponentTypeHandle;
        jobData.m_StudentType = __TypeHandle.__Game_Citizens_Student_RO_ComponentTypeHandle;
        jobData.m_HouseholdMemberType = __TypeHandle.__Game_Citizens_HouseholdMember_RO_ComponentTypeHandle;
        jobData.m_MovingAways = __TypeHandle.__Game_Agents_MovingAway_RO_ComponentLookup;
        jobData.m_PropertyRenters = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup;
        jobData.m_OutsideConnections = __TypeHandle.__Game_Objects_OutsideConnection_RO_ComponentLookup;
        jobData.m_Households = __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
        jobData.m_Workers = m_WorkersTemp.ToConcurrent();
        jobData.m_Adults = m_AdultsTemp.ToConcurrent();
        jobData.m_Unemployed = m_UnemployedTemp.ToConcurrent();
        jobData.m_EmployableByEducation0 = m_EmployableByEducation0.ToConcurrent();
        jobData.m_EmployableByEducation1 = m_EmployableByEducation1.ToConcurrent();
        jobData.m_EmployableByEducation2 = m_EmployableByEducation2.ToConcurrent();
        jobData.m_EmployableByEducation3 = m_EmployableByEducation3.ToConcurrent();
        jobData.m_EmployableByEducation4 = m_EmployableByEducation4.ToConcurrent();
        jobData.m_UnemploymentByEducation0 = m_UnemploymentByEducation0.ToConcurrent();
        jobData.m_UnemploymentByEducation1 = m_UnemploymentByEducation1.ToConcurrent();
        jobData.m_UnemploymentByEducation2 = m_UnemploymentByEducation2.ToConcurrent();
        jobData.m_UnemploymentByEducation3 = m_UnemploymentByEducation3.ToConcurrent();
        jobData.m_UnemploymentByEducation4 = m_UnemploymentByEducation4.ToConcurrent();
        jobData.m_PotentialWorkersByEducation0 = m_PotentialWorkersByEducation0.ToConcurrent();
        jobData.m_PotentialWorkersByEducation1 = m_PotentialWorkersByEducation1.ToConcurrent();
        jobData.m_PotentialWorkersByEducation2 = m_PotentialWorkersByEducation2.ToConcurrent();
        jobData.m_PotentialWorkersByEducation3 = m_PotentialWorkersByEducation3.ToConcurrent();
        jobData.m_PotentialWorkersByEducation4 = m_PotentialWorkersByEducation4.ToConcurrent();
        JobHandle dependsOn = JobChunkExtensions.ScheduleParallel(jobData, m_AllAdultGroup, JobHandle.CombineDependencies(base.Dependency, m_ReadDependencies));
        SumEmploymentJob sumEmploymentJob = default(SumEmploymentJob);
        sumEmploymentJob.m_Adults = m_Adults;
        sumEmploymentJob.m_Unemployed = m_Unemployed;
        sumEmploymentJob.m_Unemployment = m_Unemployment;
        sumEmploymentJob.m_EmployableByEducation = m_EmployableByEducation;
        sumEmploymentJob.m_PotentialWorkersByEducation = m_PotentialWorkersByEducation;
        sumEmploymentJob.m_UnemploymentByEducation = m_UnemploymentByEducation;
        sumEmploymentJob.m_Workers = m_Workers;
        sumEmploymentJob.m_WorkersTemp = m_WorkersTemp;
        sumEmploymentJob.m_AdultsTemp = m_AdultsTemp;
        sumEmploymentJob.m_UnemployedTemp = m_UnemployedTemp;
        sumEmploymentJob.m_EmployableByEducation0 = m_EmployableByEducation0;
        sumEmploymentJob.m_EmployableByEducation1 = m_EmployableByEducation1;
        sumEmploymentJob.m_EmployableByEducation2 = m_EmployableByEducation2;
        sumEmploymentJob.m_EmployableByEducation3 = m_EmployableByEducation3;
        sumEmploymentJob.m_EmployableByEducation4 = m_EmployableByEducation4;
        sumEmploymentJob.m_UnemploymentByEducation0 = m_UnemploymentByEducation0;
        sumEmploymentJob.m_UnemploymentByEducation1 = m_UnemploymentByEducation1;
        sumEmploymentJob.m_UnemploymentByEducation2 = m_UnemploymentByEducation2;
        sumEmploymentJob.m_UnemploymentByEducation3 = m_UnemploymentByEducation3;
        sumEmploymentJob.m_UnemploymentByEducation4 = m_UnemploymentByEducation4;
        sumEmploymentJob.m_PotentialWorkersByEducation0 = m_PotentialWorkersByEducation0;
        sumEmploymentJob.m_PotentialWorkersByEducation1 = m_PotentialWorkersByEducation1;
        sumEmploymentJob.m_PotentialWorkersByEducation2 = m_PotentialWorkersByEducation2;
        sumEmploymentJob.m_PotentialWorkersByEducation3 = m_PotentialWorkersByEducation3;
        sumEmploymentJob.m_PotentialWorkersByEducation4 = m_PotentialWorkersByEducation4;
        SumEmploymentJob jobData2 = sumEmploymentJob;
        base.Dependency = IJobExtensions.Schedule(jobData2, dependsOn);
        m_WriteDependencies = base.Dependency;
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
    public CountEmploymentSystem()
    {
    }
}
