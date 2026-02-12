using UnityEngine;
using System.Collections.Generic;
using System;

public class CSVReader
{
    public static List<T> Read<T>(string file) where T : ICSVLoad, new()
    {
        List<T> list = new List<T>();
        TextAsset data = Resources.Load<TextAsset>(file);

        // 헤더로 사용할 줄 수 (상단 IgnoreHeaderCount줄만큼 무시)
        const int IgnoreHeaderCount = 4;

        if (data == null)
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {file}");
            return list;
        }

        // 엔터키 처리 (\r\n 또는 \n)
        string[] lines = data.text.Replace("\r\n", "\n").Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            if(i < IgnoreHeaderCount) continue;
            string line = lines[i];

            // 빈 줄이나 주석(#) 처리
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            string[] values = ParseCsvLine(line).ToArray();
            for (int v = 0; v < values.Length; v++)
            {
                values[v] = NormalizeCsvString(values[v]);
            }
            // 데이터가 비어있으면 건너뛰기
            if (values.Length == 0 || string.IsNullOrWhiteSpace(values[0])) continue;
            
            try
            {
                T entry = new T();
                entry.LoadFromCsv(values); // 각 데이터 클래스의 LoadFromCsv 실행
                list.Add(entry);
            }
            catch (Exception e)
            {
                Debug.LogError($"CSV 파싱 오류 ({file} - {i}번 줄): {e.Message}");
            }
        }

        return list;
    }
    private static List<string> ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            // 큰따옴표 처리
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++; // 다음 따옴표 스킵
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            // 쉼표 처리
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());

        return result;
    }
    private static string NormalizeCsvString(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;

        s = s.Trim();

        // 바깥 따옴표 1겹만 제거
        if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
        {
            s = s.Substring(1, s.Length - 2);
        }
        return s;
    }
}