# ☃ About “Eternal Snowman”

- 생존을 위해 다른 눈사람들과 경쟁하여 최후의 1인으로 살아남아 녹지 않는 눈사람인 “Eternal Snowman”이 되기 위한 배틀 로얄 게임


---


# 📦 Package
- 설치 방법
    : Unity > Window > Package Manger > ‘+’ 버튼 >  add package from git URL…
    - 아래 링크 붙여넣기

- Native WebSockets (1.1.4 ver) [링크](https://github.com/endel/NativeWebSocket.git#upm)  
- NuGetForUnity (4.2.0 ver) [링크](https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity)


# 🚀 빌드
- 빌드
	- sh bulid.sh를 실행하거나, 유니티 에디터에서 build and run
	- 유니티 에디터에서 빌드하는 경우, 파일 경로를 server.js로 설정. 기존 Builds를 대체.
- 빌드 설정
	- WEbGLTemplate이 CustomTemplate으로 설정
		- index.html 파일 확인!
- 실행
	- server/server.js를 실행
    
---


# ⚙️ Code Convention

## **네이밍 규칙**

### **클래스 및 메서드 이름**

: PascalCase를 사용하여 각 단어의 첫 글자를 대문자로 표기

ex) `PlayerController`, `CalculateScore()`

### **로컬 변수 및 매개변수**

: camelCase를 사용하여 첫 단어는 소문자로, 이후 단어의 첫 글자는 대문자로 표기

ex) `playerHealth`, `totalScore`

### **프라이빗 필드**

: 접두사로 밑줄(_)을 붙이고 camelCase를 사용합니다.

ex) `_currentLevel`, `_isGameOver`

### **상수**

: 모든 문자를 대문자로 하고, 단어 사이에 밑줄(_)을 사용합니다.

ex) `MAX_HEALTH`, `DEFAULT_SPEED`


## **중괄호 사용**

- 모든 제어문(if, else, for, while 등)과 메서드 정의 시 중괄호를 사용하며, 중괄호는 새로운 줄에서 시작

```csharp
if (isGameOver)
{
		// 코드 블록
}
else
{
		// 코드 블록
}
```

## **공백 및 들여쓰기**

- 들여쓰기는 공백 4칸을 사용
- 연산자 주변과 쉼표 다음에는 한 칸의 공백을 추가

```csharp
int sum = a + b;

MyFunction(param1, param2);
```

## **접근 제한자 명시**

- 모든 필드와 메서드에는 명시적으로 접근 제한자를 지정

```csharp
public class Player
{
		private int _health;
		
		public void TakeDamage(int damage)
		{
				_health -= damage;
		}
}
```

## **파일 및 폴더 구조**

```
Assets/
├── Animations/        // 애니메이션 클립 및 컨트롤러
├── Audio/             // 오디오 파일
│   ├── Music/         // 배경 음악
│   └── SFX/           // 효과음
├── Materials/         // 머티리얼 및 셰이더
├── Models/            // 3D 모델
├── Prefabs/           // 프리팹
│   ├── Characters/    // 캐릭터 관련 프리팹
│   ├── Environment/   // 환경 요소 프리팹
│   └── UI/            // UI 요소 프리팹
├── Resources/         // 런타임에 로드할 리소스
├── Scenes/            // 씬 파일
├── Scripts/           // 스크립트 파일
│   ├── Gameplay/      // 게임 플레이 관련 스크립트
│   ├── Network/       // 네트워크 관련 스크립트
│   ├── UI/            // UI 관련 스크립트
│   └── Utilities/     // 유틸리티 및 헬퍼 스크립트
├── Sprites/           // 2D 스프라이트
│   ├── Characters/    // 캐릭터 스프라이트
│   ├── Environment/   // 환경 스프라이트
│   └── UI/            // UI 스프라이트
└── Plugins/           // 외부 플러그인 및 SDK
```

스크립트는 기능별로 폴더를 구분하여 관리합니다.

•	예시: Scripts/Player, Scripts/Enemies, Scripts/UI

•	각 클래스는 별도의 파일에 저장하며, 파일 이름은 클래스 이름과 동일하게 합니다.

## **주석 작성**

- 필요한 경우 코드의 의도를 설명하는 주석을 추가하되, 과도한 주석은 지양
- 공개 메서드나 클래스에는 XML 주석을 사용하여 문서화

```csharp
/// <summary>
/// 플레이어의 현재 점수를 반환합니다.
/// </summary>
/// <returns>현재 점수</returns>
public int GetScore()
{
    return _score;
}
```

## **기타 권장 사항**

- null 체크 시 null 병합 연산자(`??`)나 null 조건부 연산자(`?.`)를 활용
- 문자열 보간(string interpolation)을 사용하여 가독성을 높임
    - 문자열 내에 변수나 표현식의 값을 직접 삽입하여 가독성과 유지 보수성을 높이는 방법
    - C#에서는 문자열 앞에 $ 기호를 붙이고, 중괄호 {} 안에 변수나 표현식을 작성하여 구현

```csharp
string playerInfo = $"Name: {playerName}, Score: {playerScore}"
```

참고
https://unity.com/kr/how-to/naming-and-code-style-tips-c-scripting-unity

---

# 📌 **시스템 요구 사항**

## **WebGL**

| **브라우저를 실행하는 운영체제** | **Windows, macOS, Linux** |
| --- | --- |
| **하드웨어** | 워크스테이션 및 노트북 폼 팩터. |
| **추가 요구 사항** | 다음에 해당하는 Chrome, Firefox, Safari 또는 Edge(Chromium 기반) 버전:- WebGL 2.0 지원- HTML 5 스탠다드 준수- 64비트- WebAssembly 지원**참고**:WebGL 1.0은 지원 중단 예정입니다. |

## **브라우저별 최소 지원 버전**

1. **Chrome**
- 최소 버전: **57**
- WebGL 2.0 및 WebAssembly 지원이 포함된 첫 버전.
1. **Firefox**
- 최소 버전: **51**
- WebGL 2.0 및 WebAssembly 지원이 포함된 첫 버전.
1. **Safari**
- 최소 버전: **11**
- macOS High Sierra(10.13)+에서 WebGL 2.0 지원.
1. **Edge (Chromium 기반)**
- 최소 버전: **79**
- Chromium 기반으로 전환 후 WebGL 2.0 및 WebAssembly 지원.

**참고**

https://docs.unity3d.com/kr/2022.3/Manual/system-requirements.html
