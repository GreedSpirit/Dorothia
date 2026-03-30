using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [SerializeField] private float autoSaveSecond = 60f;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {
        OnClickStartGame();
    }

    //종료 시
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    //홈 버튼 등으로 인한 중지 상태에서의 강제종료 시
    private void OnApplicationPause(bool pause)
    {
        if (pause == true)
            SaveGame();
    }

    private IEnumerator AutoSaveCoroutine()
    {
        while(true)
        {
            //정해진 시간만큼 대기한 후
            yield return new WaitForSeconds(autoSaveSecond);

            //게임 저장
            SaveGame();
        }
    }

    public void OnClickStartGame()
    {
        Instance.LoadGame();
        StartCoroutine(AutoSaveCoroutine());
    }

    private void SaveGame()
    {
        var saveData = CreateSaveData();
        SaveManagement.Save("GameData", saveData);
        Debug.Log("저장 완료!");
    }

    private SaveData CreateSaveData()
    {
        Debug.Log(EquipmentInventory.Instance);
        Debug.Log(GremlinInventory.Instance);
        return new SaveData
        {
            equipInv = EquipmentInventory.Instance.GetSaveData(),
            GremlinInv = GremlinInventory.Instance.GetSaveData(),
            skillData = SkillManager.Instance.GetSaveData()
        };
    }
    public async void LoadGame()
    {
        var data = SaveManagement.Load<SaveData>("GameData");

        if (data == null) return;
        Debug.Log(EquipmentInventory.Instance);
        Debug.Log(GremlinInventory.Instance);
        EquipmentInventory.Instance.LoadFromSaveData(data.equipInv);
        GremlinInventory.Instance.LoadFromSaveData(data.GremlinInv);
        SkillManager.Instance.LoadFromSaveData(data.skillData);
    }
}
