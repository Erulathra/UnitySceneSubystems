using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SceneSubsystem : MonoBehaviour
{
	public abstract void Initialize();
	public virtual void PreAwake() { }
	public virtual void Destroy() { }
}

public class SceneSubsystemManager : MonoBehaviour
{
	private static SceneSubsystemManager instance;
	
	private readonly Dictionary<Type, int> subsystemsFindDict = new();
	private readonly List<SceneSubsystem> subsystems = new ();
	

    // ReSharper disable Unity.PerformanceAnalysis
	public static T GetSubsystem<T>() where T : SceneSubsystem
	{
		if (instance.subsystemsFindDict.ContainsKey(typeof(T)))
		{
			int index = instance.subsystemsFindDict[typeof(T)];
			return (T)instance.subsystems[index];
		}

        Debug.LogError($"<b>SceneSubsystems:</b> Subsystem {typeof(T)} is not registered");
        Debug.Break();
		
		return null;
	}

	
	public static void Initialize(GameObject gameObject, Action<SceneSubsystemManager> initializationOrderHandler)
	{
		if (instance)
			return;

		instance = gameObject.AddComponent<SceneSubsystemManager>();
		
		initializationOrderHandler.Invoke(instance);
		instance.InitializeSubsystems();
		instance.PreAwakeSubsystems();
	}

	private void InitializeSubsystems()
	{
		foreach (var subsystem in subsystems)
		{
			subsystem.Initialize();
		}
	}
	
	private void PreAwakeSubsystems()
	{
		foreach (var subsystem in subsystems)
		{
			subsystem.PreAwake();
		}
	}

	private void OnDestroy()
	{
		for (int subsystemID = subsystems.Count - 1; subsystemID >= 0; subsystemID--)
		{
			subsystems[subsystemID].Destroy();
		}
	}
	
	public void FindOrAddSubsystem<T>() where T : SceneSubsystem
	{
		T subsystem = FindObjectOfType<T>();
		if (subsystem == null)
		{
			subsystem = gameObject.AddComponent<T>();
		}

		if (subsystem == null)
		{
			Debug.LogError($"<b>SceneSubsystems:</b> Subsystem {typeof(T)} creation failed");
			return;
		}

		subsystems.Add(subsystem);
		subsystemsFindDict.Add(typeof(T), subsystems.Count - 1);
	}
}