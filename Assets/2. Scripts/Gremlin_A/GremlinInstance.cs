using System;
using UnityEngine;

public class GremlinInstance : MonoBehaviour
{
    [SerializeField] private GremlinMovement _movement;                 // 움직임 제어
    [SerializeField] private GremlinVisual _visual;                     // 시각적 제어
    [SerializeField] private GremlinRarityModelingManager _modeling;    // 모델링 제어
    public GremlinBehaviour _behaviour { get; private set; }            // 행동 제어
    public GremlinAnimatorController _controller { get; private set; }  // 애니메이션 제어

    public void Init(Gremlin gremlin)
    {
        if(gremlin._gremlinData.Type == Gremlin_Type.공격형)
        {
            //공격형 전용 GremlinBehaviour "스트라이커그렘린" 추가
            if(_behaviour == null)
            {
                gameObject.AddComponent<StrikerGremlin>();
            }

            //추가된 컴포넌트 가져옴
            StrikerGremlin striker = gameObject.GetComponent<StrikerGremlin>();

            //해당 컴포넌트의 초기 함수 실행
            //해당 그렘린에게 할당된 모든 동작, 플레이어의 위치, 해당 그렘린의 등급을 인자값으로 사용.
            striker.Init(ItemCalculator.GetGremlinEffect(gremlin._gremlinData.PetID),
                GremlinManager.Instance.PlayerTransform, gremlin._rarity);

            //그렘린의 최종 공격력 : (공격력 * 등급 배율) + (그렘린 강화수치 * 강화 배율)
            striker.finalAttack = ItemCalculator.StrikerGremlinValueCalc(this, striker);

            //변경이 완료된 그렘린 동작 투입
            _behaviour = striker;
        }
        else if(gremlin._gremlinData.Type == Gremlin_Type.지원형)
        {
            //지원형 그렘린 전용 GremlinBehaviour "버퍼그렘린" 추가
            if(_behaviour == null)
            {
                gameObject.AddComponent<BufferGremlin>();
            }

            //추가된 컴포넌트 가져옴
            BufferGremlin buffer = gameObject.GetComponent<BufferGremlin>();

            //해당 컴포넌트의 초기 함수 실행
            //해당 그렘린에게 할당된 모든 동작, 플레이어의 위치, 해당 그렘린의 등급을 인자값으로 사용
            buffer.Init(ItemCalculator.GetGremlinEffect(gremlin._gremlinData.PetID),
                GremlinManager.Instance.PlayerTransform, gremlin._rarity);

            //초기 동작을 통해 추가된 액티브 스킬 수가 0 이상인 경우(존재할 경우)
            if(buffer.ActiveStatus.Count > 0)
            {
                //버프값은 0
                float buffValue = 0;
                //Dictionary의 값을 가져옴 ( 기획상 1개만 사용, 확장 고려 X )
                foreach(var item in buffer.ActiveStatus.Values)
                {
                    buffValue = item;
                }

                //버프의 최종값 : (버프값 * 등급 배율) + (강화 수치 * 강화 보너스)
                buffer.finalValue = ItemCalculator.BufferGremlinValueCalc(this, buffValue, buffer);
            }

            
            //버퍼그렘린의 행동 시의 액션에 움직임제어 - 버프 사용시 고정 상태로 변화
            buffer.onActing += _movement.ChangeActingState;

            //변경 완료된 버퍼그렘린 투입
            _behaviour = buffer;
        }

        //행동의 초기 함수 실행
        _movement.Init(GremlinManager.Instance.PlayerTransform);
        
        //애니메이션 제어 전용 컴포넌트 추가
        GremlinAnimatorController controller = gameObject.AddComponent<GremlinAnimatorController>();

        //해당 클래스의 초기함수 실행
        controller.Init();

        //변경 완료된 컴포넌트 투입
        _controller = controller;
    }

    /// <summary>
    /// 그렘린 등급에 따라 외형을 변화시킵니다.
    /// </summary>
    /// <param name="rarity">현재 그렘린 등급</param>
    public void ChangeModeling(Rarity rarity)
    {
        //모델링 제어 컴포넌트가 있을 때만 동작합니다.
        if(_modeling != null)
        {
            //등급에 따라 모델링을 변화시킵니다.
            _modeling.ChangeModel(rarity);
        }
    }

    public void ReCalculation(Gremlin gremlin)
    {
        if(_behaviour is BufferGremlin)
        {
            BufferGremlin buffer = gameObject.GetComponent<BufferGremlin>();

            //버프값은 0
            float buffValue = 0;
            //Dictionary의 값을 가져옴 ( 기획상 1개만 사용, 확장 고려 X )
            foreach (var item in buffer.PassiveStatus.Values)
            {
                buffValue = item;
            }

            //버프의 최종값 : (버프값 * 등급 배율) + (강화 수치 * 강화 보너스)
            buffer.finalValue = ItemCalculator.BufferGremlinValueCalc(this, buffValue, buffer);
        }
        else if(_behaviour is StrikerGremlin)
        {
            StrikerGremlin striker = gameObject.GetComponent<StrikerGremlin>();

            striker.finalAttack = ItemCalculator.StrikerGremlinValueCalc(this, striker);
        }
    }
}
