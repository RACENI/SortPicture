# 📸 사진 정리 프로그램 (SortPicture)

여러 사진 메타데이터(EXIF)를 기반으로 사진을 자동 분류·정리하는 **C# 콘솔 프로그램**입니다.

- 촬영 시간(EXIF) 또는 GPS(EXIF) 정보를 기준으로 분류합니다.
- 분류 결과는 **프로그램 실행 폴더 하위**에 생성됩니다.
- 기본 동작은 **복사(Copy)**이며, 완료 후 **원본 삭제 여부를 선택**할 수 있습니다.

---

## ✨ 주요 기능

- 📅 촬영 시간(EXIF) 기반 사진 정리
- 🌍 GPS(EXIF) 기반 사진 분류
- 🧭 GPS 분류 시 사용자 지정 “기준 지역명” 선택(콘솔에서 선택)
- 🔐 Google API Key 입력 및 `config.json` 저장
- 🧱 정리 기준 확장을 위한 `ISortManager` 구조 사용

---

## ✅ 요구 사항

- Windows 환경 권장
- .NET Framework **4.7.2**
- NuGet 패키지(프로젝트에 포함됨)
  - MetadataExtractor
  - Newtonsoft.Json
  - XmpCore
- GPS 기반 정리 사용 시 인터넷 연결 필요

---

## ▶ 실행 방식 및 폴더 규칙

1) 프로그램 실행 파일이 있는 폴더에 `working` 폴더를 생성합니다.  
2) 정리할 사진 파일을 `working` 폴더에 넣습니다.  
3) 프로그램을 실행하고 정리 모드를 선택합니다.

예시:
```text
프로그램실행경로
├─ picturemetadata.exe (또는 빌드된 실행 파일)
├─ picturemetadata.sln
├─ working
│  ├─ IMG_0001.jpg
│  ├─ IMG_0002.jpg
│  └─ IMG_0003.jpg
```

---

## 🧭 실행 방법(모드 선택)

프로그램 실행 후 콘솔에서 아래 값을 입력합니다.

- `1` : 날짜(EXIF) 기반 정리
- `2` : GPS(EXIF) 기반 정리
- `-1` : 종료

정리가 끝난 뒤, 원본 파일 처리 옵션을 선택합니다.

- `1` 입력 시: `working` 폴더의 원본 파일을 삭제합니다.
- 엔터/그 외 입력: 원본 파일은 유지됩니다.

※ 분류 과정에서의 파일 저장은 **복사(Copy)**로 수행되며,  
마지막 단계에서 원본 삭제를 선택하면 결과적으로 “이동한 것처럼” 동작합니다.

---

## 📁 정리 결과 생성 위치

- 정리 결과 폴더는 **프로그램 실행 디렉터리 하위**에 생성됩니다.
- 폴더명은 정리 기준값(날짜 또는 선택한 지역명)으로 생성됩니다.
- 동일 파일명이 있을 경우 덮어쓰기(`true`)로 복사됩니다.

### 날짜 기반 정리 결과 예시
```text
프로그램실행경로
├─ 2024-08-15
│  ├─ IMG_0001.jpg
│  └─ IMG_0002.jpg
├─ 2024-08-16
│  └─ IMG_0003.jpg
└─ working
   ├─ IMG_0001.jpg
   ├─ IMG_0002.jpg
   └─ IMG_0003.jpg
```

---

## 🌍 GPS 기능 사용 시 주의 사항 (Google API)

- GPS 기반 정리를 사용하려면 **Google Geocoding API**가 활성화된 API Key가 필요합니다.
- 호출 URL 예시(코드 기준):
  - `https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&key={apiKey}&language=ko`
- Google API는 무료 할당량이 존재할 수 있으나, 사용량/정책에 따라 과금이 발생할 수 있습니다.
- API Key는 프로그램 실행 폴더의 `config.json`에 저장됩니다.

⚠️ 보안 주의
- `config.json`에는 개인 API Key가 들어갈 수 있으므로 공개 저장소 커밋을 피하십시오.
- 필요 시 `.gitignore`에 `config.json`을 추가하는 것을 권장합니다.

🔗 Google API 발급 방법  
👉 https://saracenletter.tistory.com/332

---

## 📂 프로젝트 구조

```text
SortPicture-master
├─ picturemetadata.sln
├─ picturemetadata
│  ├─ Program.cs                  # 콘솔 진입점 / 모드 선택
│  ├─ FileManager.cs              # 폴더 생성 및 파일 복사 / config.json 저장
│  ├─ AppSettings.cs              # 설정 모델(googleApiKey)
│  ├─ SortManagers
│  │  ├─ ISortManager.cs          # 정리 기준 인터페이스
│  │  ├─ DateManager.cs           # 날짜(EXIF) 기반 정리
│  │  └─ GpsManager.cs            # GPS(EXIF) 기반 정리 + Geocoding 호출
│  └─ packages.config             # NuGet 패키지 정의
└─ README.md
```

---

## 🛠 개발 히스토리

### 2024/08/12
#### <수정 예정 사항>
- 위치정보가 잘 담긴 사진이면 해당 프로그램을 수정해서 좀 더 유연하게 활용할 수 있을 듯합니다.(시간될 때 업데이트 예정)
- 구조도 수정해야 될 부분이 보여서, 함께 수정할 예정입니다.

### 2024/08/13
#### <수정 사항>
- 프로그램 구조 개선(다양한 메타데이터로 정리할 수 있게끔 코드 확장 관점에서 개선함)
- GPS 기반 사진정리 기틀 마련

### 2024/08/15
#### <수정 사항>
- GPS 기반 사진정리 추가
  - 사용자가 직접 장소를 선택해주어야 함
  - 사용자가 선택한 장소가 중복이 되면 좁은 행정단위로 우선 분류
- 구글 API 값 저장 기능 추가
  - [구글 API 발급 방법(클릭 시 설명 페이지로 이동)](https://saracenletter.tistory.com/332)

---

## 🚀 향후 계획

- GPS 정보가 없는 사진에 대한 보완 분류 전략 추가
- 사용자 정의 규칙 기반 자동 분류 기능
- 설정 저장 방식 개선 (보안 및 유지보수 관점)
- 코드 전반 구조 정비 및 리팩토링

---
 
## 🗓 제작 정보

- 최초 제작일: 2020/02/18
- 지속 개선 및 리팩토링 진행 중
