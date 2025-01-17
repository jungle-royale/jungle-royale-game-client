using UnityEngine;

public class GroundCrack : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Vector3[] _originalVertices;
    private Vector3[] _currentVertices;

    [SerializeField] private float crackSpeed = 1.0f; // 갈라지는 속도
    private Vector3 _meshCenter;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();

        if (_meshFilter != null)
        {
            _originalVertices = _meshFilter.mesh.vertices;
            _currentVertices = (Vector3[])_originalVertices.Clone();

            // 메시의 중심 계산 (Local Space 기준)
            _meshCenter = Vector3.zero;
            foreach (var vertex in _originalVertices)
            {
                _meshCenter += vertex;
            }
            _meshCenter /= _originalVertices.Length;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space)) // Space 키를 눌러 갈라짐 시작
        {
            ApplyCrackEffect();
        }
    }

    private void ApplyCrackEffect()
    {
        for (int i = 0; i < _currentVertices.Length; i++)
        {
            // 중심으로부터 각 정점까지의 방향 벡터 계산
            Vector3 direction = (_originalVertices[i] - _meshCenter).normalized;

            // 방향 벡터를 따라 정점 이동
            _currentVertices[i] += direction * crackSpeed * Time.deltaTime;
        }

        // 메시 업데이트
        _meshFilter.mesh.vertices = _currentVertices;
        _meshFilter.mesh.RecalculateNormals();
        _meshFilter.mesh.RecalculateBounds();
    }
}