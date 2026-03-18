using UnityEngine;

/// <summary>
/// EffectManager의 오브젝트 풀에서 재사용될 때 스스로 상태를 초기화하고
/// 지속 시간을 알려줄 수 있는 이펙트 컴포넌트용 인터페이스.
///
/// ParticleSystem이 없는 커스텀 이펙트(예: MultipleObjectsMake)에 구현한다.
/// </summary>
public interface IResettableEffect
{
    /// <summary>풀에서 꺼낼 때 내부 상태를 초기 상태로 되돌린다.</summary>
    void ResetEffect();

    /// <summary>이 이펙트가 완전히 끝나기까지 걸리는 시간(초)을 반환한다.</summary>
    float GetEffectDuration();
}
