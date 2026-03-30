using UnityEngine;
using UnityEngine.UI;

public class DungeonEnter : MonoBehaviour
{
    [SerializeField] private  DungeonInfo _targetpanel;

    //던전id
    [SerializeField] int _dungeonId;

    void Awake()
    {
        var btn = GetComponent<Button>();

        btn.onClick.AddListener(() => _targetpanel.Open(_dungeonId));
    }
}
