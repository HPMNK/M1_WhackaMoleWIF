using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonDontDestroyGenerated<T> : MonoBehaviour where T : MonoBehaviour
{

    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name);
                _instance = go.AddComponent<T>();
                (_instance as SingletonDontDestroyGenerated<T>)?.OnCreate();

                DontDestroyOnLoad(go);

            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }

    }

  
    public virtual void OnCreate()
    {

    }



}
