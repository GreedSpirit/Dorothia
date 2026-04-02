using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/*
[Save System 구조]

1. 런타임
   - Dictionary + BigInteger 사용 (성능/정확성)

2. 저장
   - JsonUtility 사용
   - Dictionary → List 변환
   - BigInteger → string 변환

3. 보안
   - AES 암호화 적용
   - 파일 직접 수정 방지

4. 저장 위치
   - Application.persistentDataPath
   - 모든 플랫폼 대응

5. 저장 전략
   - 재화 변경 시 즉시 저장
   - 일정 시간 자동 저장
   - 앱 종료 시 저장
*/

/// <summary>
/// Base64 -> 복호화 -> JSON -> 객체 변환
/// </summary>
public static class SaveUtility
{
    //보안 키 (byte 배열)
    //AES는 키 길이가 반드시 16 / 24 / 32 byte여야 함
    //byte 배열로 직접 정의해서 길이 오류 방지
    private static readonly byte[] KEY = new byte[32]
    {
        21, 12, 53, 14, 95, 26, 37, 48,
        59, 10, 11, 92, 13, 24, 35, 46,
        57, 68, 79, 80, 91, 12, 23, 34,
        45, 56, 67, 78, 89, 90, 11, 22
    };

    //IV는 16 byte 고정
    //AES CBC 모드에서 필수
    private static readonly byte[] IV = new byte[16]
    {
        11, 22, 33, 44, 55, 66, 77, 88,
        99, 10, 20, 30, 40, 50, 60, 70
    };

    /// <summary>
    /// 저장
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <param name="data"></param>
    public static void SaveEncrypted<T>(string fileName, T data)
    {
        try
        {
            string path = GetPath(fileName);

            string json = JsonUtility.ToJson(data, true);
            string encrypted = Encrypt(json);

            File.WriteAllText(path, encrypted);


        }
        catch (Exception e)
        {
            Debug.LogError($"Save 실패: {e}");
        }
    }

    /// <summary>
    /// 로드
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T LoadEncrypted<T>(string fileName)
    {
        try
        {
            string path = GetPath(fileName);

            if (!File.Exists(path))
                return default;

            string encrypted = File.ReadAllText(path);
            string json = Decrypt(encrypted);

            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Load 실패: {e}");
            return default;
        }
    }

    /// <summary>
    /// 경로처리
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static string GetPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName + ".json");
    }

    /// <summary>
    /// 암호화
    /// </summary>
    /// <param name="plainText"></param>
    /// <returns></returns>
    private static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();

        aes.Key = KEY;
        aes.IV = IV;

        aes.Mode = CipherMode.CBC;       // 명시적 설정
        aes.Padding = PaddingMode.PKCS7; // 명시적 설정

        using var encryptor = aes.CreateEncryptor();

        byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// 복호화
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    private static string Decrypt(string cipherText)
    {
        using Aes aes = Aes.Create();

        aes.Key = KEY;
        aes.IV = IV;

        //AES 설정 명시
        //플랫폼마다 기본값 차이 방지 (모바일)
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();

        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }
}