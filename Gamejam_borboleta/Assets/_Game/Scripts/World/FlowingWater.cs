using System.Collections.Generic;
using UnityEngine;

namespace ButterflyStep
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class FlowingWater : MonoBehaviour
    {
        [SerializeField] private Sprite source;
        [SerializeField] private float width = 1f;
        [SerializeField] private float height = 5f;
        [Tooltip("Velocidade da queda, em unidades por segundo.")]
        [SerializeField] private float speed = 4f;
        [SerializeField] private Color leftColor = new Color(0.78f, 0.88f, 1f, 0.95f);
        [SerializeField] private Color rightColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private int sortingOrder = 5;

        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<Color> colors = new List<Color>();
        private readonly List<int> triangles = new List<int>();
        private Mesh mesh;

        public void Setup(Sprite sprite, float w, float h, float flowSpeed, int order)
        {
            source = sprite;
            width = w;
            height = h;
            speed = flowSpeed;
            sortingOrder = order;
        }

        private void OnEnable()
        {
            if (mesh == null) mesh = new Mesh { name = "FlowingWater", hideFlags = HideFlags.DontSave };
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            Rebuild(0f);
        }

        private void OnDisable()
        {
            if (mesh == null) return;
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
            mesh = null;
        }

        private void Update()
        {
            if (!Application.isPlaying || TimeStasis.Active) return;
            Rebuild(Time.time * speed);
        }

        private void Rebuild(float offset)
        {
            if (mesh == null || source == null) return;
            var tex = source.texture;
            var r = source.textureRect;
            float uMin = r.xMin / tex.width, uMax = r.xMax / tex.width;
            float vMin = r.yMin / tex.height, vMax = r.yMax / tex.height;
            float tile = source.bounds.size.y * (width / Mathf.Max(0.01f, source.bounds.size.x));
            float phase = Mathf.Repeat(offset, tile);
            float half = width * 0.5f;

            vertices.Clear();
            uvs.Clear();
            colors.Clear();
            triangles.Clear();
            float d0 = 0f;
            float next = phase > 0.0001f ? phase : tile;
            float p0 = phase > 0.0001f ? tile - phase : 0f;
            while (d0 < height - 0.0001f)
            {
                float d1 = Mathf.Min(height, next);
                float p1 = p0 + (d1 - d0);
                float v0 = vMax - p0 / tile * (vMax - vMin);
                float v1 = vMax - p1 / tile * (vMax - vMin);
                int i = vertices.Count;
                vertices.Add(new Vector3(-half, -d0, 0f));
                vertices.Add(new Vector3(half, -d0, 0f));
                vertices.Add(new Vector3(-half, -d1, 0f));
                vertices.Add(new Vector3(half, -d1, 0f));
                uvs.Add(new Vector2(uMin, v0));
                uvs.Add(new Vector2(uMax, v0));
                uvs.Add(new Vector2(uMin, v1));
                uvs.Add(new Vector2(uMax, v1));
                colors.Add(leftColor);
                colors.Add(rightColor);
                colors.Add(leftColor);
                colors.Add(rightColor);
                triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
                triangles.Add(i + 1); triangles.Add(i + 3); triangles.Add(i + 2);
                d0 = d1;
                next = d1 + tile;
                p0 = 0f;
            }
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }
    }
}
