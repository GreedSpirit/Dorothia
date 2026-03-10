using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;

public class SkillResourceAutoAddresser : AssetPostprocessor
{
    // 경로 설정
    private const string EffectFolder = "Assets/3. Prefabs/Effects";
    private const string IconFolder = "Assets/Image/Skills";

    // 그룹명 설정
    private const string EffectGroupName = "SkillEffect";
    private const string IconGroupName = "SkillIcon";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;

        foreach (string path in importedAssets)
        {
            // 스킬 이펙트 (프리팹) 처리
            if (path.StartsWith(EffectFolder) && path.EndsWith(".prefab"))
            {
                ProcessAsset(settings, path, EffectGroupName);
            }
            // 스킬 아이콘 (이미지) 처리
            else if (path.StartsWith(IconFolder) && (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".tga")))
            {
                ProcessAsset(settings, path, IconGroupName);
            }
        }
    }

    private static void ProcessAsset(AddressableAssetSettings settings, string path, string groupName)
    {
        // 그룹 찾기 또는 생성
        AddressableAssetGroup targetGroup = settings.FindGroup(groupName);
        if (targetGroup == null)
        {
            // 기본 스키마를 사용하여 새 그룹 생성
            targetGroup = settings.CreateGroup(groupName, false, false, false, settings.DefaultGroup.Schemas);
        }

        string guid = AssetDatabase.AssetPathToGUID(path);
        var entry = settings.CreateOrMoveEntry(guid, targetGroup);

        if (entry != null)
        {
            // 주소를 확장자 없는 파일 이름으로 설정 (예: FireBall_Icon)
            entry.address = Path.GetFileNameWithoutExtension(path);

            // 라벨 넣을거라면
            //entry.SetLabel("Effect", true);

            // 변경사항 기록
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        }
    }
}