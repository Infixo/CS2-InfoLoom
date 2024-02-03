using System.Runtime.CompilerServices;
using Colossal.Collections;
using Colossal.Entities;
using Colossal.Serialization.Entities;
using Game.Buildings;
using Game.City;
using Game.Common;
using Game.Companies;
using Game.Debug;
using Game.Economy;
using Game.Objects;
using Game.Prefabs;
using Game.Reflection;
using Game.Tools;
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
using System.Collections.Generic;

namespace InfoLoom;

[CompilerGenerated]
public class IndustrialDemandUISystem : UISystemBase
{
    [BurstCompile]
    private struct UpdateIndustrialDemandJob : IJob
    {
        [ReadOnly]
        public NativeList<ArchetypeChunk> m_FreePropertyChunks;

        [ReadOnly]
        public NativeList<ArchetypeChunk> m_StorageCompanyChunks;

        [ReadOnly]
        public NativeList<ArchetypeChunk> m_IndustrialProcessDataChunks;

        [ReadOnly]
        public NativeList<ArchetypeChunk> m_CityServiceChunks;

        [ReadOnly]
        public NativeList<ArchetypeChunk> m_SpawnableChunks;

        [ReadOnly]
        public EntityTypeHandle m_EntityType;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> m_PrefabType;

        [ReadOnly]
        public ComponentTypeHandle<IndustrialProcessData> m_ProcessType;

        [ReadOnly]
        public ComponentTypeHandle<CityServiceUpkeep> m_ServiceUpkeepType;

        [ReadOnly]
        public ComponentLookup<IndustrialProcessData> m_IndustrialProcessDatas;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> m_PropertyRenters;

        [ReadOnly]
        public ComponentLookup<PrefabRef> m_Prefabs;

        [ReadOnly]
        public ComponentLookup<BuildingData> m_BuildingDatas;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> m_BuildingPropertyDatas;

        [ReadOnly]
        public ComponentLookup<Attached> m_Attached;

        [ReadOnly]
        public ComponentLookup<ResourceData> m_ResourceDatas;

        [ReadOnly]
        public ComponentLookup<WorkplaceData> m_WorkplaceDatas;

        [ReadOnly]
        public ComponentLookup<ConsumptionData> m_ConsumptionDatas;

        [ReadOnly]
        public ComponentLookup<StorageLimitData> m_StorageLimitDatas;

        [ReadOnly]
        public ComponentLookup<SpawnableBuildingData> m_SpawnableBuildingDatas;

        [ReadOnly]
        public BufferLookup<ServiceUpkeepData> m_ServiceUpkeeps;

        [ReadOnly]
        public BufferLookup<CityModifier> m_CityModifiers;

        [ReadOnly]
        public BufferLookup<InstalledUpgrade> m_InstalledUpgrades;

        [ReadOnly]
        public BufferLookup<ServiceUpkeepData> m_Upkeeps;

        [ReadOnly]
        public ComponentLookup<Population> m_Populations;

        [ReadOnly]
        public ComponentLookup<Tourism> m_Tourisms;

        [ReadOnly]
        public BufferLookup<TradeCost> m_TradeCosts;

        public EconomyParameterData m_EconomyParameters;

        public DemandParameterData m_DemandParameters;

        [ReadOnly]
        public ResourcePrefabs m_ResourcePrefabs;

        [ReadOnly]
        public NativeArray<int> m_EmployableByEducation;

        [ReadOnly]
        public NativeArray<int> m_TaxRates;

        [ReadOnly]
        public NativeArray<int> m_FreeWorkplaces;

        public Entity m_City;

        public float m_HeatingNeed;

        public float m_BaseConsumptionSum;

        public NativeValue<int> m_IndustrialCompanyDemand;

        public NativeValue<int> m_IndustrialBuildingDemand;

        public NativeValue<int> m_StorageCompanyDemand;

        public NativeValue<int> m_StorageBuildingDemand;

        public NativeValue<int> m_OfficeCompanyDemand;

        public NativeValue<int> m_OfficeBuildingDemand;

        public NativeArray<int> m_DemandFactors;

        public NativeArray<int> m_OfficeDemandFactors;

        public NativeArray<int> m_IndustrialDemands;

        public NativeArray<int> m_IndustrialZoningDemands;

        public NativeArray<int> m_IndustrialBuildingDemands;

        public NativeArray<int> m_StorageBuildingDemands;

        public NativeArray<int> m_StorageCompanyDemands;

        [ReadOnly]
        public NativeArray<int> m_Productions;

        [ReadOnly]
        public NativeArray<int> m_Demands;

        [ReadOnly]
        public NativeArray<int> m_Companies;

        public NativeArray<int> m_FreeProperties;

        [ReadOnly]
        public NativeArray<int> m_Propertyless;

        [ReadOnly]
        public NativeArray<int> m_TotalMaxWorkers;

        [ReadOnly]
        public NativeArray<int> m_TotalCurrentWorkers;

        public NativeArray<int> m_FreeStorages;

        public NativeArray<int> m_Storages;

        public NativeArray<int> m_StorageCapacities;

        public NativeArray<int> m_CachedDemands;

        public NativeArray<int> m_Results; // InfoLoom

        public NativeValue<Resource> m_ExcludedResources; // InfoLoom

        public void Execute()
        {
            DynamicBuffer<CityModifier> modifiers = m_CityModifiers[m_City];
            DynamicBuffer<TradeCost> costs = m_TradeCosts[m_City];
            Population population = m_Populations[m_City];
            Tourism tourism = m_Tourisms[m_City];
            ResourceIterator iterator = ResourceIterator.GetIterator();
            while (iterator.Next())
            {
                int resourceIndex = EconomyUtils.GetResourceIndex(iterator.resource);
                int y = DemandUtils.EstimateResourceDemand(iterator.resource, ref m_EconomyParameters, (population.m_Population + population.m_PopulationWithMoveIn) / 2, tourism.m_AverageTourists, m_ResourcePrefabs, m_ResourceDatas, m_BaseConsumptionSum) / 4;
                m_CachedDemands[resourceIndex] = math.max(m_Demands[resourceIndex], y);
                m_FreeProperties[resourceIndex] = 0;
                m_Storages[resourceIndex] = 0;
                m_FreeStorages[resourceIndex] = 0;
                m_StorageCapacities[resourceIndex] = 0;
                // Infixo: uncomment to see consumption per citizen (useful for balancing purposes)
                int population2 = (population.m_Population + population.m_PopulationWithMoveIn) / 2;
                Plugin.Log($"Per cim {iterator.resource}: est consumption {(float)y/(float)population2:F2}, demand {m_Demands[resourceIndex]/(float)population2:F2}");
            }
            for (int i = 0; i < m_DemandFactors.Length; i++)
            {
                m_DemandFactors[i] = 0;
            }
            for (int j = 0; j < m_OfficeDemandFactors.Length; j++)
            {
                m_OfficeDemandFactors[j] = 0;
            }
            // Add city services upkeep
            for (int k = 0; k < m_CityServiceChunks.Length; k++)
            {
                ArchetypeChunk archetypeChunk = m_CityServiceChunks[k];
                if (!archetypeChunk.Has(ref m_ServiceUpkeepType))
                {
                    continue;
                }
                NativeArray<Entity> nativeArray = archetypeChunk.GetNativeArray(m_EntityType);
                NativeArray<PrefabRef> nativeArray2 = archetypeChunk.GetNativeArray(ref m_PrefabType);
                for (int l = 0; l < nativeArray2.Length; l++)
                {
                    Entity prefab = nativeArray2[l].m_Prefab;
                    Entity entity = nativeArray[l];
                    if (m_ServiceUpkeeps.HasBuffer(prefab))
                    {
                        DynamicBuffer<ServiceUpkeepData> dynamicBuffer = m_ServiceUpkeeps[prefab];
                        for (int m = 0; m < dynamicBuffer.Length; m++)
                        {
                            ServiceUpkeepData serviceUpkeepData = dynamicBuffer[m];
                            if (serviceUpkeepData.m_Upkeep.m_Resource != Resource.Money)
                            {
                                int amount = serviceUpkeepData.m_Upkeep.m_Amount;
                                m_CachedDemands[EconomyUtils.GetResourceIndex(serviceUpkeepData.m_Upkeep.m_Resource)] += amount;
                            }
                        }
                    }
                    if (!m_InstalledUpgrades.HasBuffer(entity))
                    {
                        continue;
                    }
                    DynamicBuffer<InstalledUpgrade> dynamicBuffer2 = m_InstalledUpgrades[entity];
                    for (int n = 0; n < dynamicBuffer2.Length; n++)
                    {
                        Entity upgrade = dynamicBuffer2[n].m_Upgrade;
                        if (!m_Prefabs.HasComponent(upgrade))
                        {
                            continue;
                        }
                        Entity prefab2 = m_Prefabs[upgrade].m_Prefab;
                        if (m_Upkeeps.HasBuffer(prefab2))
                        {
                            DynamicBuffer<ServiceUpkeepData> dynamicBuffer3 = m_Upkeeps[prefab2];
                            for (int num = 0; num < dynamicBuffer3.Length; num++)
                            {
                                ServiceUpkeepData serviceUpkeepData2 = dynamicBuffer3[num];
                                m_CachedDemands[EconomyUtils.GetResourceIndex(serviceUpkeepData2.m_Upkeep.m_Resource)] += serviceUpkeepData2.m_Upkeep.m_Amount;
                            }
                        }
                    }
                }
            }
            // Add spawnable buildings demand for Timber, Concrete, Petrochemicals and Wood
            float num2 = 0f;
            float num3 = 0f;
            float num4 = 0f;
            float num5 = 0f;
            float price = m_ResourceDatas[m_ResourcePrefabs[Resource.Timber]].m_Price;
            float price2 = m_ResourceDatas[m_ResourcePrefabs[Resource.Concrete]].m_Price;
            float price3 = m_ResourceDatas[m_ResourcePrefabs[Resource.Petrochemicals]].m_Price;
            float price4 = m_ResourceDatas[m_ResourcePrefabs[Resource.Wood]].m_Price;
            for (int num6 = 0; num6 < m_SpawnableChunks.Length; num6++)
            {
                NativeArray<PrefabRef> nativeArray3 = m_SpawnableChunks[num6].GetNativeArray(ref m_PrefabType);
                for (int num7 = 0; num7 < nativeArray3.Length; num7++)
                {
                    Entity prefab3 = nativeArray3[num7].m_Prefab;
                    if (m_ConsumptionDatas.HasComponent(prefab3))
                    {
                        int num8 = m_ConsumptionDatas[prefab3].m_Upkeep / BuildingUpkeepSystem.kMaterialUpkeep;
                        num2 += (float)num8 / price / 2f;
                        num3 += (float)num8 / price2 / 2f;
                    }
                    BuildingData buildingData = m_BuildingDatas[prefab3];
                    BuildingPropertyData buildingPropertyData = m_BuildingPropertyDatas[prefab3];
                    float num9 = math.sqrt(buildingData.m_LotSize.x * buildingData.m_LotSize.y * buildingPropertyData.CountProperties()) * m_HeatingNeed;
                    num4 += 0.5f * num9 / (5f * price3);
                    num5 += 0.5f * num9 / price4;
                }
            }
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Timber)] += Mathf.RoundToInt(num2);
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Concrete)] += Mathf.RoundToInt(num3);
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Petrochemicals)] += Mathf.RoundToInt(num4);
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Wood)] += Mathf.RoundToInt(num5);
            Plugin.Log($"Spawnable demand: Timber {num2} Concrete {num3} Petrochem {num4} Wood {num5}");
            // Add industrial demand for some specific resources
            int num10 = 0;
            int num11 = 0;
            for (int num12 = 0; num12 < m_Productions.Length; num12++)
            {
                Resource resource = EconomyUtils.GetResource(num12);
                ResourceData resourceData = m_ResourceDatas[m_ResourcePrefabs[resource]];
                if (resourceData.m_IsProduceable)
                {
                    if (resourceData.m_Weight > 0f)
                    {
                        num10 += m_Productions[num12];
                    }
                    else
                    {
                        num11 += m_Productions[num12];
                    }
                }
            }
            int num13 = num11 + num10;
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Machinery)] += num10 / 2000;
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Paper)] += num11 / 4000;
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Furniture)] += num11 / 4000;
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Software)] += num13 / 2000;
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Financial)] += num13 / 2000;
            m_CachedDemands[EconomyUtils.GetResourceIndex(Resource.Telecom)] += num13 / 2000;
            Plugin.Log($"Industrial demand: Machinery {num10/2000} Paper {num11/4000} Furniture {num11/4000} Software {num13/2000} Financial {num13/2000} Telecom {num13/2000}");
            // Count storage capacities
            for (int num14 = 0; num14 < m_StorageCompanyChunks.Length; num14++)
            {
                ArchetypeChunk archetypeChunk2 = m_StorageCompanyChunks[num14];
                NativeArray<Entity> nativeArray4 = archetypeChunk2.GetNativeArray(m_EntityType);
                NativeArray<PrefabRef> nativeArray5 = archetypeChunk2.GetNativeArray(ref m_PrefabType);
                for (int num15 = 0; num15 < nativeArray4.Length; num15++)
                {
                    Entity entity2 = nativeArray4[num15];
                    Entity prefab4 = nativeArray5[num15].m_Prefab;
                    if (m_IndustrialProcessDatas.HasComponent(prefab4))
                    {
                        int resourceIndex2 = EconomyUtils.GetResourceIndex(m_IndustrialProcessDatas[prefab4].m_Output.m_Resource);
                        m_Storages[resourceIndex2]++;
                        StorageLimitData storageLimitData = m_StorageLimitDatas[prefab4];
                        if (!m_PropertyRenters.HasComponent(entity2) || !m_Prefabs.HasComponent(m_PropertyRenters[entity2].m_Property))
                        {
                            m_FreeStorages[resourceIndex2]--;
                            m_StorageCapacities[resourceIndex2] += kStorageCompanyEstimateLimit;
                        }
                        else
                        {
                            Entity property = m_PropertyRenters[entity2].m_Property;
                            Entity prefab5 = m_Prefabs[property].m_Prefab;
                            m_StorageCapacities[resourceIndex2] += storageLimitData.GetAdjustedLimit(m_SpawnableBuildingDatas[prefab5], m_BuildingDatas[prefab5]);
                        }
                    }
                }
            }
            // Count free properties and free storages
            for (int num16 = 0; num16 < m_FreePropertyChunks.Length; num16++)
            {
                ArchetypeChunk archetypeChunk3 = m_FreePropertyChunks[num16];
                NativeArray<Entity> nativeArray6 = archetypeChunk3.GetNativeArray(m_EntityType);
                NativeArray<PrefabRef> nativeArray7 = archetypeChunk3.GetNativeArray(ref m_PrefabType);
                for (int num17 = 0; num17 < nativeArray7.Length; num17++)
                {
                    Entity prefab6 = nativeArray7[num17].m_Prefab;
                    if (!m_BuildingPropertyDatas.HasComponent(prefab6))
                    {
                        continue;
                    }
                    BuildingPropertyData buildingPropertyData2 = m_BuildingPropertyDatas[prefab6];
                    if (m_Attached.TryGetComponent(nativeArray6[num17], out var componentData) && m_Prefabs.TryGetComponent(componentData.m_Parent, out var componentData2) && m_BuildingPropertyDatas.TryGetComponent(componentData2.m_Prefab, out var componentData3))
                    {
                        buildingPropertyData2.m_AllowedManufactured &= componentData3.m_AllowedManufactured;
                    }
                    ResourceIterator iterator2 = ResourceIterator.GetIterator();
                    while (iterator2.Next())
                    {
                        int resourceIndex3 = EconomyUtils.GetResourceIndex(iterator2.resource);
                        if ((buildingPropertyData2.m_AllowedManufactured & iterator2.resource) != Resource.NoResource)
                        {
                            m_FreeProperties[resourceIndex3]++;
                        }
                        if ((buildingPropertyData2.m_AllowedStored & iterator2.resource) != Resource.NoResource)
                        {
                            m_FreeStorages[resourceIndex3]++;
                        }
                    }
                    // InfoLoom
                    if (buildingPropertyData2.m_AllowedManufactured != Resource.NoResource)
                    {
                        // TODO: COUNT m_FreeProperties[resourceIndex3]++;
                        Plugin.Log($"Free industry: {buildingPropertyData2.m_AllowedManufactured}");
                    }
                    if (buildingPropertyData2.m_AllowedStored != Resource.NoResource)
                    {
                        // TODO: COUNT m_FreeStorages[resourceIndex3]++;
                        Plugin.Log($"Free storage : {buildingPropertyData2.m_AllowedStored}");
                    }
                }
            }
            // MAIN LOOP, demand calculation per resource
            bool flag = m_IndustrialBuildingDemand.value > 0;
            bool flag2 = m_OfficeBuildingDemand.value > 0;
            bool flag3 = m_StorageBuildingDemand.value > 0;
            m_IndustrialCompanyDemand.value = 0;
            m_IndustrialBuildingDemand.value = 0;
            m_StorageCompanyDemand.value = 0;
            m_StorageBuildingDemand.value = 0;
            m_OfficeCompanyDemand.value = 0;
            m_OfficeBuildingDemand.value = 0;
            int num18 = 0; // counts all office resources
            int num19 = 0; // counts all indutry resources
            iterator = ResourceIterator.GetIterator();
            while (iterator.Next())
            {
                int resourceIndex4 = EconomyUtils.GetResourceIndex(iterator.resource);
                if (!m_ResourceDatas.HasComponent(m_ResourcePrefabs[iterator.resource]))
                {
                    continue;
                }
                ResourceData resourceData2 = m_ResourceDatas[m_ResourcePrefabs[iterator.resource]];
                bool isProduceable = resourceData2.m_IsProduceable;
                bool isMaterial = resourceData2.m_IsMaterial;
                bool isTradable = resourceData2.m_IsTradable;
                bool flag4 = resourceData2.m_Weight == 0f; // IMMATERIAL RESOURCE
                if (isTradable && !flag4)
                {
                    int num20 = m_CachedDemands[resourceIndex4];
                    m_StorageCompanyDemands[resourceIndex4] = 0;
                    m_StorageBuildingDemands[resourceIndex4] = 0;
                    if (num20 > kStorageProductionDemand && m_StorageCapacities[resourceIndex4] < num20)
                    {
                        m_StorageCompanyDemands[resourceIndex4] = 1;
                    }
                    if (m_FreeStorages[resourceIndex4] < 0)
                    {
                        m_StorageBuildingDemands[resourceIndex4] = 1;
                    }
                    m_StorageCompanyDemand.value += m_StorageCompanyDemands[resourceIndex4];
                    m_StorageBuildingDemand.value += m_StorageBuildingDemands[resourceIndex4];
                    m_DemandFactors[17] += math.max(0, m_StorageBuildingDemands[resourceIndex4]);
                }
                if (!isProduceable)
                {
                    continue;
                }
                float value = (isMaterial ? m_DemandParameters.m_ExtractorBaseDemand : m_DemandParameters.m_IndustrialBaseDemand);
                float num21 = (1f + (float)m_CachedDemands[resourceIndex4] - (float)m_Productions[resourceIndex4]) / ((float)m_CachedDemands[resourceIndex4] + 1f);
                _ = resourceData2.m_Price / resourceData2.m_Weight;
                TradeCost tradeCost = EconomyUtils.GetTradeCost(EconomyUtils.GetResource(resourceIndex4), costs);
                float num22 = (0.05f + tradeCost.m_SellCost) / resourceData2.m_Price;
                float num23 = (0.05f + tradeCost.m_BuyCost) / resourceData2.m_Price;
                num21 *= ((m_Productions[resourceIndex4] > m_CachedDemands[resourceIndex4]) ? (10f * num22) : (10f * num23));
                if (iterator.resource == Resource.Electronics)
                {
                    CityUtils.ApplyModifier(ref value, modifiers, CityModifierType.IndustrialElectronicsDemand);
                }
                else if (iterator.resource == Resource.Software)
                {
                    CityUtils.ApplyModifier(ref value, modifiers, CityModifierType.OfficeSoftwareDemand);
                }
                float num24 = -1.8f + 2.5f * (((float)m_TotalCurrentWorkers[resourceIndex4] + 1f) / ((float)m_TotalMaxWorkers[resourceIndex4] + 1f));
                EconomyUtils.GetProcessComplexity(m_IndustrialProcessDataChunks, m_WorkplaceDatas, iterator.resource, m_EntityType, m_ProcessType, out var complexity);
                Workplaces workplaces = WorkProviderSystem.CalculateNumberOfWorkplaces(20, complexity, 3);
                float num25 = 0f;
                for (int num26 = 0; num26 < 5; num26++)
                {
                    num25 = ((num26 >= 2) ? (num25 + math.min(5f * (float)workplaces[num26], math.max(0, m_EmployableByEducation[num26] - m_FreeWorkplaces[num26]))) : (num25 + 5f * (float)workplaces[num26]));
                }
                float num27 = 1f * (num25 / 50f - 1f);
                int num28 = (flag4 ? TaxSystem.GetOfficeTaxRate(iterator.resource, m_TaxRates) : TaxSystem.GetIndustrialTaxRate(iterator.resource, m_TaxRates));
                float num29 = -0.1f * ((float)num28 - 10f);
                float num30 = 0f;
                if (!flag4) // weight > 0
                {
                    m_IndustrialDemands[resourceIndex4] = Mathf.RoundToInt(100f * (math.max(-1f, value * num21) + num24 + num27 + num29));
                    if (!isMaterial)
                    {
                        num30 = float.NegativeInfinity;
                        for (int num31 = 0; num31 < m_IndustrialProcessDataChunks.Length; num31++)
                        {
                            ArchetypeChunk archetypeChunk4 = m_IndustrialProcessDataChunks[num31];
                            NativeArray<IndustrialProcessData> nativeArray8 = archetypeChunk4.GetNativeArray(ref m_ProcessType);
                            for (int num32 = 0; num32 < archetypeChunk4.Count; num32++)
                            {
                                IndustrialProcessData industrialProcessData = nativeArray8[num32];
                                if (industrialProcessData.m_Output.m_Resource == iterator.resource && industrialProcessData.m_Input1.m_Resource != iterator.resource)
                                {
                                    float num33 = 0f;
                                    bool flag5 = false;
                                    if (industrialProcessData.m_Input1.m_Amount != 0)
                                    {
                                        Entity entity3 = m_ResourcePrefabs[industrialProcessData.m_Input1.m_Resource];
                                        int resourceIndex5 = EconomyUtils.GetResourceIndex(industrialProcessData.m_Input1.m_Resource);
                                        float num34 = math.max(-3f, (float)(m_Productions[resourceIndex5] - m_CachedDemands[resourceIndex5]) / ((float)m_Productions[resourceIndex5] + 1f));
                                        ResourceData resourceData3 = m_ResourceDatas[entity3];
                                        _ = resourceData3.m_Weight / resourceData3.m_Price;
                                        float num35 = EconomyUtils.GetTradeCost(industrialProcessData.m_Input1.m_Resource, costs).m_BuyCost / resourceData3.m_Price;
                                        num33 += 10f * num35 * (num34 + 0.1f);
                                        flag5 = true;
                                    }
                                    if (industrialProcessData.m_Input2.m_Amount != 0)
                                    {
                                        Entity entity4 = m_ResourcePrefabs[industrialProcessData.m_Input2.m_Resource];
                                        int resourceIndex6 = EconomyUtils.GetResourceIndex(industrialProcessData.m_Input2.m_Resource);
                                        float num36 = math.max(-3f, (float)(m_Productions[resourceIndex6] - m_CachedDemands[resourceIndex6]) / ((float)m_Productions[resourceIndex6] + 1f));
                                        ResourceData resourceData4 = m_ResourceDatas[entity4];
                                        _ = resourceData4.m_Weight / resourceData4.m_Price;
                                        float num37 = EconomyUtils.GetTradeCost(industrialProcessData.m_Input2.m_Resource, costs).m_BuyCost / resourceData4.m_Price;
                                        num33 += 10f * num37 * (num36 + 0.1f);
                                        flag5 = true;
                                    }
                                    if (flag5)
                                    {
                                        num30 = math.max(num30, num33);
                                    }
                                }
                            }
                        }
                        m_IndustrialDemands[resourceIndex4] += Mathf.RoundToInt(100f * num30);
                    }
                }
                else // weight == 0
                {
                    m_IndustrialDemands[resourceIndex4] = Mathf.RoundToInt(100f * (num24 + num27 + num29));
                }
                m_IndustrialDemands[resourceIndex4] = math.min(100, math.max(0, m_IndustrialDemands[resourceIndex4]));
                if (flag4) // weight == 0
                {
                    m_OfficeCompanyDemand.value += Mathf.RoundToInt(m_IndustrialDemands[resourceIndex4]);
                    num18++;
                }
                else // weight > 0
                {
                    m_IndustrialCompanyDemand.value += Mathf.RoundToInt(m_IndustrialDemands[resourceIndex4]);
                    if (!isMaterial)
                    {
                        num19++;
                    }
                }
                m_IndustrialZoningDemands[resourceIndex4] = m_IndustrialDemands[resourceIndex4];
                if (!isMaterial && m_FreeProperties[resourceIndex4] == 0)
                {
                    m_IndustrialDemands[resourceIndex4] = 0;
                }
                if (m_CachedDemands[resourceIndex4] > 0)
                {
                    if (!isMaterial && m_IndustrialZoningDemands[resourceIndex4] > 0)
                    {
                        m_IndustrialBuildingDemands[resourceIndex4] = math.max(0, Mathf.CeilToInt(math.max(1f, (float)math.min(1, m_Propertyless[resourceIndex4]) + (float)m_Companies[resourceIndex4] / m_DemandParameters.m_FreeIndustrialProportion) - (float)m_FreeProperties[resourceIndex4]));
                    }
                    else if (m_IndustrialZoningDemands[resourceIndex4] > 0)
                    {
                        m_IndustrialBuildingDemands[resourceIndex4] = 1;
                    }
                    else
                    {
                        m_IndustrialBuildingDemands[resourceIndex4] = 0;
                    }
                    if (m_IndustrialBuildingDemands[resourceIndex4] > 0)
                    {
                        if (flag4) // weight == 0
                        {
                            m_OfficeBuildingDemand.value += ((m_IndustrialBuildingDemands[resourceIndex4] > 0) ? m_IndustrialZoningDemands[resourceIndex4] : 0);
                        }
                        else if (!isMaterial) // weight > 0
                        {
                            m_IndustrialBuildingDemand.value += ((m_IndustrialBuildingDemands[resourceIndex4] > 0) ? m_IndustrialZoningDemands[resourceIndex4] : 0);
                        }
                    }
                }
                if (isMaterial)
                {
                    continue;
                }
                // InfoLoom, summary
                Plugin.Log($"{iterator.resource} ({resourceIndex4}): office {flag4} bldg {m_IndustrialBuildingDemands[resourceIndex4]} zone {m_IndustrialZoningDemands[resourceIndex4]}");
                Plugin.Log($"{iterator.resource} ({resourceIndex4}): work [1] {num24} edu [2] {num27} tax [11] {num29}");
                Plugin.Log($"{iterator.resource} ({resourceIndex4}): base {value} local [4] {num21} inputs [10] {num30}");
                if (flag4) // weight == 0
                {
                    if (!flag2 || (m_IndustrialBuildingDemands[resourceIndex4] > 0 && m_IndustrialZoningDemands[resourceIndex4] > 0))
                    {
                        int num38 = ((m_IndustrialBuildingDemands[resourceIndex4] > 0) ? m_IndustrialZoningDemands[resourceIndex4] : 0);
                        int demandFactorEffect = DemandUtils.GetDemandFactorEffect(m_IndustrialDemands[resourceIndex4], num27);
                        int demandFactorEffect2 = DemandUtils.GetDemandFactorEffect(m_IndustrialDemands[resourceIndex4], num24);
                        int demandFactorEffect3 = DemandUtils.GetDemandFactorEffect(m_IndustrialDemands[resourceIndex4], num29);
                        int num39 = demandFactorEffect + demandFactorEffect2 + demandFactorEffect3;
                        m_OfficeDemandFactors[2] += demandFactorEffect; // EducatedWorkforce 
                        m_OfficeDemandFactors[1] += demandFactorEffect2; // UneducatedWorkforce 
                        m_OfficeDemandFactors[11] += demandFactorEffect3; // Taxes 
                        m_OfficeDemandFactors[13] += math.min(0, num38 - num39); // EmptyBuildings 
                    }
                    // InfoLoom - no demand, TODO COUNT
                    else
                    {
                        Plugin.Log($"No office demand for: {iterator.resource}");
                    }
                }
                else if ((!flag && !flag3) || (m_IndustrialBuildingDemands[resourceIndex4] > 0 && m_IndustrialZoningDemands[resourceIndex4] > 0)) // weight > 0
                {
                    int num40 = ((m_IndustrialBuildingDemands[resourceIndex4] > 0) ? m_IndustrialZoningDemands[resourceIndex4] : 0);
                    int demandFactorEffect4 = DemandUtils.GetDemandFactorEffect(m_IndustrialDemands[resourceIndex4], num27);
                    int demandFactorEffect5 = DemandUtils.GetDemandFactorEffect(m_IndustrialDemands[resourceIndex4], num24);
                    int demandFactorEffect6 = DemandUtils.GetDemandFactorEffect(m_IndustrialDemands[resourceIndex4], math.max(0f, value * num21));
                    int demandFactorEffect7 = DemandUtils.GetDemandFactorEffect(m_IndustrialDemands[resourceIndex4], num30);
                    int demandFactorEffect8 = DemandUtils.GetDemandFactorEffect(m_IndustrialDemands[resourceIndex4], num29);
                    int num41 = demandFactorEffect4 + demandFactorEffect5 + demandFactorEffect6 + demandFactorEffect7 + demandFactorEffect8;
                    m_DemandFactors[2] += demandFactorEffect4; // EducatedWorkforce 
                    m_DemandFactors[1] += demandFactorEffect5; // UneducatedWorkforce 
                    m_DemandFactors[4] += demandFactorEffect6; // LocalDemand 
                    m_DemandFactors[10] += demandFactorEffect7; // LocalInputs 
                    m_DemandFactors[11] += demandFactorEffect8; // Taxes 
                    m_DemandFactors[13] += math.min(0, num40 - num41); // EmptyBuildings 
                }
                // InfoLoom - no demand, TODO COUNT
                else
                {
                    Plugin.Log($"No industry demand for: {iterator.resource}");
                }
            }
            Plugin.Log($"Native  values building/company/numres: IND {m_IndustrialBuildingDemand.value}/{m_IndustrialCompanyDemand.value}/{num19} " +
                $"STO {m_StorageBuildingDemand.value}/{m_StorageCompanyDemand.value} OFF {m_OfficeBuildingDemand.value}/{m_OfficeCompanyDemand.value}/{num18} ");
            m_StorageBuildingDemand.value = Mathf.CeilToInt(math.pow(20f * (float)m_StorageBuildingDemand.value, 0.75f));
            m_IndustrialBuildingDemand.value = 2 * m_IndustrialBuildingDemand.value / num19; // Infixo: THIS IS ERROR
            m_OfficeCompanyDemand.value *= 2 * m_OfficeCompanyDemand.value / num18; // Infixo: THIS IS ERROR
            m_IndustrialBuildingDemand.value = math.clamp(m_IndustrialBuildingDemand.value, 0, 100);
            m_OfficeBuildingDemand.value = math.clamp(m_OfficeBuildingDemand.value, 0, 100);
            Plugin.Log($"Clamped values building/company/numres: IND {m_IndustrialBuildingDemand.value}/{m_IndustrialCompanyDemand.value}/{num19} " +
                $"STO {m_StorageBuildingDemand.value}/{m_StorageCompanyDemand.value} OFF {m_OfficeBuildingDemand.value}/{m_OfficeCompanyDemand.value}/{num18} ");
            // InfoLoom
        }
    }

    private struct TypeHandle
    {
        [ReadOnly]
        public EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<IndustrialProcessData> __Game_Prefabs_IndustrialProcessData_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentTypeHandle<CityServiceUpkeep> __Game_City_CityServiceUpkeep_RO_ComponentTypeHandle;

        [ReadOnly]
        public ComponentLookup<StorageLimitData> __Game_Companies_StorageLimitData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<SpawnableBuildingData> __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<BuildingData> __Game_Prefabs_BuildingData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<BuildingPropertyData> __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<IndustrialProcessData> __Game_Prefabs_IndustrialProcessData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<PrefabRef> __Game_Prefabs_PrefabRef_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<PropertyRenter> __Game_Buildings_PropertyRenter_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Attached> __Game_Objects_Attached_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<ResourceData> __Game_Prefabs_ResourceData_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<WorkplaceData> __Game_Prefabs_WorkplaceData_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<ServiceUpkeepData> __Game_Prefabs_ServiceUpkeepData_RO_BufferLookup;

        [ReadOnly]
        public BufferLookup<CityModifier> __Game_City_CityModifier_RO_BufferLookup;

        [ReadOnly]
        public ComponentLookup<ConsumptionData> __Game_Prefabs_ConsumptionData_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<InstalledUpgrade> __Game_Buildings_InstalledUpgrade_RO_BufferLookup;

        [ReadOnly]
        public ComponentLookup<Population> __Game_City_Population_RO_ComponentLookup;

        [ReadOnly]
        public ComponentLookup<Tourism> __Game_City_Tourism_RO_ComponentLookup;

        [ReadOnly]
        public BufferLookup<TradeCost> __Game_Companies_TradeCost_RO_BufferLookup;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void __AssignHandles(ref SystemState state)
        {
            __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
            __Game_Prefabs_PrefabRef_RO_ComponentTypeHandle = state.GetComponentTypeHandle<PrefabRef>(isReadOnly: true);
            __Game_Prefabs_IndustrialProcessData_RO_ComponentTypeHandle = state.GetComponentTypeHandle<IndustrialProcessData>(isReadOnly: true);
            __Game_City_CityServiceUpkeep_RO_ComponentTypeHandle = state.GetComponentTypeHandle<CityServiceUpkeep>(isReadOnly: true);
            __Game_Companies_StorageLimitData_RO_ComponentLookup = state.GetComponentLookup<StorageLimitData>(isReadOnly: true);
            __Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup = state.GetComponentLookup<SpawnableBuildingData>(isReadOnly: true);
            __Game_Prefabs_BuildingData_RO_ComponentLookup = state.GetComponentLookup<BuildingData>(isReadOnly: true);
            __Game_Prefabs_BuildingPropertyData_RO_ComponentLookup = state.GetComponentLookup<BuildingPropertyData>(isReadOnly: true);
            __Game_Prefabs_IndustrialProcessData_RO_ComponentLookup = state.GetComponentLookup<IndustrialProcessData>(isReadOnly: true);
            __Game_Prefabs_PrefabRef_RO_ComponentLookup = state.GetComponentLookup<PrefabRef>(isReadOnly: true);
            __Game_Buildings_PropertyRenter_RO_ComponentLookup = state.GetComponentLookup<PropertyRenter>(isReadOnly: true);
            __Game_Objects_Attached_RO_ComponentLookup = state.GetComponentLookup<Attached>(isReadOnly: true);
            __Game_Prefabs_ResourceData_RO_ComponentLookup = state.GetComponentLookup<ResourceData>(isReadOnly: true);
            __Game_Prefabs_WorkplaceData_RO_ComponentLookup = state.GetComponentLookup<WorkplaceData>(isReadOnly: true);
            __Game_Prefabs_ServiceUpkeepData_RO_BufferLookup = state.GetBufferLookup<ServiceUpkeepData>(isReadOnly: true);
            __Game_City_CityModifier_RO_BufferLookup = state.GetBufferLookup<CityModifier>(isReadOnly: true);
            __Game_Prefabs_ConsumptionData_RO_ComponentLookup = state.GetComponentLookup<ConsumptionData>(isReadOnly: true);
            __Game_Buildings_InstalledUpgrade_RO_BufferLookup = state.GetBufferLookup<InstalledUpgrade>(isReadOnly: true);
            __Game_City_Population_RO_ComponentLookup = state.GetComponentLookup<Population>(isReadOnly: true);
            __Game_City_Tourism_RO_ComponentLookup = state.GetComponentLookup<Tourism>(isReadOnly: true);
            __Game_Companies_TradeCost_RO_BufferLookup = state.GetBufferLookup<TradeCost>(isReadOnly: true);
        }
    }

    private const string kGroup = "cityInfo";

    private SimulationSystem m_SimulationSystem;

    private static readonly int kStorageProductionDemand = 20000;

    private static readonly int kStorageCompanyEstimateLimit = 864000;

    private ResourceSystem m_ResourceSystem;

    private CitySystem m_CitySystem;

    private ClimateSystem m_ClimateSystem;

    private TaxSystem m_TaxSystem;

    private CountEmploymentSystem m_CountEmploymentSystem;

    private CountFreeWorkplacesSystem m_CountFreeWorkplacesSystem;

    private CountCompanyDataSystem m_CountCompanyDataSystem;

    private EntityQuery m_EconomyParameterQuery;

    private EntityQuery m_DemandParameterQuery;

    private EntityQuery m_FreeIndustrialQuery;

    private EntityQuery m_StorageCompanyQuery;

    private EntityQuery m_ProcessDataQuery;

    private EntityQuery m_CityServiceQuery;

    private EntityQuery m_SpawnableQuery;

    private NativeValue<int> m_IndustrialCompanyDemand;

    private NativeValue<int> m_IndustrialBuildingDemand;

    private NativeValue<int> m_StorageCompanyDemand;

    private NativeValue<int> m_StorageBuildingDemand;

    private NativeValue<int> m_OfficeCompanyDemand;

    private NativeValue<int> m_OfficeBuildingDemand;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_CachedDemands;

    //[EnumArray(typeof(DemandFactor))]
    //[DebugWatchValue]
    private NativeArray<int> m_IndustrialDemandFactors;

    //[EnumArray(typeof(DemandFactor))]
    //[DebugWatchValue]
    private NativeArray<int> m_OfficeDemandFactors;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_ResourceDemands;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_IndustrialZoningDemands;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_IndustrialBuildingDemands;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_StorageBuildingDemands;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_StorageCompanyDemands;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_FreeProperties;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_FreeStorages;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_Storages;

    //[ResourceArray]
    //[DebugWatchValue]
    private NativeArray<int> m_StorageCapacities;

    //[DebugWatchDeps]
    //private JobHandle m_WriteDependencies;

    private JobHandle m_ReadDependencies;

    private int m_LastIndustrialCompanyDemand;

    private int m_LastIndustrialBuildingDemand;

    private int m_LastStorageCompanyDemand;

    private int m_LastStorageBuildingDemand;

    private int m_LastOfficeCompanyDemand;

    private int m_LastOfficeBuildingDemand;

    private TypeHandle __TypeHandle;

    //[DebugWatchValue(color = "#f7dc6f")]
    public int industrialCompanyDemand => m_LastIndustrialCompanyDemand;

    //[DebugWatchValue(color = "#b7950b")]
    public int industrialBuildingDemand => m_LastIndustrialBuildingDemand;

    //[DebugWatchValue(color = "#cccccc")]
    public int storageCompanyDemand => m_LastStorageCompanyDemand;

    //[DebugWatchValue(color = "#999999")]
    public int storageBuildingDemand => m_LastStorageBuildingDemand;

    //[DebugWatchValue(color = "#af7ac5")]
    public int officeCompanyDemand => m_LastOfficeCompanyDemand;

    //[DebugWatchValue(color = "#6c3483")]
    public int officeBuildingDemand => m_LastOfficeBuildingDemand;

    // InfoLoom

    private RawValueBinding m_uiResults;
    private RawValueBinding m_uiExResources;

    private NativeArray<int> m_Results;
    private NativeValue<Resource> m_ExcludedResources;

    // INDUSTRIAL & OFFICE
    // 0 - free properties, 1 - propertyless companies
    // 2 - tax rate
    // 3 & 4 - service utilization rate (available/maximum), non-leisure/leisure
    // 5 & 6 - sales efficiency (sales capacity/consumption), non-leisure/leisure // how effectively a shop is utilizing its sales capacity by comparing the actual sales to the maximum sales potential
    // 7 - employee capacity ratio // how efficiently the company is utilizing its workforce by comparing the actual number of employees to the maximum number it could employ
    // 8 & 9 - educated & uneducated workforce

    public override int GetUpdateInterval(SystemUpdatePhase phase)
    {
        return 16;
    }

    public override int GetUpdateOffset(SystemUpdatePhase phase)
    {
        return 7;
    }

    /*
    public NativeArray<int> GetConsumption(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_CachedDemands;
    }

    public NativeArray<int> GetIndustrialDemandFactors(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_IndustrialDemandFactors;
    }

    public NativeArray<int> GetOfficeDemandFactors(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_OfficeDemandFactors;
    }

    public NativeArray<int> GetResourceDemands(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_ResourceDemands;
    }

    public NativeArray<int> GetBuildingDemands(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_IndustrialBuildingDemands;
    }

    public NativeArray<int> GetStorageCompanyDemands(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_StorageCompanyDemands;
    }

    public NativeArray<int> GetStorageBuildingDemands(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_StorageBuildingDemands;
    }

    public NativeArray<int> GetIndustrialResourceZoningDemands(out JobHandle deps)
    {
        deps = m_WriteDependencies;
        return m_IndustrialZoningDemands;
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
        m_ResourceSystem = base.World.GetOrCreateSystemManaged<ResourceSystem>();
        m_CitySystem = base.World.GetOrCreateSystemManaged<CitySystem>();
        m_ClimateSystem = base.World.GetOrCreateSystemManaged<ClimateSystem>();
        m_TaxSystem = base.World.GetOrCreateSystemManaged<TaxSystem>();
        m_CountEmploymentSystem = base.World.GetOrCreateSystemManaged<CountEmploymentSystem>();
        m_CountFreeWorkplacesSystem = base.World.GetOrCreateSystemManaged<CountFreeWorkplacesSystem>();
        m_CountCompanyDataSystem = base.World.GetOrCreateSystemManaged<CountCompanyDataSystem>();
        m_EconomyParameterQuery = GetEntityQuery(ComponentType.ReadOnly<EconomyParameterData>());
        m_DemandParameterQuery = GetEntityQuery(ComponentType.ReadOnly<DemandParameterData>());
        m_FreeIndustrialQuery = GetEntityQuery(ComponentType.ReadOnly<IndustrialProperty>(), ComponentType.ReadOnly<PropertyOnMarket>(), ComponentType.ReadOnly<PrefabRef>(), ComponentType.Exclude<Abandoned>(), ComponentType.Exclude<Destroyed>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Temp>(), ComponentType.Exclude<Condemned>());
        m_StorageCompanyQuery = GetEntityQuery(ComponentType.ReadOnly<PrefabRef>(), ComponentType.ReadOnly<Game.Companies.StorageCompany>(), ComponentType.Exclude<Game.Objects.OutsideConnection>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Temp>());
        m_ProcessDataQuery = GetEntityQuery(ComponentType.ReadOnly<IndustrialProcessData>(), ComponentType.Exclude<ServiceCompanyData>());
        m_CityServiceQuery = GetEntityQuery(ComponentType.ReadOnly<CityServiceUpkeep>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Temp>());
        m_SpawnableQuery = GetEntityQuery(ComponentType.ReadOnly<BuildingCondition>(), ComponentType.Exclude<Deleted>(), ComponentType.Exclude<Abandoned>(), ComponentType.Exclude<Destroyed>(), ComponentType.Exclude<Temp>());
        m_IndustrialCompanyDemand = new NativeValue<int>(Allocator.Persistent);
        m_IndustrialBuildingDemand = new NativeValue<int>(Allocator.Persistent);
        m_StorageCompanyDemand = new NativeValue<int>(Allocator.Persistent);
        m_StorageBuildingDemand = new NativeValue<int>(Allocator.Persistent);
        m_OfficeCompanyDemand = new NativeValue<int>(Allocator.Persistent);
        m_OfficeBuildingDemand = new NativeValue<int>(Allocator.Persistent);
        m_IndustrialDemandFactors = new NativeArray<int>(18, Allocator.Persistent);
        m_OfficeDemandFactors = new NativeArray<int>(18, Allocator.Persistent);
        int resourceCount = EconomyUtils.ResourceCount;
        m_ResourceDemands = new NativeArray<int>(resourceCount, Allocator.Persistent);
        m_IndustrialZoningDemands = new NativeArray<int>(resourceCount, Allocator.Persistent);
        m_IndustrialBuildingDemands = new NativeArray<int>(resourceCount, Allocator.Persistent);
        m_CachedDemands = new NativeArray<int>(resourceCount, Allocator.Persistent);
        m_StorageBuildingDemands = new NativeArray<int>(resourceCount, Allocator.Persistent);
        m_StorageCompanyDemands = new NativeArray<int>(resourceCount, Allocator.Persistent);
        m_FreeProperties = new NativeArray<int>(resourceCount, Allocator.Persistent);
        m_FreeStorages = new NativeArray<int>(resourceCount, Allocator.Persistent);
        m_Storages = new NativeArray<int>(resourceCount, Allocator.Persistent);
        m_StorageCapacities = new NativeArray<int>(resourceCount, Allocator.Persistent);
        RequireForUpdate(m_EconomyParameterQuery);
        RequireForUpdate(m_DemandParameterQuery);
        RequireForUpdate(m_ProcessDataQuery);

        // InfoLoom
        SetDefaults(); // there is no serialization, so init just for safety
        m_Results = new NativeArray<int>(10, Allocator.Persistent);
        m_ExcludedResources = new NativeValue<Resource>(Allocator.Persistent);

        AddBinding(m_uiResults = new RawValueBinding(kGroup, "ilIndustrial", delegate (IJsonWriter binder)
        {
            binder.ArrayBegin(m_Results.Length);
            for (int i = 0; i < m_Results.Length; i++)
                binder.Write(m_Results[i]);
            binder.ArrayEnd();
        }));

        AddBinding(m_uiExResources = new RawValueBinding(kGroup, "ilIndustrialExRes", delegate (IJsonWriter binder)
        {
            List<string> resList = new List<string>();
            for (int i = 0; i < Game.Economy.EconomyUtils.ResourceCount; i++)
                if ((m_ExcludedResources.value & Game.Economy.EconomyUtils.GetResource(i)) != Resource.NoResource)
                    resList.Add(Game.Economy.EconomyUtils.GetName(Game.Economy.EconomyUtils.GetResource(i)));
            binder.ArrayBegin(resList.Count);
            foreach (string res in resList)
                binder.Write(res);
            binder.ArrayEnd();
        }));

        Plugin.Log("IndustrialDemandUISystem created.");
    }

    [Preserve]
    protected override void OnDestroy()
    {
        m_IndustrialCompanyDemand.Dispose();
        m_IndustrialBuildingDemand.Dispose();
        m_StorageCompanyDemand.Dispose();
        m_StorageBuildingDemand.Dispose();
        m_OfficeCompanyDemand.Dispose();
        m_OfficeBuildingDemand.Dispose();
        m_IndustrialDemandFactors.Dispose();
        m_OfficeDemandFactors.Dispose();
        m_ResourceDemands.Dispose();
        m_IndustrialZoningDemands.Dispose();
        m_IndustrialBuildingDemands.Dispose();
        m_StorageBuildingDemands.Dispose();
        m_StorageCompanyDemands.Dispose();
        m_CachedDemands.Dispose();
        m_FreeProperties.Dispose();
        m_Storages.Dispose();
        m_FreeStorages.Dispose();
        m_StorageCapacities.Dispose();
        base.OnDestroy();
    }

    public void SetDefaults() //Context context)
    {
        m_IndustrialCompanyDemand.value = 0;
        m_IndustrialBuildingDemand.value = 0;
        m_StorageCompanyDemand.value = 0;
        m_StorageBuildingDemand.value = 0;
        m_OfficeCompanyDemand.value = 0;
        m_OfficeBuildingDemand.value = 0;
        m_IndustrialDemandFactors.Fill(0);
        m_OfficeDemandFactors.Fill(0);
        m_ResourceDemands.Fill(0);
        m_IndustrialZoningDemands.Fill(0);
        m_IndustrialBuildingDemands.Fill(0);
        m_StorageBuildingDemands.Fill(0);
        m_StorageCompanyDemands.Fill(0);
        m_FreeProperties.Fill(0);
        m_Storages.Fill(0);
        m_FreeStorages.Fill(0);
        m_LastIndustrialCompanyDemand = 0;
        m_LastIndustrialBuildingDemand = 0;
        m_LastStorageCompanyDemand = 0;
        m_LastStorageBuildingDemand = 0;
        m_LastOfficeCompanyDemand = 0;
        m_LastOfficeBuildingDemand = 0;
    }

    /* not used
    public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
    {
        writer.Write(m_IndustrialCompanyDemand.value);
        writer.Write(m_IndustrialBuildingDemand.value);
        writer.Write(m_StorageCompanyDemand.value);
        writer.Write(m_StorageBuildingDemand.value);
        writer.Write(m_OfficeCompanyDemand.value);
        writer.Write(m_OfficeBuildingDemand.value);
        writer.Write(m_IndustrialDemandFactors.Length);
        writer.Write(m_IndustrialDemandFactors);
        writer.Write(m_OfficeDemandFactors);
        writer.Write(m_ResourceDemands);
        writer.Write(m_IndustrialZoningDemands);
        writer.Write(m_IndustrialBuildingDemands);
        writer.Write(m_StorageBuildingDemands);
        writer.Write(m_StorageCompanyDemands);
        writer.Write(m_FreeProperties);
        writer.Write(m_Storages);
        writer.Write(m_FreeStorages);
        writer.Write(m_LastIndustrialCompanyDemand);
        writer.Write(m_LastIndustrialBuildingDemand);
        writer.Write(m_LastStorageCompanyDemand);
        writer.Write(m_LastStorageBuildingDemand);
        writer.Write(m_LastOfficeCompanyDemand);
        writer.Write(m_LastOfficeBuildingDemand);
    }

    public void Deserialize<TReader>(TReader reader) where TReader : IReader
    {
        reader.Read(out int value);
        m_IndustrialCompanyDemand.value = value;
        reader.Read(out int value2);
        m_IndustrialBuildingDemand.value = value2;
        reader.Read(out int value3);
        m_StorageCompanyDemand.value = value3;
        reader.Read(out int value4);
        m_StorageBuildingDemand.value = value4;
        reader.Read(out int value5);
        m_OfficeCompanyDemand.value = value5;
        reader.Read(out int value6);
        m_OfficeBuildingDemand.value = value6;
        if (reader.context.version < Version.demandFactorCountSerialization)
        {
            NativeArray<int> nativeArray = new NativeArray<int>(13, Allocator.Temp);
            reader.Read(nativeArray);
            CollectionUtils.CopySafe(nativeArray, m_IndustrialDemandFactors);
            reader.Read(nativeArray);
            CollectionUtils.CopySafe(nativeArray, m_OfficeDemandFactors);
            nativeArray.Dispose();
        }
        else
        {
            reader.Read(out int value7);
            if (value7 == m_IndustrialDemandFactors.Length)
            {
                reader.Read(m_IndustrialDemandFactors);
                reader.Read(m_OfficeDemandFactors);
            }
            else
            {
                NativeArray<int> nativeArray2 = new NativeArray<int>(value7, Allocator.Temp);
                reader.Read(nativeArray2);
                CollectionUtils.CopySafe(nativeArray2, m_IndustrialDemandFactors);
                reader.Read(nativeArray2);
                CollectionUtils.CopySafe(nativeArray2, m_OfficeDemandFactors);
                nativeArray2.Dispose();
            }
        }
        reader.Read(m_ResourceDemands);
        reader.Read(m_IndustrialZoningDemands);
        reader.Read(m_IndustrialBuildingDemands);
        reader.Read(m_StorageBuildingDemands);
        reader.Read(m_StorageCompanyDemands);
        if (reader.context.version <= Version.companyDemandOptimization)
        {
            NativeArray<int> value8 = new NativeArray<int>(EconomyUtils.ResourceCount, Allocator.Temp);
            reader.Read(value8);
            reader.Read(value8);
            if (reader.context.version <= Version.demandFactorCountSerialization)
            {
                reader.Read(value8);
                reader.Read(value8);
            }
            reader.Read(value8);
            reader.Read(value8);
        }
        reader.Read(m_FreeProperties);
        reader.Read(m_Storages);
        reader.Read(m_FreeStorages);
        reader.Read(out m_LastIndustrialCompanyDemand);
        reader.Read(out m_LastIndustrialBuildingDemand);
        reader.Read(out m_LastStorageCompanyDemand);
        reader.Read(out m_LastStorageBuildingDemand);
        reader.Read(out m_LastOfficeCompanyDemand);
        reader.Read(out m_LastOfficeBuildingDemand);
    }
    */

    [Preserve]
    protected override void OnUpdate()
    {
        if (m_SimulationSystem.frameIndex % 128 != 66)
            return;
        //Plugin.Log($"OnUpdate: {m_SimulationSystem.frameIndex}");
        base.OnUpdate();
        ResetResults();

        if (!m_DemandParameterQuery.IsEmptyIgnoreFilter && !m_EconomyParameterQuery.IsEmptyIgnoreFilter)
        {
            m_LastIndustrialCompanyDemand = m_IndustrialCompanyDemand.value;
            m_LastIndustrialBuildingDemand = m_IndustrialBuildingDemand.value;
            m_LastStorageCompanyDemand = m_StorageCompanyDemand.value;
            m_LastStorageBuildingDemand = m_StorageBuildingDemand.value;
            m_LastOfficeCompanyDemand = m_OfficeCompanyDemand.value;
            m_LastOfficeBuildingDemand = m_OfficeBuildingDemand.value;
            JobHandle deps;
            CountCompanyDataSystem.IndustrialCompanyDatas industrialCompanyDatas = m_CountCompanyDataSystem.GetIndustrialCompanyDatas(out deps);
            __TypeHandle.__Game_Companies_TradeCost_RO_BufferLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_City_Tourism_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_City_Population_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Buildings_InstalledUpgrade_RO_BufferLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_City_CityModifier_RO_BufferLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_ServiceUpkeepData_RO_BufferLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_WorkplaceData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_ResourceData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Objects_Attached_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_IndustrialProcessData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Companies_StorageLimitData_RO_ComponentLookup.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_City_CityServiceUpkeep_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_IndustrialProcessData_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle.Update(ref base.CheckedStateRef);
            __TypeHandle.__Unity_Entities_Entity_TypeHandle.Update(ref base.CheckedStateRef);
            UpdateIndustrialDemandJob updateIndustrialDemandJob = default(UpdateIndustrialDemandJob);
            updateIndustrialDemandJob.m_FreePropertyChunks = m_FreeIndustrialQuery.ToArchetypeChunkListAsync(base.World.UpdateAllocator.ToAllocator, out var outJobHandle);
            updateIndustrialDemandJob.m_StorageCompanyChunks = m_StorageCompanyQuery.ToArchetypeChunkListAsync(base.World.UpdateAllocator.ToAllocator, out var outJobHandle2);
            updateIndustrialDemandJob.m_IndustrialProcessDataChunks = m_ProcessDataQuery.ToArchetypeChunkListAsync(base.World.UpdateAllocator.ToAllocator, out var outJobHandle3);
            updateIndustrialDemandJob.m_CityServiceChunks = m_CityServiceQuery.ToArchetypeChunkListAsync(base.World.UpdateAllocator.ToAllocator, out var outJobHandle4);
            updateIndustrialDemandJob.m_SpawnableChunks = m_SpawnableQuery.ToArchetypeChunkListAsync(base.World.UpdateAllocator.ToAllocator, out var outJobHandle5);
            updateIndustrialDemandJob.m_EntityType = __TypeHandle.__Unity_Entities_Entity_TypeHandle;
            updateIndustrialDemandJob.m_PrefabType = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentTypeHandle;
            updateIndustrialDemandJob.m_ProcessType = __TypeHandle.__Game_Prefabs_IndustrialProcessData_RO_ComponentTypeHandle;
            updateIndustrialDemandJob.m_ServiceUpkeepType = __TypeHandle.__Game_City_CityServiceUpkeep_RO_ComponentTypeHandle;
            updateIndustrialDemandJob.m_StorageLimitDatas = __TypeHandle.__Game_Companies_StorageLimitData_RO_ComponentLookup;
            updateIndustrialDemandJob.m_SpawnableBuildingDatas = __TypeHandle.__Game_Prefabs_SpawnableBuildingData_RO_ComponentLookup;
            updateIndustrialDemandJob.m_BuildingDatas = __TypeHandle.__Game_Prefabs_BuildingData_RO_ComponentLookup;
            updateIndustrialDemandJob.m_BuildingPropertyDatas = __TypeHandle.__Game_Prefabs_BuildingPropertyData_RO_ComponentLookup;
            updateIndustrialDemandJob.m_IndustrialProcessDatas = __TypeHandle.__Game_Prefabs_IndustrialProcessData_RO_ComponentLookup;
            updateIndustrialDemandJob.m_Prefabs = __TypeHandle.__Game_Prefabs_PrefabRef_RO_ComponentLookup;
            updateIndustrialDemandJob.m_PropertyRenters = __TypeHandle.__Game_Buildings_PropertyRenter_RO_ComponentLookup;
            updateIndustrialDemandJob.m_Attached = __TypeHandle.__Game_Objects_Attached_RO_ComponentLookup;
            updateIndustrialDemandJob.m_ResourceDatas = __TypeHandle.__Game_Prefabs_ResourceData_RO_ComponentLookup;
            updateIndustrialDemandJob.m_WorkplaceDatas = __TypeHandle.__Game_Prefabs_WorkplaceData_RO_ComponentLookup;
            updateIndustrialDemandJob.m_ServiceUpkeeps = __TypeHandle.__Game_Prefabs_ServiceUpkeepData_RO_BufferLookup;
            updateIndustrialDemandJob.m_CityModifiers = __TypeHandle.__Game_City_CityModifier_RO_BufferLookup;
            updateIndustrialDemandJob.m_ConsumptionDatas = __TypeHandle.__Game_Prefabs_ConsumptionData_RO_ComponentLookup;
            updateIndustrialDemandJob.m_InstalledUpgrades = __TypeHandle.__Game_Buildings_InstalledUpgrade_RO_BufferLookup;
            updateIndustrialDemandJob.m_Upkeeps = __TypeHandle.__Game_Prefabs_ServiceUpkeepData_RO_BufferLookup;
            updateIndustrialDemandJob.m_Populations = __TypeHandle.__Game_City_Population_RO_ComponentLookup;
            updateIndustrialDemandJob.m_Tourisms = __TypeHandle.__Game_City_Tourism_RO_ComponentLookup;
            updateIndustrialDemandJob.m_TradeCosts = __TypeHandle.__Game_Companies_TradeCost_RO_BufferLookup;
            updateIndustrialDemandJob.m_DemandParameters = m_DemandParameterQuery.GetSingleton<DemandParameterData>();
            updateIndustrialDemandJob.m_EconomyParameters = m_EconomyParameterQuery.GetSingleton<EconomyParameterData>();
            updateIndustrialDemandJob.m_ResourcePrefabs = m_ResourceSystem.GetPrefabs();
            updateIndustrialDemandJob.m_EmployableByEducation = m_CountEmploymentSystem.GetEmployableByEducation(out var deps2);
            updateIndustrialDemandJob.m_TaxRates = m_TaxSystem.GetTaxRates();
            updateIndustrialDemandJob.m_FreeWorkplaces = m_CountFreeWorkplacesSystem.GetFreeWorkplaces(out var deps3);
            updateIndustrialDemandJob.m_City = m_CitySystem.City;
            updateIndustrialDemandJob.m_HeatingNeed = BuildingUpkeepSystem.GetHeatingMultiplier(m_ClimateSystem.temperature);
            updateIndustrialDemandJob.m_BaseConsumptionSum = m_ResourceSystem.BaseConsumptionSum;
            updateIndustrialDemandJob.m_IndustrialCompanyDemand = m_IndustrialCompanyDemand;
            updateIndustrialDemandJob.m_IndustrialBuildingDemand = m_IndustrialBuildingDemand;
            updateIndustrialDemandJob.m_StorageCompanyDemand = m_StorageCompanyDemand;
            updateIndustrialDemandJob.m_StorageBuildingDemand = m_StorageBuildingDemand;
            updateIndustrialDemandJob.m_OfficeCompanyDemand = m_OfficeCompanyDemand;
            updateIndustrialDemandJob.m_OfficeBuildingDemand = m_OfficeBuildingDemand;
            updateIndustrialDemandJob.m_IndustrialBuildingDemands = m_IndustrialBuildingDemands;
            updateIndustrialDemandJob.m_IndustrialDemands = m_ResourceDemands;
            updateIndustrialDemandJob.m_IndustrialZoningDemands = m_IndustrialZoningDemands;
            updateIndustrialDemandJob.m_StorageBuildingDemands = m_StorageBuildingDemands;
            updateIndustrialDemandJob.m_StorageCompanyDemands = m_StorageCompanyDemands;
            updateIndustrialDemandJob.m_Companies = industrialCompanyDatas.m_ProductionCompanies;
            updateIndustrialDemandJob.m_Propertyless = industrialCompanyDatas.m_ProductionPropertyless;
            updateIndustrialDemandJob.m_Demands = industrialCompanyDatas.m_Demand;
            updateIndustrialDemandJob.m_FreeProperties = m_FreeProperties;
            updateIndustrialDemandJob.m_Productions = industrialCompanyDatas.m_Production;
            updateIndustrialDemandJob.m_TotalCurrentWorkers = industrialCompanyDatas.m_CurrentProductionWorkers;
            updateIndustrialDemandJob.m_TotalMaxWorkers = industrialCompanyDatas.m_MaxProductionWorkers;
            updateIndustrialDemandJob.m_Storages = m_Storages;
            updateIndustrialDemandJob.m_FreeStorages = m_FreeStorages;
            updateIndustrialDemandJob.m_StorageCapacities = m_StorageCapacities;
            updateIndustrialDemandJob.m_DemandFactors = m_IndustrialDemandFactors;
            updateIndustrialDemandJob.m_OfficeDemandFactors = m_OfficeDemandFactors;
            updateIndustrialDemandJob.m_CachedDemands = m_CachedDemands;
            updateIndustrialDemandJob.m_Results = m_Results;
            updateIndustrialDemandJob.m_ExcludedResources = m_ExcludedResources;
            UpdateIndustrialDemandJob jobData = updateIndustrialDemandJob;
            base.Dependency = IJobExtensions.Schedule(jobData, JobUtils.CombineDependencies(base.Dependency, m_ReadDependencies, outJobHandle, deps, outJobHandle2, outJobHandle3, outJobHandle4, outJobHandle5, deps2, deps3));
            // since this is a copy of an actual simulation system but for UI purposes, then noone will read from us or wait for us
            base.Dependency.Complete();
            //m_WriteDependencies = base.Dependency;
            //m_CountCompanyDataSystem.AddReader(base.Dependency);
            //m_ResourceSystem.AddPrefabsReader(base.Dependency);
            //m_CountEmploymentSystem.AddReader(base.Dependency);
            //m_CountFreeWorkplacesSystem.AddReader(base.Dependency);
            //m_TaxSystem.AddReader(base.Dependency);

            // Update UI
            m_uiResults.Update();
            m_uiExResources.Update();
        }
    }

    private void ResetResults()
    {
        m_ExcludedResources.value = Resource.NoResource;
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
    public IndustrialDemandUISystem()
    {
    }
}
