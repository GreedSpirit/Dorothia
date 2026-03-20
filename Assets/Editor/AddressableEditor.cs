using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public class AddressableEditor : EditorWindow
{
    private const string EffectFolder = "Assets/Resources/Skill/Effect";
    private const string MotionFolder = "Assets/Resources/Skill/Motion";
    private const string IconFolder = "Assets/Resources/Skill/Icon";

    private const string EquipFolder = "Assets/99. IgnoredAssets/EquipIcon/";

    [MenuItem("Tools/Resource Manager")]
    public static void ShowWindow()
    {
        GetWindow<AddressableEditor>("Resource Manager");
    }

    private void OnGUI()
    {
        GUILayout.Label("Skill Resource Auto Addressable Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox($"설정된 경로:\n1. 스킬 이펙트: {EffectFolder}" +
        $"\n2. 스킬 아이콘: {IconFolder}" +
        $"\n2. 장비 아이콘: {EquipFolder}", MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("전체 에셋 다시 스캔 및 등록", GUILayout.Height(40)))
        {
            RefreshAllSkillAssets();
        }

        if (GUILayout.Button("Addressable 설정 열기", GUILayout.Height(30)))
        {
            EditorApplication.ExecuteMenuItem("Window/Asset Management/Addressables/Groups");
        }
    }

    private void RefreshAllSkillAssets()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("Addressable Settings를 찾을 수 없습니다. 먼저 Settings를 생성하세요.");
            return;
        }

        int count = 0;
        // 스킬 모션 폴더 스캔
        count += ScanDirectory(settings, MotionFolder, "SkillMotion", "*.anim");
        // 스킬 이펙트 폴더 스캔
        count += ScanDirectory(settings, EffectFolder, "SkillEffect", "*.prefab");
        // 스킬 아이콘 폴더 스캔 (다중 확장자 지원)
        count += ScanDirectory(settings, IconFolder, "SkillIcon", "*.png");
        count += ScanDirectory(settings, IconFolder, "SkillIcon", "*.jpg");
        count += ScanDirectory(settings, IconFolder, "SkillIcon", "*.tga");
        // 장비 아이콘 폴더 스캔
        foreach (var type in Enum.GetValues(typeof(Equip_Type)))
        {
            string folderPath = EquipFolder + type.ToString();
            count += ScanDirectory(settings, folderPath, type.ToString(), "*.png");
            count += ScanDirectory(settings, folderPath, type.ToString(), "*.jpg");
            count += ScanDirectory(settings, folderPath, type.ToString(), "*.tga");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"총 {count}개의 에셋이 어드레서블로 업데이트되었습니다.");
        EditorUtility.DisplayDialog("완료", $"{count}개의 에셋 등록 완료!", "확인");
    }

    private int ScanDirectory(AddressableAssetSettings settings, string path, string groupName, string filter)
    {
        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"경로가 존재하지 않습니다: {path}");
            return 0;
        }

        string[] files = Directory.GetFiles(path, filter, SearchOption.AllDirectories);
        int updatedCount = 0;

        foreach (string filePath in files)
        {
            // 시스템 경로를 유니티 프로젝트 상대 경로로 변환
            //string relativePath = filePath.Replace(Application.dataPath, "Assets").Replace("\\", "/");

            string relativePath = filePath.Replace("\\", "/");
            if (relativePath.StartsWith(Application.dataPath.Replace("\\", "/")))
            {
                relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
            }

            if (ApplyAddressable(settings, relativePath, groupName))
            {
                updatedCount++;
            }
        }
        return updatedCount;
    }

    private bool ApplyAddressable(AddressableAssetSettings settings, string path, string groupName)
    {
        AddressableAssetGroup targetGroup = settings.FindGroup(groupName);
        if (targetGroup == null)
        {
            targetGroup = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));
        }

        string guid = AssetDatabase.AssetPathToGUID(path);
        var entry = settings.CreateOrMoveEntry(guid, targetGroup);

        if (entry != null)
        {
            string newAddress = Path.GetFileNameWithoutExtension(path);
            if (entry.address == newAddress) return false;

            entry.address = newAddress;
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
            return true;
        }
        return false;
    }
}