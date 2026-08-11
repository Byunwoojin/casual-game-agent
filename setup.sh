#!/bin/bash
# 프로젝트 루트에서 실행:  bash setup.sh
set -e

# 1) git 저장소가 없으면 초기화
if [ ! -d ".git" ]; then
  git init
  echo "git 저장소를 초기화했습니다."
else
  echo "이미 git 저장소입니다. init은 건너뜁니다."
fi

# 2) .gitignore, .gitmessage 복사 (이미 있으면 덮어쓰지 않고 확인)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [ ! -f ".gitignore" ]; then
  cp "$SCRIPT_DIR/.gitignore" .
  echo ".gitignore를 추가했습니다."
else
  echo ".gitignore가 이미 있어 건너뜁니다. 필요하면 내용을 수동 병합하세요."
fi

cp "$SCRIPT_DIR/.gitmessage" .
cp "$SCRIPT_DIR/COMMIT_CONVENTION.md" .

# 3) 이 저장소에서만 커밋 템플릿 적용 (전역 설정 아님)
git config --local commit.template .gitmessage

echo ""
echo "설정 완료. 'git commit' (메시지 옵션 없이) 실행 시 템플릿이 자동으로 열립니다."
echo "규칙 문서: COMMIT_CONVENTION.md"
