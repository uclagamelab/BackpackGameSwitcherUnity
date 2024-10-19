/*
An assortment of random useful functions/classes/stuff

Also includes some Debug developer settings

TODO convert some of these to extension methods!

*/
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

//TODO try and put camera stuff in here

public static class Alextensions
{

    public static V GetOrCreate<K, V>(this Dictionary<K, V> dict, K key, System.Func<V> create = null) where V : class, new()
    {
        if (key == null) //null keys are not allowed
        {
            return null;
        }

        V ret = dict.GetValueOrDefault(key);
        if (ret == null)
        {
            ret = create != null ? create.Invoke() : new V();
            dict[key] = ret;
        }

        return ret;
    }

    public static int CountNullRobust<T>(this IList<T> l)
    {
        return l == null ? 0 : l.Count;
    }

    public static bool indexInRange<T>(this IList<T> l, int foundIdx)
    {
        return foundIdx >= 0 && foundIdx < l.Count;
    }

    public static T GetOrDefault<T>(this IList<T> l, int index, T outOfRangeValue = default)
    {
        if (!l.indexInRange(index))
        {
            return outOfRangeValue;
        }
        return l[index];
    }

    public static V GetOrDefault<K, V>(this Dictionary<K, V> dict, K key, V optionalDefault = default(V))
    {
        if (typeof(K).IsClass && key == null) //null keys are not allowed
        {
            return optionalDefault;
        }

        return !dict.ContainsKey(key) ? optionalDefault : dict[key];
    }



    public delegate void TFunction(float t);

    public static void varyWithT(this MonoBehaviour thiss, TFunction tfunc, float dur)
    {
        thiss.StartCoroutine(genericT(tfunc, dur));
    }

    public static IEnumerator genericT(TFunction tfunc, float dur)
    {
        float timer = 0;

        while (timer < dur)
        {
            float t = Mathf.Clamp01(timer / dur);
            tfunc(t);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        //force call with 1
        tfunc(1);
    }

    public delegate void NoArgNoRetFunction();

    public static Color withAlpha(this Color thiss, float a)
    {
         Color ret = thiss;
        ret.a = a;
        return ret;
    }

    public static T chooseRandom<T>(this List<T> thiss)
    {
        if (thiss.Count == 0)
        {
            return default(T);
        }
        else
        {
            int idx = Random.Range(0,thiss.Count);
            return thiss[idx];
        }
    }



    public static Vector3 withX(this Vector3 thiss, float x)
    {
        Vector3 ret = thiss;
        ret.x = x;
        return ret;   
    }
    public static Vector3 withY(this Vector3 thiss, float y)
    {
        Vector3 ret = thiss;
        ret.y = y;
        return ret;
    }

    public static Vector3 withZ(this Vector3 thiss, float z)
    {
        Vector3 ret = thiss;
        ret.z = z;
        return ret;
    }

    public static float ezPerlin(this MonoBehaviour thiss, float t, int seedo)
    {
        int seed = Mathf.Abs(seedo) + 1;
        int sign1 = (seed % 1) == 0 ? 1 : -1;
        int sign2 = (seed % 4) < 3 ? 1 : -1;

        float t1 = t * sign1;
        float t2 = t * sign2;

        //turn into a decimal <= .05
        float coeff1 = 1.0f + Mathf.Pow((1.0f * seed) / Mathf.NextPowerOfTwo(seed), 2);
        float coeff2 = 1 + (Mathf.Log(seed + 10) % 1);


        float offset = (1.0f / (seed + 1));
        return Mathf.PerlinNoise(t1 * coeff1 + offset, t2 * coeff2 - offset);
    }

    public static void delayedFunction(this MonoBehaviour thiss, NoArgNoRetFunction func, float delay)
    {
        thiss.StartCoroutine(delayedFunctionRoutine(func, delay));
    } 

    static IEnumerator delayedFunctionRoutine (NoArgNoRetFunction func, float delay)
    {
        yield return new WaitForSeconds(delay);
        func();
    }

	//public static string toSti(this ResourceType grade)
	public static void SetLayerRecursively (this GameObject obj, int newLayer)
	{
		if (null == obj)
		{
			return;
		}
			
		obj.layer = newLayer;
			
		foreach (Transform child in obj.transform)
		{
			if (null == child)
			{
				continue;
			}
			SetLayerRecursively (child.gameObject, newLayer);
		}
	}


   

}



public class AlexUtil : MonoBehaviour
{	
	public static float ConvertRange (float originalMin, float originalMax, float targetMin, float targetMax, float number)
	{
		float ret = (number - originalMin) / (originalMax - originalMin) * (targetMax - targetMin) + targetMin;

		ret = Mathf.Clamp (ret, targetMin, targetMax);
		return ret;
	}

    public static void DrawCenteredText(Vector2 offset, string text, int fontSize, Color fontColor, string font)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = fontColor;
        style.fontSize = fontSize;
        style.font = (Font)Resources.Load("Fonts/" + font);
        Vector2 size = style.CalcSize(new GUIContent(text));
        Vector2 position = new Vector2(Screen.width, Screen.height) / 2 - size / 2;
        position += offset;
        GUI.Label(new Rect(position.x, position.y, size.x, size.y), text, style);
    }

    public static void DrawText(Vector2 position, string text, int fontSize, Color fontColor, string font)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = fontColor;
        style.fontSize = fontSize;
        style.font = (Font)Resources.Load("Fonts/" + font);
        Vector2 size = style.CalcSize(new GUIContent(text));
        GUI.Label(new Rect(position.x, position.y, size.x, size.y), text, style);
    }


	                              

	public class StoredTransform
	{
		public Vector3 position;
		public Quaternion rotation;


        public Vector3 forward
        {
            get { return rotation * Vector3.forward; }
            set { rotation = Quaternion.LookRotation(value); }
        }

   
		
		public StoredTransform (Transform t)
		{
			this.position = t.position;
			this.rotation = t.rotation;
		}

		public static implicit operator UnityEngine.Transform(StoredTransform t)  // implicit digit to byte conversion operator
		{
			System.Console.WriteLine("conversion occurred");
			return new StoredTransform(t);
		}
	}
}
