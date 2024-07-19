using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonGenerated<T> : MonoBehaviour where T : MonoBehaviour
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

            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }

    }
}
