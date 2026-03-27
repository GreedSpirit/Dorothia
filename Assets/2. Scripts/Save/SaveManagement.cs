using System;
using System.Collections.Generic;
using UnityEngine;

public static class SaveManagement
{
    public static void Save<T>(string key, T data)
    {
        //저장하고자 하는 데이터의 클래스를 덮어줄 Wrapper 생성
        var wrapper = new Wrapper<T>(data);
        //해당 Wrapper 기준으로 Json파일 생성
        string json = JsonUtility.ToJson(wrapper);

        //PlayerPrefs를 통해 저장
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    public static T Load<T>(string key)
    {
        //Key가 저장해둔 공간에 유효하지 않으면 반환
        if (!PlayerPrefs.HasKey(key))
            return default;

        //key값에 맞는 json 형성
        string json = PlayerPrefs.GetString(key);
        //그 json을 기반으로 Wrapper 복원
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);

        //각 클래스의 정보를 담은 Wrapper을 반환
        return wrapper.data;
    }

    //저장 시에 사용할, JsonUtility가 지원하지 않는 기능을 대비한 Wrapper.
    //BigInteger의 경우 BigIntegerWrapper 사용!
    [Serializable]
    private class Wrapper<T>
    {
        //담아둘 정보의 클래스
        public T data;

        public Wrapper(T data)
        {
            this.data = data;
        }
    }
}
