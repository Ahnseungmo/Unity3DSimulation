using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // 씬에서 이미 존재하는 인스턴스가 있는지 검사
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                {
                    // 없으면 새 GameObject 생성 후 컴포넌트 추가
                    GameObject singletonObj = new GameObject(typeof(T).Name + " (Singleton)");
                    _instance = singletonObj.AddComponent<T>();
                    DontDestroyOnLoad(singletonObj);
                }
            }

            return _instance;
        }
    }

    // 중복 방지
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
