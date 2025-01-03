// using UnityEngine;
// using System;
// using System.Collections;
// using System.Collections.Generic;

// public class EffectManager : Singleton<EffectManager>
// {
//     private Dictionary<string, GameObject> effectPrefabs = new Dictionary<string, GameObject>();

//     /// 특정 경로의 이펙트 프리팹 로드 (캐싱)
//     public GameObject LoadEffect(string path)
//     {
//         if (!effectPrefabs.ContainsKey(path))
//         {
//             GameObject prefab = Resources.Load<GameObject>(path);
//             if (prefab == null)
//             {
//                 Debug.LogError($"이펙트 프리팹을 {path}에서 찾을 수 없습니다!");
//                 return null;
//             }
//             effectPrefabs[path] = prefab;
//         }
//         return effectPrefabs[path];
//     }

//     /// 이펙트를 생성하고 자동 삭제 처리
//     public GameObject PlayEffect(string effectPath, Vector3 position, Quaternion rotation, Transform parent = null)
//     {
//         GameObject prefab = LoadEffect(effectPath);
//         if (prefab == null) return null;

//         // Prefab을 인스턴스화
//         GameObject effectInstance = Instantiate(prefab, position, rotation);

//         // 부모 객체 설정 (필요할 경우)
//         if (parent != null)
//         {
//             effectInstance.transform.SetParent(parent);
//         }

//         // 자동 삭제 처리
//         StartCoroutine(DestroyEffectWhenDone(effectInstance));

//         return effectInstance; // 인스턴스 반환
//     }

//     private IEnumerator DestroyEffectWhenDone(GameObject effect)
//     {
//         // 하위에 있는 모든 ParticleSystem 찾기
//         ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>();

//         if (particleSystems == null || particleSystems.Length == 0)
//         {
//             Debug.LogError("ParticleSystem이 프리팹에 없습니다!");
//             yield break;
//         }

//         // 모든 ParticleSystem이 종료될 때까지 대기
//         bool isAlive = true;
//         while (isAlive)
//         {
//             isAlive = false;
//             foreach (var ps in particleSystems)
//             {
//                 if (ps != null && ps.IsAlive()) // null 체크 추가
//                 {
//                     isAlive = true;
//                     break;
//                 }
//             }
//             yield return null;
//         }

//         // 모든 ParticleSystem이 종료된 후 이펙트 삭제
//         if (effect != null) // 삭제 전 객체 null 체크
//         {
//             Destroy(effect);
//         }
//     }
// }