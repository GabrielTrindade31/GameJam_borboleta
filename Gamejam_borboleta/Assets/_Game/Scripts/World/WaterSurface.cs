using UnityEngine;

namespace ButterflyStep
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WaterSurface : MonoBehaviour
    {
        [SerializeField] private float width = 6f;
        [SerializeField] private float depth = 2f;
        [SerializeField] private float columnsPerUnit = 5f;
        [SerializeField] private Color topColor = new Color(0.45f, 0.78f, 1f, 0.72f);
        [SerializeField] private Color bottomColor = new Color(0.08f, 0.24f, 0.5f, 0.9f);
        [SerializeField] private Color foamColor = new Color(0.9f, 0.97f, 1f, 0.95f);
        [SerializeField] private float foamThickness = 0.09f;
        [Tooltip("Velocidade do brilho correndo na superfície (acompanha a correnteza).")]
        [SerializeField] private float flowSpeed;
        [SerializeField] private float waveHeight = 0.05f;
        [SerializeField] private float stiffness = 60f;
        [SerializeField] private float damping = 4f;
        [SerializeField] private float spread = 18f;
        [SerializeField] private int sortingOrder = 6;
        [Tooltip("Água congelada: superfície parada, sem ondas.")]
        [SerializeField] private bool frozen;

        private Mesh mesh;
        private Vector3[] vertices;
        private Color[] colors;
        private float[] heights;
        private float[] speeds;
        private float[] deltas;
        private int columns;

        public float Width => width;

        public void Setup(float w, float d, float flow, int order)
        {
            width = w;
            depth = d;
            if (d < 0.5f) foamThickness = Mathf.Max(0.02f, d * 0.22f);
            flowSpeed = flow;
            sortingOrder = order;
        }

        public void SetFrozen(bool value)
        {
            frozen = value;
            if (!value) return;
            waveHeight = 0f;
            flowSpeed = 0f;
            foamThickness = 0.02f;
        }

        public void SetColors(Color top, Color bottom)
        {
            topColor = top;
            bottomColor = bottom;
        }

        private void OnEnable()
        {
            if (mesh == null) Build();
        }

        private void Build()
        {
            columns = Mathf.Max(2, Mathf.CeilToInt(width * columnsPerUnit) + 1);
            heights = new float[columns];
            speeds = new float[columns];
            deltas = new float[columns];
            vertices = new Vector3[columns * 4];
            colors = new Color[columns * 4];
            var uvs = new Vector2[columns * 4];
            var tris = new int[(columns - 1) * 12];
            for (int i = 0; i < columns; i++)
            {
                float u = i / (float)(columns - 1);
                uvs[i * 4] = new Vector2(u, 1f);
                uvs[i * 4 + 1] = new Vector2(u, 0f);
                uvs[i * 4 + 2] = new Vector2(u, 1f);
                uvs[i * 4 + 3] = new Vector2(u, 1f);
                if (i == columns - 1) continue;
                int a = i * 4, b = (i + 1) * 4, t = i * 12;
                tris[t] = a; tris[t + 1] = b; tris[t + 2] = a + 1;
                tris[t + 3] = b; tris[t + 4] = b + 1; tris[t + 5] = a + 1;
                tris[t + 6] = a + 3; tris[t + 7] = b + 3; tris[t + 8] = a + 2;
                tris[t + 9] = b + 3; tris[t + 10] = b + 2; tris[t + 11] = a + 2;
            }
            mesh = new Mesh { name = "WaterSurface", hideFlags = HideFlags.DontSave };
            mesh.MarkDynamic();
            UpdateMesh(0f);
            mesh.uv = uvs;
            mesh.triangles = tris;
            GetComponent<MeshFilter>().sharedMesh = mesh;
            var r = GetComponent<MeshRenderer>();
            r.sortingOrder = sortingOrder;
        }

        public void Disturb(float worldX, float force)
        {
            if (heights == null || frozen) return;
            float local = worldX - transform.position.x + width * 0.5f;
            int center = Mathf.RoundToInt(local / width * (columns - 1));
            for (int o = -2; o <= 2; o++)
            {
                int i = center + o;
                if (i < 0 || i >= columns) continue;
                speeds[i] += force * 60f * (1f - Mathf.Abs(o) * 0.3f);
            }
        }

        private void Update()
        {
            if (mesh == null || frozen || !Application.isPlaying) return;
            float dt = Mathf.Min(Time.deltaTime, 0.033f);
            for (int i = 0; i < columns; i++)
            {
                speeds[i] += (-stiffness * heights[i] - damping * speeds[i]) * dt;
                heights[i] += speeds[i] * dt;
            }
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < columns; i++)
                {
                    float left = i > 0 ? heights[i - 1] - heights[i] : 0f;
                    float right = i < columns - 1 ? heights[i + 1] - heights[i] : 0f;
                    deltas[i] = (left + right) * spread * dt;
                }
                for (int i = 0; i < columns; i++) speeds[i] += deltas[i];
            }
            UpdateMesh(Time.time);
        }

        private void UpdateMesh(float time)
        {
            float half = width * 0.5f;
            for (int i = 0; i < columns; i++)
            {
                float x = -half + width * i / (columns - 1);
                float wx = x + transform.position.x;
                float ambient = Mathf.Sin(wx * 1.7f + time * 2.2f) * waveHeight + Mathf.Sin(wx * 0.6f - time * 1.3f) * waveHeight * 0.6f;
                float h = Mathf.Clamp(heights[i] + ambient, -0.6f, 0.6f);
                bool edge = i == 0 || i == columns - 1;
                if (edge) h *= 0.3f;
                float shimmer = 0.5f + 0.5f * Mathf.Sin((wx - time * flowSpeed) * 2.4f);
                vertices[i * 4] = new Vector3(x, h, 0f);
                vertices[i * 4 + 1] = new Vector3(x, -depth, 0f);
                vertices[i * 4 + 2] = new Vector3(x, h + foamThickness * 0.5f, 0f);
                vertices[i * 4 + 3] = new Vector3(x, h - foamThickness, 0f);
                var top = Color.Lerp(topColor, foamColor, flowSpeed != 0f ? shimmer * 0.18f : 0f);
                colors[i * 4] = top;
                colors[i * 4 + 1] = bottomColor;
                var foam = foamColor;
                foam.a *= Mathf.Lerp(0.55f, 1f, flowSpeed != 0f ? shimmer : 0.5f + 0.5f * Mathf.Sin(wx * 3f + time));
                colors[i * 4 + 2] = foam;
                colors[i * 4 + 3] = foam;
            }
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (mesh == null) return;
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.4f);
            Gizmos.DrawWireCube(transform.position + Vector3.down * depth * 0.5f, new Vector3(width, depth, 0f));
        }
    }
}
