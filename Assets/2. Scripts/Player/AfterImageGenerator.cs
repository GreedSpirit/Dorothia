using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class AfterImageGenerator : MonoBehaviour
{
    [Header("Settings")]
    public float ghostDelay = 0.1f;
    public float ghostDuration = 0.5f;
    public Color ghostColor = new Color(0, 0.5f, 1f, 1f);

    [Header("References")]
    public SkinnedMeshRenderer targetRenderer;
    public Material ghostMaterial;

    private IObjectPool<AfterImageGhost> ghostPool;
    private bool isGenerating = false;

    private void Awake()
    {
        ghostPool = new ObjectPool<AfterImageGhost>(
            CreateGhost,       // 새 객체 생성 로직
            OnGetGhost,        // 풀에서 꺼낼 때
            OnReleaseGhost,    // 풀에 반납할 때
            OnDestroyGhost,    // 풀 용량 초과 시 파괴
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 20
        );
    }

    #region Pool Callbacks
    private AfterImageGhost CreateGhost()
    {
        GameObject go = new GameObject("Ghost_Pool_Item");
        return go.AddComponent<AfterImageGhost>();
    }

    private void OnGetGhost(AfterImageGhost ghost)
    {
        ghost.gameObject.SetActive(true);
    }

    private void OnReleaseGhost(AfterImageGhost ghost)
    {
        ghost.gameObject.SetActive(false);
    }

    private void OnDestroyGhost(AfterImageGhost ghost)
    {
        Destroy(ghost.gameObject);
    }
    #endregion

    public void StartAfterImage()
    {
        if (!isGenerating)
        {
            isGenerating = true;
            StartCoroutine(CreateGhostCoroutine());
        }
    }

    public void StopAfterImage()
    {
        isGenerating = false;
    }

    private IEnumerator CreateGhostCoroutine()
    {
        while (isGenerating)
        {
            // 현재 포즈 Bake
            Mesh bakedMesh = new Mesh();
            targetRenderer.BakeMesh(bakedMesh);

            // 풀에서 잔상 가져오기
            AfterImageGhost ghost = ghostPool.Get();

            // 위치 설정 및 초기화
            ghost.transform.SetPositionAndRotation(targetRenderer.transform.position, targetRenderer.transform.rotation);
            ghost.Init(ghostPool, bakedMesh, ghostMaterial, ghostColor, ghostDuration);

            yield return new WaitForSeconds(ghostDelay);
        }
    }
}