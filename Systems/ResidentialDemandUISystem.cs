using System.Runtime.CompilerServices;
using Colossal.Collections;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game.Agents;
using Game.Buildings;
using Game.Citizens;
using Game.City;
using Game.Common;
using Game.Debug;
using Game.Prefabs;
using Game.Reflection;
using Game.Tools;
using Game.Triggers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;
using Game;
using Game.Simulation;
using Game.UI;
using Colossal.UI.Binding;

namespace InfoLoom;

[CompilerGenerated]
public class ResidentialDemandUISystem : UISystemBase
{
    [BurstCompile]
    private struct UpdateResidentialDemandJob : IJob
    {
        [ReadOnly]
        public NativeList<ArchetypeChunk> m_ResidentialChunks;

        [ReadOnly]
        public NativeList<ArchetypeChunk> m_HouseholdChunks;

        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<ZonePropertiesData> m_UnlockedZones;

        [ReadOnly]
        public BufferTypeHandle<Renter> m_RenterType;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> m_PrefabType;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> m_PropertyRenterType;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> m_BuildingPropertyDatas;

        [ReadOnly]
        public ComponentLookup<Household> m_Households;

        [ReadOnly]
        public ComponentLookup<Population> m_Populations;

        [ReadOnly]
        public ComponentLookup<SpawnableBuildingData> m_SpawnableDatas;

        [ReadOnly]
        public ComponentLookup<ZonePropertiesData> m_ZonePropertyDatas;

        [ReadOnly]
        public NativeList<DemandParameterData> m_DemandParameters;

        [ReadOnly]
        public NativeValue<int> m_UnemploymentRate;

        [ReadOnly]
        public NativeValue<int> m_StudyPositions;

        [ReadOnly]
        public NativeArray<int> m_TaxRates;

        public Entity m_City;

        public NativeValue<int> m_HouseholdDemand;

        public NativeValue<int3> m_BuildingDemand;

        public NativeArray<int> m_LowDemandFactors;

        public NativeArray<int> m_MediumDemandFactors;

        public NativeArray<int> m_HighDemandFactors;

        //public NativeQueue<TriggerAction> m_TriggerQueue;

        public NativeArray<int> m_Results;

        public void Execute()
        {
            DemandParameterData demandParameterData = m_DemandParameters[0];
            //Plugin.Log($"Execute: happiness    neutral {demandParameterData.m_NeutralHappiness},    effect {demandParameterData.m_HappinessEffect}, minimum {demandParameterData.m_MinimumHappiness}");
            //Plugin.Log($"Execute: homelessness neutral {demandParameterData.m_NeutralHomelessness}, effect {demandParameterData.m_HomelessEffect}");
            //Plugin.Log($"Execute: unemployment neutral {demandParameterData.m_NeutralUnemployment}, effect {demandParameterData.m_UnemploymentEffect}");
            bool3 c = default(bool3);
            for (int i = 0; i < m_UnlockedZones.Length; i++)
            {
                if (m_UnlockedZones[i].m_ResidentialProperties > 0f)
                {
                    float num = m_UnlockedZones[i].m_ResidentialProperties / m_UnlockedZones[i].m_SpaceMultiplier;
                    if (!m_UnlockedZones[i].m_ScaleResidentials)
                    {
                        c.z = true;
                    }
                    else if (num < 1f)
                    {
                        c.y = true;
                    }
                    else
                    {
                        c.x = true;
                    }
                }
            }
			// free res properties
            int3 @int = default(int3);
			// total res properties
            int3 int2 = default(int3);
            //DemandParameterData demandParameterData = m_DemandParameters[0];
            int value = m_StudyPositions.value;
            int value2 = m_UnemploymentRate.value;
            Population population = m_Populations[m_City];
            int num2 = math.max(demandParameterData.m_MinimumHappiness, population.m_AverageHappiness);
            int num3 = 0;
            int num4 = 0;
            // loop through all households, ex. Tourist, Commuter, MovingAway, Deleted and Temp
            for (int j = 0; j < m_HouseholdChunks.Length; j++)
            {
                ArchetypeChunk archetypeChunk = m_HouseholdChunks[j];
                if (!archetypeChunk.Has(ref m_PropertyRenterType))
                {
                    // not property renter => homeless
                    num3 += archetypeChunk.Count;
                }
                num4 += archetypeChunk.Count;
            }
            float num5 = 0f;
            for (int k = 0; k <= 2; k++)
            {
                num5 -= 3f * ((float)k + 1f) * ((float)TaxSystem.GetResidentialTaxRate(k, m_TaxRates) - 10f);
            }
            float taxRate = 10f - num5 / (3f * 6f);
            //Plugin.Log($".. taxes {taxRate}: {TaxSystem.GetResidentialTaxRate(0, m_TaxRates)} {TaxSystem.GetResidentialTaxRate(1, m_TaxRates)} {TaxSystem.GetResidentialTaxRate(2, m_TaxRates)}");
            float num6 = demandParameterData.m_HappinessEffect * (float)(num2 - demandParameterData.m_NeutralHappiness);
            float num7 = (0f - demandParameterData.m_HomelessEffect) * (100f * (float)num3 / (1f + (float)num4) - demandParameterData.m_NeutralHomelessness);
            float num8 = (0f - demandParameterData.m_UnemploymentEffect) * ((float)value2 - demandParameterData.m_NeutralUnemployment);
            // value - study positions
            float num9 = math.min(math.sqrt(2f * (float)value), -1f + math.min(2.5f, math.sqrt((float)value / 300f)) + 0.5f * (num6 + num8 + num7 + num5));
            float y = num8 + num6 + num7 + num5;
            m_HouseholdDemand.value = math.min(100, math.max(0, Mathf.RoundToInt(math.max(num9, y))));
            m_Results[16] = Mathf.RoundToInt(math.max(num9, y)); // 220204 household demand
            m_LowDemandFactors[7] = Mathf.RoundToInt(num6);
            m_LowDemandFactors[8] = Mathf.RoundToInt(num7);
            m_LowDemandFactors[6] = Mathf.RoundToInt(num8);
            m_LowDemandFactors[11] = Mathf.RoundToInt(num5);
            m_MediumDemandFactors[7] = Mathf.RoundToInt(num6); // Happiness
            m_MediumDemandFactors[8] = Mathf.RoundToInt(num7); // Homelessness
            m_MediumDemandFactors[6] = Mathf.RoundToInt(num8); // Unemployment
            m_MediumDemandFactors[11] = Mathf.RoundToInt(num5); // Taxes
            m_MediumDemandFactors[12] = Mathf.RoundToInt(num9); // Students
            m_HighDemandFactors[7] = Mathf.RoundToInt(num6);
            m_HighDemandFactors[8] = Mathf.RoundToInt(num7);
            m_HighDemandFactors[6] = Mathf.RoundToInt(num8);
            m_HighDemandFactors[11] = Mathf.RoundToInt(num5);
            m_HighDemandFactors[12] = Mathf.RoundToInt(num9);
            // InfoLoom
            //Plugin.Log($".. zones {c} avgHap {population.m_AverageHappiness} study {value} unemp {value2} ratio {demandParameterData.m_FreeResidentialProportion}");
            //Plugin.Log($".. hless {num3} house {num4} [ taxE {num5} hapE {num6} hlessE {num7} unemE {num8} ] = {y} {num9}");
            // calculate empty buildings, loop through all ResidentialProperty, ex. Abandoned, Condemened, Destroyed, etc.
            for (int l = 0; l < m_ResidentialChunks.Length; l++)
            {
                ArchetypeChunk archetypeChunk2 = m_ResidentialChunks[l];
                NativeArray<PrefabRef> nativeArray = archetypeChunk2.GetNativeArray(ref m_PrefabType);
                BufferAccessor<Renter> bufferAccessor = archetypeChunk2.GetBufferAccessor(ref m_RenterType);
                // iterate through buildings
                for (int m = 0; m < nativeArray.Length; m++)
                {
                    Entity prefab = nativeArray[m].m_Prefab;
                    SpawnableBuildingData spawnableBuildingData = m_SpawnableDatas[prefab];
                    ZonePropertiesData zonePropertiesData = m_ZonePropertyDatas[spawnableBuildingData.m_ZonePrefab];
                    float num10 = zonePropertiesData.m_ResidentialProperties / zonePropertiesData.m_SpaceMultiplier;
                    if (!m_BuildingPropertyDatas.HasComponent(prefab))
                    {
                        continue;
                    }
                    BuildingPropertyData buildingPropertyData = m_BuildingPropertyDatas[prefab];
                    DynamicBuffer<Renter> dynamicBuffer = bufferAccessor[m];
					// occupied properties
                    int num11 = 0;
                    for (int n = 0; n < dynamicBuffer.Length; n++)
                    {
                        if (m_Households.HasComponent(dynamicBuffer[n].m_Renter))
                        {
                            num11++;
                        }
                    }
                    if (!zonePropertiesData.m_ScaleResidentials)
                    {
                        // low density (not scalable, only 1 household per buildinng)
                        int2.z++;
                        @int.z += 1 - num11;
                        m_Results[3] += num11; // low
                    }
                    else if (num10 < 1f)
                    {
                        // medium density, scaling < 1f
                        int2.y += buildingPropertyData.m_ResidentialProperties;
                        @int.y += buildingPropertyData.m_ResidentialProperties - num11;
                        m_Results[4] += num11; // med
                    }
                    else
                    {
                        // high density, scaling >= 1f
                        int2.x += buildingPropertyData.m_ResidentialProperties;
                        @int.x += buildingPropertyData.m_ResidentialProperties - num11;
                        m_Results[5] += num11; // high
                    }
                }
            }
            int num12 = m_LowDemandFactors[7] + m_LowDemandFactors[8] + m_LowDemandFactors[6] + m_LowDemandFactors[11] + m_LowDemandFactors[12];
            int num13 = m_MediumDemandFactors[7] + m_MediumDemandFactors[8] + m_MediumDemandFactors[6] + m_MediumDemandFactors[11] + m_MediumDemandFactors[12];
            int num14 = m_HighDemandFactors[7] + m_HighDemandFactors[8] + m_HighDemandFactors[6] + m_HighDemandFactors[11] + m_HighDemandFactors[12];
            // @float is needed free res properties (capped at min. 5)
            float3 @float = new float3(math.max(5f, 0.01f * demandParameterData.m_FreeResidentialProportion * math.max(1f, int2.x)), math.max(5f, 0.01f * demandParameterData.m_FreeResidentialProportion * math.max(1f, int2.y)), math.max(5f, 0.01f * demandParameterData.m_FreeResidentialProportion * math.max(1f, int2.z)));
            // actual demand: (needed - current) / needed => (1 - current/needed)
            // if current > needed => demand = 0
            // if current = 0 => demand = 100
            // if current < needed => demand = 0..100
            m_BuildingDemand.value = new int3((int)(100f * math.saturate((@float.x - (float)@int.x) / math.max(1f, @float.x))), (int)(100f * math.saturate((@float.y - (float)@int.y) / math.max(1f, @float.y))), (int)(100f * math.saturate((@float.z - (float)@int.z) / math.max(1f, @float.z))));
            m_BuildingDemand.value = math.select(default(int3), m_BuildingDemand.value, c);
            //Plugin.Log($".. tot {int2} needed {@float} free {@int} bulDem {m_BuildingDemand.value}");
            // EmptyBuildings
            // if sum of other factors is LOWER than raw building demand, then EmptyBuildings is 0
            // if sum of other factors is HIGHER than raw building demand, then EmptyBuildings is a diff between them
            // so, it says if CURRENT empty buildings are enough to satisfy existing demand, or MORE
            // valuable info: how many empty buildings are needed to satisfy current demand
            m_HighDemandFactors[13] = math.min(0, m_BuildingDemand.value.x - num14);
            m_MediumDemandFactors[13] = math.min(0, m_BuildingDemand.value.y - num13);
            m_LowDemandFactors[13] = math.min(0, m_BuildingDemand.value.z - num12);
            // InfoLoom: no game effects
            //m_TriggerQueue.Enqueue(new TriggerAction(TriggerType.ResidentialDemand, Entity.Null, (int2.x + int2.y + int2.z > 100) ? ((float)(m_BuildingDemand.value.x + m_BuildingDemand.value.y + m_BuildingDemand.value.z) / 100f) : 0f));
            // InfoLoom
            m_Results[0] = int2.z; // total residential properties, low
			m_Results[1] = int2.y; // total residential properties, med
			m_Results[2] = int2.x; // total residential properties, high
            m_Results[6] = Mathf.RoundToInt(10f * demandParameterData.m_FreeResidentialProportion);
            m_Results[7] = population.m_AverageHappiness;
            m_Results[8] = demandParameterData.m_NeutralHappiness;
            m_Results[9] = m_UnemploymentRate.value;
            m_Results[10] = Mathf.RoundToInt(10f * demandParameterData.m_NeutralUnemployment);
            m_Results[11] = num3; // homeless households
            m_Results[12] = num4; // households
            m_Results[13] = Mathf.RoundToInt(10f * demandParameterData.m_NeutralHomelessness);
            m_Results[14] = m_StudyPositions.value;
            m_Results[15] = Mathf.RoundToInt(10f * taxRate);
            //Plugin.Log($"TOTAL : L {m_Results[0]} M {m_Results[1]} H {m_Results[2]}");
            //Plugin.Log($"USED  : L {m_Results[3]} M {m_Results[4]} H {m_Results[5]}");
            //float mr6 = (float)m_Results[6] / 10f;
            //Plugin.Log($"FREE  : L {m_Results[0]-m_Results[3]} M {m_Results[1] - m_Results[4]} H {m_Results[2] - m_Results[5]} ratio {mr6:F1}% ");
            //Plugin.Log($"DEMAND: L {(1f - (m_Results[0] - m_Results[3]) / (0.01f * mr6 * (float)math.max(1,m_Results[0]))) * 100f:F0} " +
            //$"M {(1f - (m_Results[1] - m_Results[4]) / (0.01f * mr6 * (float)math.max(1,m_Results[1]))) * 100f:F0} " +
            //$"H {(1f - (m_Results[2] - m_Results[5]) / (0.01f * mr6 * (float)math.max(1,m_Results[2]))) * 100f:F0}");
            //Plugin.Log($"OTHER: hap {m_Results[7]}/{m_Results[8]} unem {m_Results[9]}/{m_Results[10]} hless {m_Results[11]}/{m_Results[12]}/{m_Results[13]}");
            // 240204 student ratio for new households, from HouseholdSpawnSystem
            int fact6 = math.max(0, m_LowDemandFactors[6] + m_MediumDemandFactors[6] + m_HighDemandFactors[6]); // unemployment
            int fact12 = math.max(0, m_LowDemandFactors[12] + m_MediumDemandFactors[12] + m_HighDemandFactors[12]); // students
            m_Results[17] = fact12 == 0 ? 0 : Mathf.RoundToInt(100f * (float)fact12 / (float)(fact12 + fact6));
        }
    }

    private struct TypeHandle
    {
        [ReadOnly]
        public BufferTypeHandle<Renter> __Game_Buildings_Renter_RO_BufferTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Household> __Game_Citizens_Household_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Population> __Game_City_Population_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<SpawnableBuildingData> __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<ZonePropertiesData> __Game_Prefabs_ZonePropertiesData_RO_ComponentLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Game_Buildings_Renter_RO_BufferTypeHandle = state.GetBufferTypeHandle<Renter>(isReadOnly: true);
            __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(isReadOnly: true);
            __Game_Buildings_PropertyRenter_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PropertyRenter>(isReadOnly: true);
            __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(isReadOnly: true);
            __Game_Citizens_Household_RO_ComponentLookup = state.GetComponentLookup<Household>(isReadOnly: true);
            __Game_City_Population_RO_ComponentLookup = state.GetComponentLookup<Population>(isReadOnly: true);
            __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup = state.GetComponentLookup<SpawnableBuildingData>(isReadOnly: true);
            __Game_Prefabs_ZonePropertiesData_RO_ComponentLookup = state.GetComponentLookup<ZonePropertiesData>(isReadOnly: true);
        }
    }

    private const string kGroup = "cityInfo";

    private SimulationSystem m_SimulationSystem;

    private TaxSystem m_TaxSystem;

    private CountEmploymentSystem m_CountEmploymentSystem;

    private CountStudyPositionsSystem m_CountStudyPositionsSystem;

    private CitySystem m_CitySystem;

    //private TriggerSystem m_TriggerSystem;

    private EntityQuery m_DemandParameterGroup;

    private EntityQuery m_AllHouseholdGroup;

    private EntityQuery m_AllResidentialGroup;

    private EntityQuery m_UnlockedZoneQuery;

    private NativeValue<int> m_HouseholdDemand;

    private NativeValue<int3> m_BuildingDemand;

    //[EnumArray(typeof(DemandFactor))]
    //[DebugWatchValue]
    private NativeArray<int> m_LowDemandFactors;

    //[EnumArray(typeof(DemandFactor))]
    //[DebugWatchValue]
    private NativeArray<int> m_MediumDemandFactors;

    //[EnumArray(typeof(DemandFactor))]
    //[DebugWatchValue]
    private NativeArray<int> m_HighDemandFactors;

    //[DebugWatchDeps]
    //private JobHandle m_WriteDependencies;

    private JobHandle m_ReadDependencies;

    private int m_LastHouseholdDemand;

    private int3 m_LastBuildingDemand;

    private TypeHandle __TypeHandle;

    //[DebugWatchValue(color = "#27ae60")]
    public int householdDemand => m_LastHouseholdDemand;

    //[DebugWatchValue(color = "#117a65")]
    public int3 buildingDemand => m_LastBuildingDemand;

    // InfoLoom

    private RawValueBinding m_uiResults;

    private NativeArray<int> m_Results;

    // RESIDENTIAL
    // 0,1,2 - count of residential properties, low/med/high
    // 3,4,5 - count of occupied properties, low/med/high
    // 6 - free residential ratio (%) * 10
    // 7 & 8 - AverageHappiness, neutral happiness
    // 9 & 10 - unemployment value, neutral unemployment (%) * 10
    // 11, 12, 13 - homeless households, total households, NeutralHomelessness (%) * 10
    // 14 - study positions
    // 15 - tax rate (weighted)

    // 240209 Set gameMode to avoid errors in the Editor
    public override GameMode gameMode => GameMode.Game;

    /* not used
    public NativeArray<int> GetLowDensityDemandFactors(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_LowDemandFactors;
    }

    public NativeArray<int> GetMediumDensityDemandFactors(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_MediumDemandFactors;
    }

    public NativeArray<int> GetHighDensityDemandFactors(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_HighDemandFactors;
    }
    */

    public void AddReader(JobHandle reader)
    {
        m_ReadDependencies = JobHandle.CombineDependencies(m_ReadDependencies, reader);
    }

    [Preserve]
    protected override void OnCreate()
    {
        base.OnCreate();
        m_SimulationSystem = base.World.GetOrCreateSystemManaged<SimulationSystem>(); // TODO: use UIUpdateState eventually
        m_DemandParameterGroup = GetEntityQuery(ComponentType.ReadOnly<DemandParameterData>());
        m_AllHouseholdGroup = GetEntityQuery(ComponentType.ReadOnly<Household>(), ComponentType.Exclude<TouristHousehold>(), ComponentType.Exclude<CommuterHousehold>(), ComponentType.Exclude<MovingAway>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Temp>());
        m_AllResidentialGroup = GetEntityQuery(ComponentType.ReadOnly<ResidentialProperty>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Condemned>(), ComponentType.Exclude<Abandoned>(), ComponentType.Exclude<Destroyed>(), ComponentType.Exclude<Temp>());
        m_UnlockedZoneQuery = GetEntityQuery(ComponentType.ReadOnly<ZoneData>(), ComponentType.ReadOnly<ZonePropertiesData>(), ComponentType.Exclude<Locked>());
        m_CitySystem = base.World.GetOrCreateSystemManaged<CitySystem>();
        m_TaxSystem = base.World.GetOrCreateSystemManaged<TaxSystem>();
        m_CountEmploymentSystem = base.World.GetOrCreateSystemManaged<CountEmploymentSystem>();
        m_CountStudyPositionsSystem = base.World.GetOrCreateSystemManaged<CountStudyPositionsSystem>();
        //m_TriggerSystem = base.World.GetOrCreateSystemManaged<TriggerSystem>();
        m_HouseholdDemand = new NativeValue<int>(Allocator.Persistent);
        m_BuildingDemand = new NativeValue<int3>(Allocator.Persistent);
        m_LowDemandFactors = new NativeArray<int>(18, Allocator.Persistent);
        m_MediumDemandFactors = new NativeArray<int>(18, Allocator.Persistent);
        m_HighDemandFactors = new NativeArray<int>(18, Allocator.Persistent);

        // InfoLoom
        SetDefaults(); // there is no serialization, so init just for safety
        m_Results = new NativeArray<int>(18, Allocator.Persistent);

        AddBinding(m_uiResults = new RawValueBinding(kGroup, "ilResidential", delegate (IJsonWriter binder)
        {
            binder.ArrayBegin(m_Results.Length);
            for (int i = 0; i < m_Results.Length; i++)
                binder.Write(m_Results[i]);
            binder.ArrayEnd();
        }));

        Plugin.Log("ResidentialDemandUISystem created.");
    }

    [Preserve]
    protected override void OnDestroy()
    {
        m_HouseholdDemand.Dispose();
        m_BuildingDemand.Dispose();
        m_LowDemandFactors.Dispose();
        m_MediumDemandFactors.Dispose();
        m_HighDemandFactors.Dispose();
        // InfoLoom
        m_Results.Dispose();
        base.OnDestroy();
    }

    public void SetDefaults() //Context context)
    {
        m_HouseholdDemand.value = 0;
        m_BuildingDemand.value = default(int3);
        m_LowDemandFactors.Fill(0);
        m_MediumDemandFactors.Fill(0);
        m_HighDemandFactors.Fill(0);
        m_LastHouseholdDemand = 0;
        m_LastBuildingDemand = default(int3);
    }

    /* not used
    public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
    {
        writer.Write(m_HouseholdDemand.value);
        writer.Write(m_BuildingDemand.value);
        writer.Write(m_LowDemandFactors.Length);
        writer.Write(m_LowDemandFactors);
        writer.Write(m_MediumDemandFactors);
        writer.Write(m_HighDemandFactors);
        writer.Write(m_LastHouseholdDemand);
        writer.Write(m_LastBuildingDemand);
    }
    */

    /* not used
    public void Deserialize<TReader>(TReader reader) where TReader : IReader
    {
        reader.Read(out int value);
        m_HouseholdDemand.value = value;
        if (reader.context.version < Version.residentialDemandSplit)
        {
            reader.Read(out int value2);
            m_BuildingDemand.value = new int3(value2 / 3, value2 / 3, value2 / 3);
        }
        else
        {
            reader.Read(out int3 value3);
            m_BuildingDemand.value = value3;
        }
        if (reader.context.version < Version.demandFactorCountSerialization)
        {
            NativeArray<int> nativeArray = new NativeArray<int>(13, Allocator.Temp);
            reader.Read(nativeArray);
            CollectionUtils.CopySafe(nativeArray, m_LowDemandFactors);
            nativeArray.Dispose();
        }
        else
        {
            reader.Read(out int value4);
            if (value4 == m_LowDemandFactors.Length)
            {
                reader.Read(m_LowDemandFactors);
                reader.Read(m_MediumDemandFactors);
                reader.Read(m_HighDemandFactors);
            }
            else
            {
                NativeArray<int> nativeArray2 = new NativeArray<int>(value4, Allocator.Temp);
                reader.Read(nativeArray2);
                CollectionUtils.CopySafe(nativeArray2, m_LowDemandFactors);
                reader.Read(nativeArray2);
                CollectionUtils.CopySafe(nativeArray2, m_MediumDemandFactors);
                reader.Read(nativeArray2);
                CollectionUtils.CopySafe(nativeArray2, m_HighDemandFactors);
                nativeArray2.Dispose();
            }
        }
        reader.Read(out m_LastHouseholdDemand);
        if (reader.context.version < Version.residentialDemandSplit)
        {
            reader.Read(out int value5);
            m_LastBuildingDemand = new int3(value5 / 3, value5 / 3, value5 / 3);
        }
        else
        {
            reader.Read(out m_LastBuildingDemand);
        }
    }
    */

    [Preserve]
    protected override void OnUpdate()
    {
        if (m_SimulationSystem.frameIndex % 128 != 11)
            return;
        //Plugin.Log($"OnUpdate: {m_SimulationSystem.frameIndex}");
        base.OnUpdate();
        ResetResults();

        if (!m_DemandParameterGroup.IsEmptyIgnoreFilter)
        {
            m_LastHouseholdDemand = m_HouseholdDemand.value;
            m_LastBuildingDemand = m_BuildingDemand.value;
            __TypeHandle.__Game_Prefabs_ZonePropertiesData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_City_Population_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Buildings_Renter_RO_BufferTypeHandle.Update(ref base.CheckedStateRef);
            UpdateResidentialDemandJob updateResidentialDemandJob = default(UpdateResidentialDemandJob);
            updateResidentialDemandJob.m_ResidentialChunks = m_AllResidentialGroup.ToArchetypeChunkListAsync(base.World.UpdateAllocator.ToAllocator, out var outJobHandle);
            updateResidentialDemandJob.m_HouseholdChunks = m_AllHouseholdGroup.ToArchetypeChunkListAsync(base.World.UpdateAllocator.ToAllocator, out var outJobHandle2);
            updateResidentialDemandJob.m_UnlockedZones = m_UnlockedZoneQuery.ToComponentDataArray<ZonePropertiesData>(Allocator.TempJob);
            updateResidentialDemandJob.m_RenterType = __TypeHandle.__Game_Buildings_Renter_RO_BufferTypeHandle;
            updateResidentialDemandJob.m_PrefabType = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
            updateResidentialDemandJob.m_PropertyRenterType = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentTypeHandle;
            updateResidentialDemandJob.m_BuildingPropertyDatas = __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
            updateResidentialDemandJob.m_Households = __TypeHandle.__Game_Citizens_Household_RO_ComponentLookup;
            updateResidentialDemandJob.m_Populations = __TypeHandle.__Game_City_Population_RO_ComponentLookup;
            updateResidentialDemandJob.m_SpawnableDatas = __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
            updateResidentialDemandJob.m_ZonePropertyDatas = __TypeHandle.__Game_Prefabs_ZonePropertiesData_RO_ComponentLookup;
            updateResidentialDemandJob.m_DemandParameters = m_DemandParameterGroup.ToComponentDataListAsync<DemandParameterData>(base.World.UpdateAllocator.ToAllocator, out var outJobHandle3);
            updateResidentialDemandJob.m_UnemploymentRate = m_CountEmploymentSystem.GetUnemployment(out var deps);
            updateResidentialDemandJob.m_StudyPositions = m_CountStudyPositionsSystem.GetStudyPositions(out var deps2);
            updateResidentialDemandJob.m_TaxRates = m_TaxSystem.GetTaxRates();
            updateResidentialDemandJob.m_City = m_CitySystem.City;
            updateResidentialDemandJob.m_HouseholdDemand = m_HouseholdDemand;
            updateResidentialDemandJob.m_BuildingDemand = m_BuildingDemand;
            updateResidentialDemandJob.m_LowDemandFactors = m_LowDemandFactors;
            updateResidentialDemandJob.m_MediumDemandFactors = m_MediumDemandFactors;
            updateResidentialDemandJob.m_HighDemandFactors = m_HighDemandFactors;
            //updateResidentialDemandJob.m_TriggerQueue = m_TriggerSystem.CreateActionBuffer();
            updateResidentialDemandJob.m_Results = m_Results;
            UpdateResidentialDemandJob jobData = updateResidentialDemandJob;
            IJobExtensions.Schedule(jobData, JobUtils.CombineDependencies(base.Dependency, m_ReadDependencies, outJobHandle, outJobHandle2, outJobHandle3, deps, deps2)).Complete();
            // since this is a copy of an actual simulation system but for UI purposes, then noone will read from us or wait for us
            //m_WriteDependencies = base.Dependency;
            //m_CountEmploymentSystem.AddReader(base.Dependency);
            //m_CountStudyPositionsSystem.AddReader(base.Dependency);
            //m_TaxSystem.AddReader(base.Dependency);
            //m_TriggerSystem.AddActionBufferWriter(base.Dependency);
        }
        // Update UI
        m_uiResults.Update();
    }

    private void ResetResults()
    {
        m_Results.Fill<int>(0);
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
    public ResidentialDemandUISystem()
    {
    }
}
