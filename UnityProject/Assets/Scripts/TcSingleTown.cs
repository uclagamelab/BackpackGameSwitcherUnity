/*

Class for accessing singletons by type, using a one-time gameobject.find (will give up if not found the first time)

Could be extended to instantiate things via a resources path as well.

 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SingletownStrategy { FindExisting, CreateGameObjectAndAddComponentIfNonExisting, InstantiateFromResourcesPrefab };
class SingleTown<T> where T : Component
{
    string _resourcesPrefabPath = null;

    SingletownStrategy _strategy = SingletownStrategy.FindExisting;

    public SingleTown(SingletownStrategy strategy = SingletownStrategy.FindExisting)
    {
        _strategy = strategy;
    }

    public SingleTown(string resourcesPrefabPath)
    {
        _strategy = SingletownStrategy.InstantiateFromResourcesPrefab;
        _resourcesPrefabPath = resourcesPrefabPath;
    }

    bool _haveAttemptedStrategy = false;
    T _instance;
    public T instance
    {
        get
        {
            if (!_haveAttemptedStrategy && _instance == null)
            {
                _haveAttemptedStrategy = true;
                //--- FIND EXISTING ---------------------------------------------------------------------------
                if (_strategy == SingletownStrategy.FindExisting || _strategy == SingletownStrategy.CreateGameObjectAndAddComponentIfNonExisting)
                {
                    _instance = GameObject.FindObjectOfType<T>();
                    if (_instance != null)
                    {
                        //Allow the to look again, if it goes null
                        _haveAttemptedStrategy = false;
                    }
                }
                //--- CREATE GAME OBJECT AND ADD SCRIPT -------------------------------------------------------
                if (_instance == null && _strategy == SingletownStrategy.CreateGameObjectAndAddComponentIfNonExisting) 
                {
                    //Don't instantiate if not in play mode
                    if (Application.isPlaying)
                    {
                        _instance = new GameObject("SINGLE_TOWN_" + typeof(T).ToString().ToUpper(), typeof(T)).GetComponent<T>();
                    }
                }
                //--- CREATE FROM RESOURCES PREFAB ---------------------------------------------------
                else if (_strategy == SingletownStrategy.InstantiateFromResourcesPrefab)
                {
                    if (Application.isPlaying)
                    {
                        _instance = GameObject.Instantiate(Resources.Load<GameObject>(_resourcesPrefabPath)).GetComponent<T>();
                        _instance.name = "SINGLE_TOWN_" + typeof(T).ToString().ToUpper();
                    }
                }
                
            }
            return _instance;
        }
        set
        {
            _instance = value;
            if (_instance != null)
            {
                _haveAttemptedStrategy = true;
            }
        }
    }
}
