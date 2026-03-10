using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using System.IO;

public class EffectAutoAddresser : AssetPostprocessor
{
    private const string EffectFolder = "Assets/3. Prefabs/Effects";
    // 설정하고자 하는 그룹명
    private const string TargetGroupName = "SkillEffect"; 

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;

        // "SkillEffect" 그룹 찾기 또는 생성
        AddressableAssetGroup skillEffectGroup = settings.FindGroup(TargetGroupName);
        if (skillEffectGroup == null)
        {
            skillEffectGroup = settings.CreateGroup(TargetGroupName, false, false, false, settings.DefaultGroup.Schemas);
        }

        foreach (string path in importedAssets)
        {
            // 경로 확인 및 프리팹 여부 체크
            if (path.StartsWith(EffectFolder) && path.EndsWith(".prefab"))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);

                // 지정된 그룹(SkillEffect)으로 엔트리 생성/이동
                var entry = settings.CreateOrMoveEntry(guid, skillEffectGroup);

                if (entry != null)
                {
                    // 주소를 파일 이름으로 자동 설정
                    entry.address = Path.GetFileNameWithoutExtension(path);

                    // 선택 사항: 라벨도 자동으로 붙이고 싶다면 추가
                    // entry.SetLabel("Effect", true);
                }
            }
        }

        // 변경사항 저장 및 UI 갱신
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
    }
}