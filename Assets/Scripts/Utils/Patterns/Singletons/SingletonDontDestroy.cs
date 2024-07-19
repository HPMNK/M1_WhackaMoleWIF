using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonDontDestroy<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            (Instance as SingletonDontDestroy<T>)?.OnCreate();

            DontDestroyOnLoad(Instance.gameObject);
        }
    }

/// <summary>
/// Use OnCreate instead of Awake (called at the same time)
/// </summary>
    public virtual void OnCreate()
    {

    }

}
