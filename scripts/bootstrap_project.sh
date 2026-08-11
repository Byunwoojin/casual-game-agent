#!/usr/bin/env bash
# 기존 완성 프로젝트를 새 장르 프로젝트로 복제 + 네임스페이스/asmdef/productName 일괄 치환.
# casual-game-agent-design.md 4번 문서 "2. 프로젝트 부트스트랩" 절차의 스크립트화.
#
# 사용법:
#   ./bootstrap_project.sh <source_project_dir> <dest_project_dir> <OldNamespace> <NewNamespace>
#
# 예:
#   ./bootstrap_project.sh ~/casual-game-agent/SlidingPuzzle ~/casual-game-agent/MatchThree SlidingPuzzle MatchThree
#
# 주의: 소스 프로젝트는 Unity 에디터가 열려있지 않은 상태여야 함(Library 등은 어차피 제외하고 복사).
set -euo pipefail

if [ "$#" -ne 4 ]; then
  echo "사용법: $0 <source_project_dir> <dest_project_dir> <OldNamespace> <NewNamespace>" >&2
  exit 1
fi

SRC="$1"; DEST="$2"; OLD_NS="$3"; NEW_NS="$4"

if [ ! -d "$SRC" ]; then
  echo "소스 프로젝트를 찾을 수 없음: $SRC" >&2
  exit 1
fi
if [ -e "$DEST" ]; then
  echo "대상 경로가 이미 존재함(덮어쓰지 않음): $DEST" >&2
  exit 1
fi

echo "=== [1/4] rsync 복사: $SRC -> $DEST ==="
mkdir -p "$DEST"
rsync -a \
  --exclude 'Library' --exclude 'Temp' --exclude 'Logs' --exclude 'UserSettings' --exclude 'obj' \
  --exclude '*.csproj' --exclude '*.sln' --exclude '*.slnx' \
  "$SRC/" "$DEST/"

echo "=== [2/4] 네임스페이스 치환: $OLD_NS -> $NEW_NS ==="
if [ -d "$DEST/Assets/Scripts" ]; then
  grep -rl "$OLD_NS" "$DEST/Assets/Scripts" "$DEST/Assets/Tests" 2>/dev/null | while read -r f; do
    sed -i '' "s/${OLD_NS}/${NEW_NS}/g" "$f"
  done
fi

echo "=== [3/4] asmdef 파일명 리네임 ==="
find "$DEST" -iname "*${OLD_NS}*.asmdef" -o -iname "*${OLD_NS}*.asmdef.meta" | while read -r f; do
  newf=$(echo "$f" | sed "s/${OLD_NS}/${NEW_NS}/g")
  mv "$f" "$newf"
done

echo "=== [4/4] productName 갱신 ==="
if [ -f "$DEST/ProjectSettings/ProjectSettings.asset" ]; then
  sed -i '' "s/productName: .*/productName: ${NEW_NS}/" "$DEST/ProjectSettings/ProjectSettings.asset"
fi

echo ""
echo "부트스트랩 완료: $DEST"
echo ""
echo "여기부터는 장르 전용 작업 (자동화 불가, Claude Code 가 직접 작성해야 함):"
echo "  1. 이전 장르 전용 파일 삭제 — 게임보드 컴포넌트, Importer, 소스 이미지 폴더,"
echo "     Assets/Resources/Levels/*.asset, Assets/Scenes/*.unity"
echo "     (Scenes 는 SceneSetupTool 로 재생성할 거면 통째로 지우는 게 안전 — 오래된 참조가 안 남음)"
echo "  2. LevelDataSO 서브타입 새로 작성 (Assets/Scripts/Data/LevelDataSO.cs 교체)"
echo "  3. 게임보드 컴포넌트 작성 — MonoBehaviour(렌더링/입력) + 순수 로직 클래스 분리 (유닛 테스트 위해 필수)"
echo "  4. Importer 작성 (Editor/*.cs) — 원본 소스를 Sprite 로 강제 재설정(textureType=Sprite)하는 스텝 잊지 말 것"
echo "  5. SceneSetupTool 에서 옛 게임보드 타입 참조를 새 타입으로 교체"
echo "  6. LocalizationStringTableTool 의 UI_ROWS 에 장르 전용 텍스트 추가 (공용 키는 그대로 재사용)"
echo "  7. verify_project.sh 로 배치모드 검증"
