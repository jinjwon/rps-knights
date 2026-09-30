# RPS Knights

가위바위보의 짧은 심리전과 3D 기사 결투를 연결하는 게임 프로토타입입니다. 손을 공개한 뒤 승패에 따른 조건을 가지고 전투에 들어가며, 로컬 봇을 상대로 전투 흐름을 실험합니다.

## 플레이 가능한 버전

| 브랜치 | 구현 내용 | 실행 방법 |
|---|---|---|
| [prototype/unity-web](https://github.com/jinjwon/rps-knights/tree/prototype/unity-web) | Unity 6 · 무작위 가위바위보 · 3D 손 흔들기와 동시 공개 · 제한 시간 45초의 기본 결투 | [Unity 프로젝트 안내](https://github.com/jinjwon/rps-knights/blob/prototype/unity-web/unity/README.md) |
| [prototype/playable-loop](https://github.com/jinjwon/rps-knights/tree/prototype/playable-loop) | Godot · 손 선택 · 직업별 전투와 스킬 · 3승 경기 | 해당 브랜치의 project.godot를 Godot에서 열고 F5 |
| main | 초기 개발 환경과 기획 문서 | 전체 플레이는 위 프로토타입 브랜치 사용 |

Unity 버전은 가위바위보 패자에게 4초의 이동속도 감소를 적용합니다. Godot 버전의 특수 스킬·총 변형·총알 막기는 Unity 버전에 아직 옮기지 않았습니다. 두 버전 모두 임시 기사 모델을 사용하는 개발 단계이며 온라인 대전은 구현하지 않았습니다.

## 개발 방향

한 번의 가위바위보 결과가 결투에서 어떤 선택을 만드는지 검증하는 것이 목적입니다. 현재 구현과 향후 기획을 구분하며, 기획 문서의 시험 수치는 확정 규칙이 아닙니다.

## 에셋과 권리

Unity의 가위·바위·보 손 모델은 Kat Deak / FUZE Technologies의 3D Hands pack을 사용합니다. [출처와 사용 조건](https://github.com/jinjwon/rps-knights/blob/prototype/unity-web/unity/Assets/Resources/Hands/LICENSE.md)을 함께 보관합니다. 엔진과 외부 에셋의 권리는 각 권리자에게 있으며, 저장소 공개만으로 모든 파일에 자유로운 재배포 라이선스가 부여되지는 않습니다.

---

## 개발 환경 기록

구축일: 2026-09-08

## 시작

- `에디터_열기.command`를 더블클릭하면 Godot에서 이 프로젝트를 연다.
- Godot에서 F5를 누르면 기본 장면을 실행한다. Mac 키보드 설정에 따라 Fn+F5를 사용한다.
- `시험_실행.command`는 에디터 없이 기본 장면을 실행한다.
- 설치된 Blender는 응용 프로그램 폴더에서 실행한다.

현재 장면은 바닥·조명·카메라·두 캐릭터 도형으로 구성된 환경 확인용이다. 이동 입력 이름과 키는 등록했지만 카드 선택이나 전투 로직은 아직 구현하지 않았다.

## 설치 구성

| 항목 | 버전·위치 |
|---|---|
| Godot 일반판 | 4.7.2 stable, `/Applications/Godot.app` |
| 내보내기 템플릿 | 4.7.2.stable, 사용자 Library의 Godot/export_templates |
| Blender | 5.2.1 LTS, `/Applications/Blender.app` |
| Git | 기존 2.50.1, 저장소 기본 브랜치 main |
| 언어·렌더러 | GDScript, Compatibility |

Godot는 공식 앱 서명 및 macOS 공증 검증을 통과했다. Blender 설치 파일은 공식 SHA-256과 일치했고 앱 서명을 확인했다. 설치 과정에서 보안 기능을 해제하지 않았다.

## 확인 결과

- Godot 에디터의 프로젝트 가져오기 완료.
- 기본 장면의 Apple M5 OpenGL 그래픽 실행 및 정상 종료 확인.
- Blender 백그라운드 실행과 버전 출력 확인.
- macOS 디버그 내보내기 성공. 결과는 `builds/RPSKnights.zip`.
- Apple Silicon 내보내기에 필요한 ETC2/ASTC 가져오기 옵션 활성화.

내보낸 파일은 로컬 개발 검증용이며 정식 배포용 공증은 하지 않았다.

## 파일 관리

`project.godot`는 프로젝트 설정, `scenes/main.tscn`은 기본 장면, `export_presets.cfg`는 macOS 내보내기 설정이다. `.godot/` 캐시와 `builds/` 출력은 Git에서 제외한다. 외부 서비스 연결이나 API 키는 필요하지 않다. Codex에서 이 `game` 폴더를 프로젝트로 열어 작업할 수 있다.

기획 기준은 `docs/게임_콘셉트와_개발가이드_v0.1.md`이며, 문서의 시험 수치는 아직 확정한 게임 규칙이 아니다.

## GitHub 관리 방식

기능 작업은 별도 브랜치에서 진행하고 검증 후 main에 반영한다. 코드·설정·기획 문서를 함께 기록한다. 빌드 출력·캐시·환경 변수 파일·인증 키는 업로드하지 않는다. 원격 저장소에 코드와 문서를 보관하며, 플레이 가능한 구현은 위 브랜치별 안내를 따른다.

## 공식 자료

- [Godot](https://godotengine.org/download/macos/)
- [Godot 라이선스](https://godotengine.org/license/)
- [Blender](https://www.blender.org/download/)
