using System.Collections.Generic;
using UnityEngine;

namespace PuzzlePals.Puzzle.Jigsaw
{
    /// <summary>
    /// Outline polygon → Unity Mesh 변환.
    ///
    /// UV 매핑: outline 은 "조각 local 좌표" (원점 = 조각 피봇).
    /// 이미지 전체에서 이 조각이 차지하는 영역 정보(cellOriginInImage, imageSize) 를 받아 UV 산출.
    /// 탭 부분은 이웃 셀 영역까지 UV 샘플링됨 (원화 이미지에서 자연스러운 연결).
    ///
    /// UV.y 규약: Unity 기본 = (0, 0) 좌하단. 스프라이트 텍스처 가져올 때도 동일.
    /// </summary>
    public static class JigsawMeshBuilder
    {
        /// <param name="outline">조각 mesh local 좌표 outline (CCW, 폐곡선). 보통 피봇이 셀 중심이 되도록 사전 오프셋 적용.</param>
        /// <param name="pivotInImage">outline 의 원점(0,0) 이 이미지 내에서 위치하는 좌표. 셀 중심 피봇 + 이미지 좌하단 원점 규약이면 (c*cellW + cellW/2, r*cellH + cellH/2).</param>
        /// <param name="imageSize">이미지 전체 크기 (outline·pivot 단위와 동일 — 보통 world unit).</param>
        /// <returns>MeshFilter 에 바로 할당 가능한 Mesh (triangulated + UV)</returns>
        public static Mesh Build(IReadOnlyList<Vector2> outline, Vector2 pivotInImage, Vector2 imageSize)
        {
            var mesh = new Mesh();
            mesh.name = "JigsawPiece";

            int n = outline.Count;
            var verts  = new Vector3[n];
            var uvs    = new Vector2[n];
            var colors = new Color[n];

            for (int i = 0; i < n; i++)
            {
                Vector2 p = outline[i];
                verts[i] = new Vector3(p.x, p.y, 0f);

                // UV = (outline vertex + outline pivot in image) / imageSize → [0, 1]
                uvs[i] = new Vector2(
                    (p.x + pivotInImage.x) / imageSize.x,
                    (p.y + pivotInImage.y) / imageSize.y
                );

                // Sprites/Default shader 가 vertex color × texture 곱셈 — alpha=1 명시로 hairline 가설 차단
                colors[i] = Color.white;
            }

            int[] tris = EarClipTriangulator.Triangulate(outline);

            mesh.vertices  = verts;
            mesh.uv        = uvs;
            mesh.colors    = colors;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
