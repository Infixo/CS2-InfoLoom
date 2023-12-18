using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace InfoLoom;

internal static class Utils
{
    public static void LogChunk(in ArchetypeChunk chunk)
    {
        var componentTypes = chunk.Archetype.GetComponentTypes();
        Plugin.Log($"chunk: {chunk.Count}, {string.Join(", ", componentTypes.Select(ct => ct.GetType().GetTypeInfo().FullName))}");
    }

    public static string[] ListEntityComponents(EntityManager manager, Entity entity)
    {
        var componentTypes = new List<ComponentType>();

        if (!manager.Exists(entity))
            throw new ArgumentException("Entity does not exist.");

        NativeArray<ComponentType> NativeArray = manager.GetComponentTypes(entity, Allocator.Temp);
        string[] ToReturn = NativeArray.Select(T => T.GetManagedType().Name).ToArray();
        NativeArray.Dispose();
        return ToReturn;
    }

    public static void InspectComponentsInChunk(EntityManager manager, in ArchetypeChunk chunk, string name)
    {
        EntityTypeHandle m_EntityHandle;
        NativeArray<Entity> entities = chunk.GetNativeArray(m_EntityHandle);
        if (entities.Length > 0)
        {
            Entity firstEntity = entities[0];
            string[] components = ListEntityComponents(manager, firstEntity);
            Plugin.Log($"{name} {entities.Length}: {string.Join(" ", components)}");
        }
    }

    public static void InspectComponentsInQuery(EntityManager manager, EntityQuery query, string name)
    {
        Dictionary<string, int> CompDict = new Dictionary<string, int>();
        NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < entities.Length; i++)
        {
            Entity entity = entities[i];
            string[] comps = ListEntityComponents(manager, entity);
            foreach (string comp in comps)
            {
                if (CompDict.ContainsKey(comp)) CompDict[comp]++;
                else CompDict.Add(comp, 1);
            }
        }
        entities.Dispose();
        // show the dictionary
        Plugin.Log($"===== {name} =====");
        foreach (var pair in CompDict)
        {
            Plugin.Log($"{pair.Key} {pair.Value}");
        }
    }

    public static void InspectComponentsInChunks(EntityManager manager, EntityQuery query, string name)
    {
        Plugin.Log($"===== Components in chunks of query {name} =====");
        NativeArray<ArchetypeChunk> chunks = query.ToArchetypeChunkArray(Allocator.Temp);
        for (int i = 0; i < chunks.Length; i++)
        {
            InspectComponentsInChunk(manager, chunks[i], name);
        }
        chunks.Dispose();
    }

}
