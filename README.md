# RPS Knights 개발 환경

구축일: 2026-09-08

## 시작

- `에디터_열기.command`를 더블클릭하면 Godot에서 이 프로젝트를 연다.
- Godot에서 F5를 누르면 기본 장면을 실행한다. Mac 키보드 설정에 따라 Fn+F5를 사용한다.
- `시험_실행.command`는 에디터 없이 기본 장면을 실행한다.
- 설치된 Blender는 응용 프로그램 폴더에서 실행한다.

현재 `prototype/playable-loop` 브랜치에는 기본 도형으로 만든 로컬 봇 대전 프로토타입이 구현되어 있다. 손 모양 선택, 가위바위보 주먹 흔들기와 동시 공개, 짧은 패널티, 3D 전투, 라운드 결과와 3승 경기까지 이어진다.

웹 게임 전환을 위한 Unity 6 프로젝트는 [`unity`](unity/) 폴더에 분리되어 있다. 현재 Unity 프로토타입은 손 버튼 선택, 양쪽 주먹 흔들기, 상대 무작위 선택 동시 공개까지 구현한다.

## 프로토타입 조작

- 가위·바위·보 선택: 화면의 손 버튼 또는 숫자 1·2·3
- 이동: W/A/S/D
- 기본 공격: 마우스 왼쪽 버튼
- 근접 가드: 마우스 오른쪽 버튼
- 회피: Space
- 직업 스킬: Q, 라운드당 최대 3회
- 바위 기사 총알 막기: E, 정면 총알 한 발에만 적용

가위를 선택할 때 총 변형을 켤 수 있다. 성공 확률은 20%이고 성공·실패 모두 스킬 1회를 소비한다. 총 기사는 탄약 3발을 가진다. 현재 캐릭터 모델과 효과음은 임시 도형이며 온라인 대전은 포함하지 않는다.

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

기능 작업은 별도 브랜치에서 진행하고 검증 후 main에 반영한다. 코드·설정·기획 문서를 함께 기록한다. 빌드 출력·캐시·환경 변수 파일·인증 키는 업로드하지 않는다. 원격 저장소 연결과 최초 업로드가 완료되어야 GitHub 백업이 생성된다.

## 공식 자료

- [Godot](https://godotengine.org/download/macos/)
- [Godot 라이선스](https://godotengine.org/license/)
- [Blender](https://www.blender.org/download/)
