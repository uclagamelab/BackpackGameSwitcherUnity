using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Enumz
{
    static Dictionary<System.Type, object> _values = new Dictionary<System.Type, object>();

    static Dictionary<System.Type, Dictionary<int, string>> _convenienceInfo = new Dictionary<System.Type, Dictionary<int, string>>();


    public static string NameFromIntValue(int value, System.Type ty)
    {
        Dictionary<int, string> ret = null;
        if (!_convenienceInfo.TryGetValue(ty, out ret) || ret == null || true)
        {
            var raw = System.Enum.GetValues(ty);
            ret = new(raw.Length);

            int i = 0;
            foreach (var e in raw)
            {

                ret[System.Convert.ToInt32(e)] = e.ToString();
                i++;
            }
            _convenienceInfo[ty] = ret;
        }

        return ret?.GetOrDefault(value);
    }

    public static int[] AllValues<T>()
    {
        var ty = typeof(T);
        return AllValues(ty);
    }
    public static int[] AllValues(string typeName)
    {
        return AllValues(System.Type.GetType(typeName));
    }

    public static int[] AllValues(Type type)
    {   
        object ret = null;
        if (!_values.TryGetValue(type, out ret) || ret == null)
        {
            var raw = System.Enum.GetValues(type);
            ret = new int[raw.Length];
            int i = 0;
            foreach (var e in raw)
            {
                (ret as int[])[i] = Convert.ToInt32(e);
                i++;
            }
            _values[type] = ret;
        }
        return ret as int[];
    }
}
