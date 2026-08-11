#!/usr/bin/env bash
# Unity 프로젝트 배치모드 자동 검증 파이프라인.
# casual-game-agent-design.md 4번 문서 "6. 배치모드 자동 검증" 절차의 스크립트화.
#
# 사용법:
#   ./verify_project.sh <project_dir> [executeMethod ...]
#
# executeMethod 는 Namespace.Class.Method 전체 경로. 순서대로 실행됨
# (보통 Importer -> SceneSetupTool -> LocalizationSetupTool -> LocalizationStringTableTool 순).
# 하나도 안 주면 컴파일 확인 + EditMode 테스트만 실행.
#
# 예:
#   ./verify_project.sh ~/casual-game-agent/SlidingPuzzle \
#     SlidingPuzzle.EditorTools.SlidingPuzzleImporter.ImportLevels \
#     SlidingPuzzle.EditorTools.SceneSetupTool.SetupAllScenes \
#     SlidingPuzzle.EditorTools.LocalizationSetupTool.SetupLocalization \
#     SlidingPuzzle.EditorTools.LocalizationStringTableTool.PopulateStringTables
#
# 주의: 대상 프로젝트가 Unity 에디터로 열려있으면 프로젝트 락 충돌로 실패함 — 먼저 닫아야 함.
set -euo pipefail

UNITY_BIN="/Applications/Unity/Hub/Editor/6000.4.2f1/Unity.app/Contents/MacOS/Unity"

if [ "$#" -lt 1 ]; then
  echo "사용법: $0 <project_dir> [executeMethod ...]" >&2
  exit 1
fi

PROJ="$1"; shift
STEPS=("$@")

if [ ! -x "$UNITY_BIN" ]; then
  echo "Unity 실행 파일을 찾을 수 없음: $UNITY_BIN (버전 다르면 스크립트 상단 경로 수정)" >&2
  exit 1
fi
if [ ! -d "$PROJ" ]; then
  echo "프로젝트 경로 없음: $PROJ" >&2
  exit 1
fi

LOGDIR="$PROJ/Logs"
mkdir -p "$LOGDIR"

check_errors() {
  local logfile="$1" label="$2"
  if grep -qE "error CS" "$logfile"; then
    echo "!! $label 실패 — 컴파일 에러:"
    grep -E "error CS" "$logfile" | sort -u
    exit 1
  fi
}

echo "=== [1] 컴파일 확인 ==="
"$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJ" \
  -logFile "$LOGDIR/verify_compile.log"
check_errors "$LOGDIR/verify_compile.log" "컴파일"
echo "OK — 컴파일 통과"

step_num=2
for method in "${STEPS[@]}"; do
  safe_name=$(echo "$method" | tr '.' '_')
  logfile="$LOGDIR/verify_step_${safe_name}.log"
  echo "=== [${step_num}] executeMethod: $method ==="
  "$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJ" \
    -executeMethod "$method" -logFile "$logfile"
  check_errors "$logfile" "$method"
  echo "OK — $method"
  step_num=$((step_num + 1))
done

run_test_platform() {
  local platform="$1" label="$2" tag="$3"
  local resultsfile="$LOGDIR/verify_${tag}_results.xml"
  echo "=== [${step_num}] ${label} 테스트 ==="
  "$UNITY_BIN" -batchmode -nographics -projectPath "$PROJ" \
    -runTests -testPlatform "$platform" -testResults "$resultsfile" \
    -logFile "$LOGDIR/verify_tests_${tag}.log" || true

  if [ ! -f "$resultsfile" ]; then
    echo "!! ${label} 테스트 결과 파일 생성 안 됨 — $LOGDIR/verify_tests_${tag}.log 확인 필요" >&2
    exit 1
  fi

  python3 - "$resultsfile" "$label" <<'PYEOF'
import sys, xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
label = sys.argv[2]
total, passed, failed = root.get('total'), root.get('passed'), root.get('failed')
print(f"{label} 테스트 결과 — total={total} passed={passed} failed={failed}")
any_fail = False
for tc in root.iter('test-case'):
    if tc.get('result') != 'Passed':
        any_fail = True
        print("  FAIL:", tc.get('fullname'))
if any_fail:
    sys.exit(1)
PYEOF
  step_num=$((step_num + 1))
}

run_test_platform "EditMode" "EditMode" "editmode"
# PlayMode E2E — 씬 전환(SceneTransitionTests)/실제 탭-판정 흐름(GameplayFlowTests) 등.
# IPointerClickHandler 직접 호출 방식이라 디스플레이 없이도 batchmode 에서 동작함.
run_test_platform "PlayMode" "PlayMode" "playmode"

echo ""
echo "전부 통과. 남은 건 사람 확인(화면 레이아웃/오디오/실제 시각적 완성도) — Unity Hub 로 직접 열어서 플레이해볼 것."
