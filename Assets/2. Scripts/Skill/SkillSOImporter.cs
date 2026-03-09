using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class SkillCSVImporter
{
    private static string csvPath = "Assets/Resources/Skill.csv";
    private static string savePath = "Assets/Resources/Skill/Data";

    [MenuItem("Tools/Import Skills from CSV")]
    public static void Import()
    {
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        string[] lines = File.ReadAllLines(csvPath);

        for (int i = 4; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] row = lines[i].Split(',');

            int id = int.Parse(row[0]);
            string name = row[1];
            Skill_Type type = (Skill_Type)int.Parse(row[2]);
            float cool = float.Parse(row[3]);
            Skill_Target target = (Skill_Target)int.Parse(row[4]);
            int statusId = int.Parse(row[5]);
            string iconName = row[6];
            string sfxPath = row[7];
            string effectName = row[8];
            string animPath = row[9];

            UpdateOrCreateSO(id, name, type, cool, target, statusId, iconName, sfxPath, effectName, animPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=yellow>CSV 임포트 완료!</color>");
    }

    private static void UpdateOrCreateSO(int id, string name, Skill_Type type, float cool, Skill_Target target, int statusId, string icon, string sfx, string effect, string anim)
    {
        string assetPath = $"{savePath}/Skill_{id}.asset";
        SkillSOData asset = AssetDatabase.LoadAssetAtPath<SkillSOData>(assetPath);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SkillSOData>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        // 데이터 할당
        asset.Job_Skill_Id = id;
        asset.Skill_Name = name;
        asset.Skill_Type = type;
        asset.Skill_Cooltime = cool;
        asset.Skill_Target = target;
        asset.Skill_Status_Id = statusId;
        asset.Skill_Animation_Path = anim;

        // 리소스 자동 연결 (Resources 폴더 기준)
        //if (!string.IsNullOrEmpty(icon))
        //    asset.Skill_Icon = Resources.Load<Sprite>($"Icons/{icon}");

        //if (!string.IsNullOrEmpty(sfx))
        //    asset.Skill_Sfx = Resources.Load<AudioClip>($"Sounds/{sfx}");

        if (!string.IsNullOrEmpty(effect))
            asset.Skill_Effect = Resources.Load<EffectData>($"Effects/{effect}");

        EditorUtility.SetDirty(asset);
    }
}