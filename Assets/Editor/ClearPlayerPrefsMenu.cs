using UnityEditor;
using UnityEngine;

public static class ClearPlayerPrefsMenu
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        if (EditorUtility.DisplayDialog(
            "PlayerPrefs 초기화",
            "정말 PlayerPrefs를 모두 삭제하시겠습니까?",
            "삭제",
            "취소"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            Debug.Log("PlayerPrefs 초기화 완료");
        }
    }
}