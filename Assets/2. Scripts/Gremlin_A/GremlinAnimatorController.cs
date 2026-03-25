using UnityEngine;

public class GremlinAnimatorController : MonoBehaviour
{
    [SerializeField] Animator animator;                // 그렘린의 애니메이터
    [SerializeField] GremlinBehaviour behaviour;       // 그렘린의 행동 ( 버프 / 공격 )

    /// <summary>
    /// 처음 생성되면서 같이 호출시킬 함수
    /// </summary>
    public void Init()
    {
        animator = gameObject.GetComponentInChildren<Animator>();  // 그렘린 애니메이터를 가지고 있는 오브젝트는 그렘린 오브젝트 하위에 있음.
        behaviour = gameObject.GetComponent<GremlinBehaviour>();   // 그렘린 오브젝트로부터 행동을 가져와야 함. Tick이 발동되는 순간을 찾아야 하기 때문.

        //행동(공격형 / 버프형의 행동)이 존재하는 경우 액션에 애니메이터 파라미터 변경 기능 추가
        if(behaviour != null)
        {
            behaviour.OnTick += ChangeAnim;
        }    
    }

    /// <summary>
    /// 그렘린 애니메이션의 공통 트리거를 변경합니다.
    /// </summary>
    public void ChangeAnim()
    {
        animator.SetTrigger("Attack");
    }
}
