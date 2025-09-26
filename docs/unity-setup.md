# Unity 프로젝트 초기 설정 가이드

## 1. Unity Hub 설정

1. Unity Hub 최신 버전을 설치하거나 업데이트합니다.
2. **Installs** 탭에서 `Unity 2022.3 LTS`(예: 2022.3.18f1) 버전을 추가합니다.
   - `iOS Build Support`, `Android Build Support`(SDK & NDK Tools, OpenJDK 포함)를 함께 설치하세요.

## 2. 새 프로젝트 생성

1. Unity Hub의 **Projects** 탭에서 **New project**를 선택합니다.
2. 템플릿으로 **3D (URP 없이)** 또는 **Blank 3D**를 선택합니다. AR Foundation 기본 샘플은 3D 기반이 가장 호환성이 좋습니다.
3. 프로젝트 이름: `GreatestOfTheTrader`
4. 위치: 리포지토리 루트(`/Users/systic/Desktop/Greatest-of-the-Trader/GreatestOfTheTrader` 등)
5. **Create project**를 눌러 프로젝트를 생성합니다.

## 3. 패키지 관리자 구성

1. Unity 에디터에서 `Window > Package Manager`를 엽니다.
2. 상단 **Packages** 필터를 `Unity Registry`로 설정합니다.
3. 다음 패키지를 설치합니다.
   - `AR Foundation` (Version 5.x 권장, Unity 2022 LTS와 호환)
   - `ARKit XR Plugin`
   - `ARCore XR Plugin`
   - (선택) `XR Interaction Toolkit` – 향후 상호작용 확장용
4. 설치 후 `Edit > Project Settings > XR Plug-in Management`에서 Android/iOS 각각 `ARCore`, `ARKit` 플러그인을 활성화합니다.

## 4. 샘플 씬 구성

1. `Assets` 폴더에 `Scenes` 폴더를 만들고 `Main.unity` 씬을 생성합니다.
2. `Hierarchy`에서 `AR Session`, `AR Session Origin` 프리팹을 추가합니다(`GameObject > XR >` 메뉴).
3. `AR Session Origin` 하위에 `AR Camera`, `AR Plane Manager`, `AR Raycast Manager`, `AR Point Cloud Manager` 등 필수 컴포넌트를 붙여 AR 기본 구성을 완료합니다.

## 5. 플랫폼별 빌드 세팅

- `File > Build Settings`
  - **Android** 탭: `Add Open Scenes`, `Switch Platform`. `Player Settings`에서 최소 API Level(예: Android 10) 지정, 패키지 네임 설정.
  - **iOS** 탭: 동일하게 `Add Open Scenes`, `Switch Platform`. 빌드 후 Xcode 프로젝트 생성.

## 6. 버전 관리 설정

1. `Edit > Project Settings > Editor`에서 `Version Control`을 **Visible Meta Files**, `Asset Serialization`을 **Force Text**로 설정합니다.
2. `.gitignore`에 `Library/`, `Temp/`, `Logs/`, `UserSettings/` 등을 추가합니다.

## 7. 초기 커밋 전 점검

- 샘플 씬(`Main.unity`)이 기본 AR 플레이 준비 상태인지 확인
- `ProjectSettings/`, `Packages/` 폴더가 버전 관리에 포함됐는지 확인
- 간단히 `Build Settings`에서 Android/iOS 각각에서 `Add Open Scenes`된 상태 저장

> 현재 CLI 환경에서는 Unity 에디터 실행이 불가하므로, 위 절차를 개발 머신에서 진행한 뒤 생성된 `GoesangAR` 프로젝트 폴더를 리포지토리에 추가하면 됩니다.
