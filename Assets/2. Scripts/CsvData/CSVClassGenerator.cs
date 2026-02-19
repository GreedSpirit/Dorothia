using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class CSVClassGenerator : EditorWindow
{
    // 저장 경로
    private string _targetPath = "Assets/2. Scripts/CsvData/Data";

    [MenuItem("Tools/CSV to Class")]
    public static void ShowWindow()
    {
        GetWindow<CSVClassGenerator>("CSV Class Generator");
    }

    private void OnGUI()
    {
        // 경로 설정
        GUILayout.Label("저장 경로 :");
        _targetPath = EditorGUILayout.TextField(_targetPath);

        GUILayout.Space(10);

        if (GUILayout.Button("선택한 CSV 파일로 클래스 생성"))
        {
            GenerateClasses();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("CSV 파일들을 선택한 후 버튼을 누르세요.\n" +
                                "CSV 규칙: 3번째 줄(변수명), 4번째 줄(자료형)이라고 가정한 자동화", MessageType.Info);
    }

    private void GenerateClasses()
    {
        // 선택된 파일들 가져오기
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("선택된 파일이 없습니다. CSV 파일을 선택해주세요.");
            return;
        }

        // 저장 경로 확인 및 없으면 생성
        if (!Directory.Exists(_targetPath))
        {
            Directory.CreateDirectory(_targetPath);
        }

        int count = 0;

        foreach (Object obj in selectedObjects)
        {
            TextAsset csvFile = obj as TextAsset;
            if (csvFile == null) continue;

            string content = GenerateClassContent(csvFile.name, csvFile.text);
            if (string.IsNullOrEmpty(content)) continue;

            string path = $"{_targetPath}/{csvFile.name}Data.cs"; // 파일명 뒤에 Data
            File.WriteAllText(path, content, Encoding.UTF8);
            Debug.Log($"생성 완료: {path}");
            count++;
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{count}개의 클래스 파일이 생성", "확인");
    }

    private string GenerateClassContent(string fileName, string csvText)
    {
        string[] lines = csvText.Replace("\r\n", "\n").Split('\n');

        if (lines.Length < 4)
        {
            Debug.LogError($"{fileName}: CSV 형식 문제 (줄 수 부족)");
            return null;
        }

        string[] varNames = ParseCsvLine(lines[2]);
        string[] varTypes = ParseCsvLine(lines[3]);

        if (varNames.Length != varTypes.Length)
        {
            Debug.LogError($"{fileName}: 변수명 개수와 자료형 개수 불일치");
            return null;
        }

        string className = $"{fileName}Data";
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("[Serializable]");
        sb.AppendLine($"public class {className} : ICSVLoad, ITableKey");
        sb.AppendLine("{");

        //프로퍼티
        for (int i = 0; i < varNames.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(varNames[i])) continue;
            sb.AppendLine($"    public {varTypes[i]} {varNames[i]} {{ get; set; }}");
        }
        sb.AppendLine();

        // ITableKey
        // 만약 첫 컬럼이 int가 아니면 오류가 날 수도 있으니 기획팀과 얘기해봅시다        
        sb.AppendLine($"    int ITableKey.Id => {varNames[0]};"); 
        // Key값 사용이 필요하다면 그건 따로 데이터 스크립트에서 설정해줍시다!
        sb.AppendLine($"    string ITableKey.Key => {varNames[0]}.ToString();");

        sb.AppendLine();

        // LoadFromCsv 구현
        sb.AppendLine("    public void LoadFromCsv(string[] values)");
        sb.AppendLine("    {");
        
        for (int i = 0; i < varNames.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(varNames[i])) continue;

            string type = varTypes[i].ToLower();
            string name = varNames[i];

            sb.AppendLine($"        // {i}: {name} ({type})");

            if (type == "int")
            {
                sb.AppendLine($"        if (values.Length > {i} && int.TryParse(values[{i}], out int v{i})) {name} = v{i};");
            }
            else if (type == "float")
            {
                sb.AppendLine($"        if (values.Length > {i} && float.TryParse(values[{i}], out float v{i})) {name} = v{i};");
            }
            else if (type == "bool")
            {
                sb.AppendLine($"        if (values.Length > {i} && bool.TryParse(values[{i}], out bool v{i})) {name} = v{i};");
            }
            else if (type == "string")
            {
                sb.AppendLine($"        if (values.Length > {i}) {name} = values[{i}];");
            }
            else
            {
                // Enum이나 기타 타입은 string으로 받거나 주석 처리
                sb.AppendLine($"        // TODO: {type} 타입 파싱 로직 추가 필요");
                sb.AppendLine($"        if (values.Length > {i} && Enum.TryParse(values[{i}], out enum v{i})) {name} = v{i};");
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // 쉼표 분리
    private string[] ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"') inQuotes = !inQuotes;
            else if (line[i] == ',' && !inQuotes)
            {
                result.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else sb.Append(line[i]);
        }
        result.Add(sb.ToString().Trim());
        return result.ToArray();
    }
}