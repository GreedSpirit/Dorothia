using UnityEngine;
using UnityEngine.Pool;

public class AfterImageGhost : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material ghostMaterial;

    private IObjectPool<AfterImageGhost> pool; // 반납할 풀 참조
    private float activeTime;
    private float timeStarted;

    private readonly int alphaID = Shader.PropertyToID("_Alpha");
    private readonly int colorID = Shader.PropertyToID("_GhostColor");
    private void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    public void Init(IObjectPool<AfterImageGhost> pool, Mesh mesh, Material mat, Color color, float duration)
    {
        this.pool = pool;
        meshFilter.mesh = mesh;

        // 머티리얼 인스턴스 생성 최소화 (동적 생성 대신 공유 머티리얼 사용 권장)
        //공유 머터리얼 사용할 시 개별 fade, color가 불가 그래서 MaterialPropertyBlock을 사용해야함
        //meshRenderer.sharedMaterial = mat;
        if (ghostMaterial == null) ghostMaterial = new Material(mat);
        meshRenderer.material = ghostMaterial;

        ghostMaterial.SetColor(colorID, color);
        activeTime = duration;
        timeStarted = Time.time;
    }

    private void Update()
    {
        float timeElapsed = Time.time - timeStarted;
        float fraction = 1f - (timeElapsed / activeTime);

        if (fraction <= 0)
        {
            pool.Release(this); // 풀로 반납
        }
        else
        {
            ghostMaterial.SetFloat(alphaID, fraction);
        }
    }
}