using System.Collections.Generic;
using UnityEngine;

// Namespace without ".Debug": inside Gaze.Presentation.Debug, "Debug" would hide UnityEngine.Debug.
namespace Gaze.Presentation
{
    /// <summary>
    /// What the debug layers are drawn with: lines and flat shapes in the scene (not gizmos), so they show up
    /// in the Game view and in the recordings. Everything uses one unlit material coloured per vertex.
    /// </summary>
    public static class DebugDraw
    {
        private static Material material;

        /// <summary>Unlit, transparent, coloured by the vertices (and by _Color).</summary>
        public static Material Material =>
            material != null ? material : material = new Material(Shader.Find("Sprites/Default")) { name = "GazeDebug" };

        public static LineRenderer CreateLine(string name, Transform parent, Color colour, float width)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = Material;
            line.useWorldSpace = true;
            line.widthMultiplier = width;
            line.numCapVertices = 4;
            line.numCornerVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            SetColour(line, colour);
            return line;
        }

        public static void SetColour(LineRenderer line, Color colour)
        {
            line.startColor = colour;
            line.endColor = colour;
        }

        /// <summary>A closed circle around <paramref name="centre"/>, lying flat (or facing <paramref name="normal"/>).</summary>
        public static void Circle(LineRenderer line, Vector3 centre, float radius, Vector3 normal, int segments = 64)
        {
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            line.loop = true;
            line.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                line.SetPosition(i, centre + rotation * new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius);
            }
        }

        /// <summary>A flat object with its own mesh, for filled shapes (the attention fan).</summary>
        public static MeshFilter CreateShape(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = Material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            var filter = go.AddComponent<MeshFilter>();
            filter.sharedMesh = new Mesh { name = name };
            return filter;
        }

        /// <summary>
        /// A flat fan in the object's local XZ plane: from -halfAngle to +halfAngle around +Z, with that radius.
        /// </summary>
        public static void Fan(Mesh mesh, float halfAngle, float radius, Color colour, int segments = 48)
        {
            var vertices = new List<Vector3> { Vector3.zero };
            var colours = new List<Color> { colour };
            var triangles = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments) * Mathf.Deg2Rad;
                vertices.Add(new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)) * radius);
                colours.Add(colour);
                if (i > 0)
                {
                    // Both faces, so it can be seen from above and from below.
                    triangles.AddRange(new[] { 0, i, i + 1 });
                    triangles.AddRange(new[] { 0, i + 1, i });
                }
            }
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetColors(colours);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        /// <summary>A small marker ball without collider, coloured through _Color.</summary>
        public static Renderer CreateDot(string name, Transform parent, float size)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = name;
            Object.Destroy(dot.GetComponent<Collider>());
            dot.transform.SetParent(parent, false);
            dot.transform.localScale = Vector3.one * size;
            var renderer = dot.GetComponent<Renderer>();
            renderer.sharedMaterial = Material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return renderer;
        }

        public static void SetColour(Renderer renderer, Color colour, MaterialPropertyBlock block)
        {
            block.SetColor("_Color", colour);
            renderer.SetPropertyBlock(block);
        }
    }
}
