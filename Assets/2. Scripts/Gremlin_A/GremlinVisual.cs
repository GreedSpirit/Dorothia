using UnityEngine;

//시각적 움직임을 제어하기 위한 클래스입니다.
public class GremlinVisual : MonoBehaviour
{
    [SerializeField] protected Transform visualModel;

    [SerializeField] private float floatStrength = 0.5f;                // 떠 있는 힘
    [SerializeField] private float floatSpeed = 2.0f;                   // 떠다니는 움직임 속도

    private Vector3 visualModelStartLocalPos;                           // 플레이어와의 움직임과는 별개로 공중에 떠 있는 애니메이션을 위한 VisualModel.

    private void Update()
    {
        HandleFloating();
    }

    /// <summary>
    /// 공중에 떠 있을 때의 움직임을 제어합니다. 시각적 움직임을 위한 연출에 불과하며, 실제 위치가 변하지는 않습니다.
    /// </summary>
    private void HandleFloating()
    {
        //시각적 모델이 없으면 반환합니다.
        if (visualModel == null)
        {
            return;
        }
        // 기존 y값에 Sin을 이용합니다.
        // time으로 -1 ~ 1의 흐름을 제어, 빠르기는 speed로, 부유하는 높낮이는 strenth로 제어합니다.
        float newY = visualModelStartLocalPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatStrength);

        //시각적으로 보여지는 모델의 위치는, Sin값을 이용해 구한 Y값만 변동이 생깁니다.
        visualModel.localPosition = new Vector3(visualModelStartLocalPos.x, newY, visualModelStartLocalPos.z);
    }
}
