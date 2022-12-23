using UnityEngine;

public abstract class Manager<T> : MonoBehaviour where T : Manager<T>
{
	private static T _instance;
	public static T Instance { 
		get {
			if (_instance == null)
			{
				_instance = FindObjectOfType<T>();
				if (_instance == null)
				{
					Debug.LogWarning("No " + typeof(T).Name + " in scene! Creating one...");
					var go = new GameObject(typeof(T).Name);
					_instance = go.AddComponent<T>();
				}
			}
			return _instance;
		}
		private set => _instance = value;
	}

	public bool DontDestroy = false;

	protected virtual void Awake()
	{
		if (this is not T)
		{
			Debug.LogWarning("Manager is not of type " + typeof(T).Name + "! Destroying self...");
			Destroy(gameObject);
			return;
		}

		if (_instance == null)
		{
			Instance = this as T;
			if (DontDestroy) DontDestroyOnLoad(gameObject);
		}
		else
		{
			Debug.LogWarning("Multiple " + typeof(T).Name + " in scene! Destroying self... : " + gameObject.name + " and " + Instance.gameObject.name);
			Destroy(gameObject);
		}
	}
}