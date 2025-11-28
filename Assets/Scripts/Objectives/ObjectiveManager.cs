using System;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectiveType { Primary, Secondary }

[Serializable]
public class Objective
{
    public string id;
    public ObjectiveType type = ObjectiveType.Primary;
    public string description;
    public bool completed;
}

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager I { get; private set; }
    public List<Objective> objectives = new();
    public bool entryReached = false;
    public bool exitEnabled = false;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Mark(string id, bool state = true)
    {
        var o = objectives.Find(x => x.id == id);
        if (o != null) o.completed = state;
        // You can fire UI updates here
    }

    public bool AllPrimariesComplete()
    {
        foreach (var o in objectives)
            if (o.type == ObjectiveType.Primary && !o.completed) return false;
        return true;
    }
}
