using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gaze.Infrastructure;
using Gaze.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gaze.Editor
{
    /// <summary>
    /// Builds the demo scenes from scratch, always the same (menu Gaze > Build scenes):
    /// - Rule: the character still and a cube coming closer and going away (the rule, step by step).
    /// - Stairs: the character climbs a staircase; on the wall, among repeated stones, an eye.
    /// - Paintings: the character walks past five identical paintings; only one matters.
    /// The eye and the paintings are our own designs, made of primitives.
    /// </summary>
    public static class SceneBuilder
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";
        private const string MaterialsFolder = "Assets/_Project/Materials/Environment";
        private const string CharacterPrefab = "Assets/_Project/Prefabs/Characters/LinkNino.prefab";

        // A child: the prefab (a capsule 2 tall) is scaled down.
        private const float CharacterScale = 0.8f;
        private const float EyeHeightInCapsule = 0.72f;   // KitPersonajes: eyes at y ≈ 0.72 (capsule units)

        [MenuItem("Gaze/Build scenes/All")]
        public static void BuildAll()
        {
            BuildRule();
            BuildStairs();
            BuildPaintings();
        }

        // ------------------------------------------------------------------ Rule

        [MenuItem("Gaze/Build scenes/Rule")]
        public static void BuildRule()
        {
            NewScene();
            Dungeon();
            Room(-8f, 8f, -6f, 10f, 5f);
            Brazier(new Vector3(-4.2f, 0f, 3.5f));
            Brazier(new Vector3(4.2f, 0f, 3.5f));
            Brazier(new Vector3(-4.2f, 0f, -3f));
            Brazier(new Vector3(4.2f, 0f, -3f));
            Torch(new Vector3(-3f, 2.4f, 9.95f), Vector3.back);
            Torch(new Vector3(3f, 2.4f, 9.95f), Vector3.back);

            Character link = BuildCharacter("Link", Vector3.zero, 0f);

            // The cube that comes in and out of the rule: far ahead -> inside the radius -> round the side,
            // past the neck -> behind, outside the attention angle. And back.
            // Violet: it must not blend in with the orange firelight.
            GameObject cube = Box("InterestingCube", null, Vector3.zero, Vector3.one * 0.5f, Mat("Cube", new Color(0.7f, 0.35f, 0.95f)));
            cube.AddComponent<InterestMarker>();
            Waypoints(cube, ending: PathWalker.Ending.PingPong, speed: 1f, faceDirection: false, startDelay: 1.5f, startsOnItsOwn: true,
                      new Vector3(-1.2f, 1.2f, 7f), new Vector3(2.2f, 1.2f, 1.4f), new Vector3(3f, 1.2f, -1.4f));

            // Scenery right next to the character, not marked: the head ignores it.
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "SceneryBall";
            ball.transform.position = new Vector3(-1.6f, 0.35f, 1.8f);
            ball.transform.localScale = Vector3.one * 0.7f;
            ball.GetComponent<Renderer>().sharedMaterial = Mat("Scenery", new Color(0.55f, 0.55f, 0.6f));
            ball.AddComponent<InterestMarker>().IsInteresting = false;

            Demo(link);
            Follow(link, yaw: -35f, distance: 6f, height: 3.4f);
            Save("Rule");
        }

        // ------------------------------------------------------------------ Stairs

        private const int Steps = 10;
        private const float Rise = 0.3f;
        private const float Run = 0.6f;
        private const float StairsTop = Steps * Rise;      // 3 m
        private const float StairsEnd = Steps * Run;       // z = 6

        [MenuItem("Gaze/Build scenes/Stairs")]
        public static void BuildStairs()
        {
            NewScene();
            Dungeon();
            Material stone = Mat("Stone", new Color(0.46f, 0.43f, 0.38f));
            Material step = Mat("Step", new Color(0.38f, 0.35f, 0.31f));
            Material panel = Mat("WallStone", new Color(0.42f, 0.39f, 0.34f));

            // A tall hall: the eye wall on the left, the stairs along it up to a platform that runs to the back wall.
            const float hallLeft = -1.4f, hallRight = 7f, hallFront = -9f, hallBack = 9.5f;
            Room(hallLeft, hallRight, hallFront, hallBack, 7.5f);
            var level = new GameObject("Level").transform;
            for (int i = 0; i < Steps; i++)
            {
                float top = (i + 1) * Rise;
                Box($"Step{i + 1:00}", level, new Vector3(0f, top / 2f, i * Run + Run / 2f), new Vector3(2.5f, top, Run), step);
            }
            float landingWidth = hallRight - hallLeft;
            Box("Landing", level, new Vector3((hallLeft + hallRight) / 2f, StairsTop / 2f, StairsEnd + 1.75f),
                new Vector3(landingWidth, StairsTop, 3.5f), stone);

            Brazier(new Vector3(2.4f, 0f, -5f));
            Brazier(new Vector3(2.4f, 0f, 1.5f));
            foreach (float x in new[] { -0.7f, 1.5f, 3.7f })
                Torch(new Vector3(x, StairsTop + 1.5f, hallBack - 0.05f), Vector3.back);

            // The wall full of repeated stones, and four identical little stone windows among them at head height.
            // Three are empty; the fourth has an eye sunk in the dark inside, hard to spot until the head looks at it.
            // The stones and the empty windows are points of interest, but not marked: scenery the head ignores.
            float[] windowZ = { -3f, 0.6f, 3f, 5.4f };
            const float eyeZ = 3f;
            var details = new GameObject("WallStones").transform;
            for (float z = -5.4f; z < 8.5f; z += 1.2f)
            {
                float floor = FloorHeight(z);
                for (int row = 0; row < 3; row++)
                {
                    float y = floor + 0.7f + row * 0.85f;
                    bool isWindowSpot = row == 1 && windowZ.Any(w => Mathf.Abs(z - w) < 0.6f);
                    if (isWindowSpot)
                        continue;
                    GameObject stoneBlock = Box($"Stone_{z:0.0}_{row}", details, new Vector3(-1.37f, y, z),
                                                new Vector3(0.08f, 0.6f, 0.9f), panel);
                    if (row == 1)
                        stoneBlock.AddComponent<InterestMarker>().IsInteresting = false;
                }
            }
            foreach (float z in windowZ)
                WallWindow(z == eyeZ ? "WallEye" : $"EmptyWindow_{z:0.0}", new Vector3(-1.4f, FloorHeight(z) + 1.55f, z),
                           hasEye: z == eyeZ);

            // On the landing, two identical paintings: the one on the right is the marked one.
            Painting("PaintingLeft", new Vector3(0.6f, StairsTop + 1.5f, hallBack - 0.02f), Vector3.back, isInteresting: false);
            Painting("PaintingRight", new Vector3(2.4f, StairsTop + 1.5f, hallBack - 0.02f), Vector3.back, isInteresting: true);

            Character link = BuildCharacter("Link", new Vector3(0f, 0f, -4.5f), 0f);
            Waypoints(link.Root.gameObject, ending: PathWalker.Ending.Stop, speed: 1.1f, faceDirection: true, startDelay: 1f, startsOnItsOwn: false,
                      new Vector3(0f, 0f, -4.5f), new Vector3(0f, 0f, -0.1f),
                      new Vector3(0f, StairsTop, StairsEnd + 0.1f), new Vector3(0f, StairsTop, StairsEnd + 1.6f));

            Demo(link);
            Follow(link, yaw: -30f, distance: 5.5f, height: 3.2f);
            Save("Stairs");
        }

        private static float FloorHeight(float z) => Mathf.Clamp(z, 0f, StairsEnd) / Run * Rise;

        // ------------------------------------------------------------------ Paintings

        [MenuItem("Gaze/Build scenes/Paintings")]
        public static void BuildPaintings()
        {
            NewScene();
            Dungeon();
            Room(-8f, 8f, -9f, 3f, 4.5f);
            Ceiling(-8f, 8f, -9f, 3f, 4.5f);
            Brazier(new Vector3(-7.2f, 0f, 2f));
            Brazier(new Vector3(7.2f, 0f, 2f));

            // Five identical paintings with a torch in each gap between them: only the one in the middle is marked.
            for (int i = 0; i < 5; i++)
                Painting($"Painting{i + 1}", new Vector3(-4f + i * 2f, 1.7f, 2.98f), Vector3.back, isInteresting: i == 2);
            foreach (float x in new[] { -3f, -1f, 1f, 3f })
                Torch(new Vector3(x, 2.6f, 2.95f), Vector3.back);

            Character link = BuildCharacter("Link", new Vector3(-6.5f, 0f, 0f), 90f);
            Waypoints(link.Root.gameObject, ending: PathWalker.Ending.Stop, speed: 1.1f, faceDirection: true, startDelay: 1f, startsOnItsOwn: false,
                      new Vector3(-6.5f, 0f, 0f), new Vector3(6.5f, 0f, 0f));

            Demo(link);
            Follow(link, yaw: 0f, distance: 7f, height: 3f);
            Save("Paintings");
        }

        // ------------------------------------------------------------------ Character and demo

        private sealed class Character
        {
            public Transform Root;
            public Transform Head;
            public Transform Feet;
            public Transform EyeLevel;
            public Transform Eyes;
            public GazeController Gaze;
            public GazePresenter Presenter;
        }

        /// <summary>
        /// The character: an empty root that walks (its origin is the feet), the KitPersonajes prefab inside,
        /// and the anchors the rule and the layers measure from.
        /// </summary>
        private static Character BuildCharacter(string name, Vector3 feet, float facing)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefab);
            if (prefab == null)
                throw new FileNotFoundException($"Bake the LinkNino outfit first (KitPersonajes): {CharacterPrefab}");

            var root = new GameObject(name).transform;
            root.SetPositionAndRotation(feet, Quaternion.Euler(0f, facing, 0f));

            var body = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            body.transform.localPosition = Vector3.up * CharacterScale;   // half of 2 m, scaled
            body.transform.localScale = Vector3.one * CharacterScale;

            var c = new Character { Root = root, Head = body.transform.Find("Head") };
            if (c.Head == null)
                throw new System.InvalidOperationException("The LinkNino prefab has no Head: bake it with Separate Head on.");

            c.Feet = Anchor("Feet", root, Vector3.zero);
            c.EyeLevel = Anchor("EyeLevel", root, Vector3.up * (CharacterScale + EyeHeightInCapsule * CharacterScale));
            c.Eyes = Anchor("Eyes", c.Head, new Vector3(0f, EyeHeightInCapsule - 0.5f, 0.3f));

            c.Presenter = root.gameObject.AddComponent<GazePresenter>();
            c.Gaze = root.gameObject.AddComponent<GazeController>();
            Set(c.Gaze, "body", root);
            Set(c.Gaze, "eyeLevel", c.EyeLevel);

            // You walk it: a CharacterController the size of the scaled capsule (it climbs the 0.3 m steps).
            // The capsule's own collider goes, or the controller would bump into it.
            Object.DestroyImmediate(body.GetComponent<Collider>());
            var controller = root.gameObject.AddComponent<CharacterController>();
            controller.height = 2f * CharacterScale;
            controller.radius = 0.5f * CharacterScale;
            controller.center = Vector3.up * CharacterScale;
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 50f;
            controller.skinWidth = 0.03f;
            var movement = root.gameObject.AddComponent<PlayerMovement>();
            Set(movement, "view", Camera.main.transform);
            return c;
        }

        /// <summary>The camera follows the character, from that side (yaw: 0 = behind it if it faces +Z).</summary>
        private static void Follow(Character c, float yaw, float distance, float height)
        {
            var follow = Camera.main.gameObject.AddComponent<FollowCamera>();
            Set(follow, "target", c.Root);
            var so = new SerializedObject(follow);
            so.FindProperty("yaw").floatValue = yaw;
            so.FindProperty("distance").floatValue = distance;
            so.FindProperty("height").floatValue = height;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Already in place in the editor, as it will start.
            Vector3 feet = c.Root.position;
            Camera.main.transform.position = feet + Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0f, -distance) + Vector3.up * height;
            Camera.main.transform.LookAt(feet + Vector3.up * 1.1f);
            Camera.main.fieldOfView = 50f;
        }

        /// <summary>The head view, the four debug layers (off) and the keys.</summary>
        private static void Demo(Character c)
        {
            var headView = c.Root.gameObject.AddComponent<HeadView>();
            Set(headView, "head", c.Head);

            var layersRoot = new GameObject("DebugLayers");
            var layers = layersRoot.AddComponent<DebugLayers>();

            var views = new List<GazeView> { headView };
            var entries = new List<(string, GameObject)>();

            GazeView Layer<T>(string label) where T : GazeView
            {
                var go = new GameObject(label);
                go.transform.SetParent(layersRoot.transform, false);
                T view = go.AddComponent<T>();
                entries.Add((label, go));
                views.Add(view);
                go.SetActive(false);
                return view;
            }

            Set(Layer<RadiusRing>("1 Radius"), "feet", c.Feet);
            Set(Layer<AttentionFan>("2 Angle and neck"), "eyeLevel", c.EyeLevel);
            Set(Layer<GazeLine>("3 Gaze line"), "eyes", c.Eyes);
            Layer<CandidateDots>("4 Candidates");

            var presenter = new SerializedObject(c.Presenter);
            SerializedProperty list = presenter.FindProperty("views");
            list.arraySize = views.Count;
            for (int i = 0; i < views.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = views[i];
            presenter.ApplyModifiedPropertiesWithoutUndo();

            var layersObject = new SerializedObject(layers);
            SerializedProperty layerList = layersObject.FindProperty("layers");
            layerList.arraySize = entries.Count;
            for (int i = 0; i < entries.Count; i++)
            {
                SerializedProperty entry = layerList.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("name").stringValue = entries[i].Item1;
                entry.FindPropertyRelative("target").objectReferenceValue = entries[i].Item2;
            }
            layersObject.ApplyModifiedPropertiesWithoutUndo();

            var keys = new GameObject("DemoKeys").AddComponent<DemoKeys>();
            Set(keys, "layers", layers);
        }

        // ------------------------------------------------------------------ Props (our own designs)

        /// <summary>
        /// A small stone window set into the left wall of the stairs (the wall plane is at <paramref name="centre"/>.x):
        /// four frame bars that stand out from the wall and a dark back, so what is inside is sunk in shadow and
        /// hard to see from the side. With <paramref name="hasEye"/> there is an eye on the back; without it, the window
        /// is empty and only a decoy (a point of interest that is not marked: the head ignores it).
        /// </summary>
        private static void WallWindow(string name, Vector3 centre, bool hasEye)
        {
            var root = new GameObject(name).transform;
            root.position = centre;
            Material frame = Mat("WindowFrame", new Color(0.36f, 0.33f, 0.29f));
            Material back = Mat("WindowBack", new Color(0.07f, 0.065f, 0.06f));

            // Outer size 0.9 x 0.9, bars 0.12 wide, standing 0.22 out of the wall.
            const float depth = 0.22f, outer = 0.9f, bar = 0.12f, inner = outer - 2f * bar;
            float x = centre.x + depth / 2f;
            float offset = (outer - bar) / 2f;
            Box("Top", root, new Vector3(x, centre.y + offset, centre.z), new Vector3(depth, bar, outer), frame);
            Box("Bottom", root, new Vector3(x, centre.y - offset, centre.z), new Vector3(depth, bar, outer), frame);
            Box("Near", root, new Vector3(x, centre.y, centre.z - offset), new Vector3(depth, inner, bar), frame);
            Box("Far", root, new Vector3(x, centre.y, centre.z + offset), new Vector3(depth, inner, bar), frame);
            Box("Back", root, new Vector3(centre.x + 0.04f, centre.y, centre.z), new Vector3(0.04f, inner, inner), back);
            foreach (Collider collider in root.GetComponentsInChildren<Collider>())
                Object.DestroyImmediate(collider);   // decoration: it must never get in the character's way

            var marker = root.gameObject.AddComponent<InterestMarker>();
            if (!hasEye)
            {
                marker.IsInteresting = false;
                return;
            }

            // The eye on the dark back, looking out into the room (+X). Small and dim so it does not give itself away.
            Transform pupil = Eye(root, new Vector3(centre.x + 0.07f, centre.y, centre.z), Vector3.right);
            Set(marker, "lookAt", pupil);
        }

        /// <summary>
        /// An eye: a stone socket, the white, the iris and the pupil, each a flat disc a little further out.
        /// <paramref name="facing"/> is where it looks. Returns the pupil (where the head aims).
        /// </summary>
        private static Transform Eye(Transform parent, Vector3 position, Vector3 facing)
        {
            var root = new GameObject("Eye").transform;
            root.SetParent(parent, false);
            root.SetPositionAndRotation(position, Quaternion.LookRotation(facing));

            Disc("Socket", root, 0.00f, 0.52f, Mat("EyeSocket", new Color(0.16f, 0.14f, 0.12f)));
            Disc("White", root, 0.02f, 0.38f, Mat("EyeWhite", new Color(0.72f, 0.68f, 0.58f)));
            Disc("Iris", root, 0.035f, 0.21f, Mat("EyeIris", new Color(0.62f, 0.2f, 0.12f)));
            return Disc("Pupil", root, 0.05f, 0.10f, Mat("EyePupil", new Color(0.03f, 0.025f, 0.025f)));
        }

        /// <summary>A flat cylinder facing the parent's +Z, pushed out by <paramref name="offset"/>.</summary>
        private static Transform Disc(string name, Transform parent, float offset, float diameter, Material material)
        {
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = name;
            Object.DestroyImmediate(disc.GetComponent<Collider>());
            disc.transform.SetParent(parent, false);
            disc.transform.localPosition = new Vector3(0f, 0f, offset);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);   // the cylinder's axis (Y) to +Z
            disc.transform.localScale = new Vector3(diameter, 0.02f, diameter);
            disc.GetComponent<Renderer>().sharedMaterial = material;
            return disc.transform;
        }

        /// <summary>A painting: a wooden frame and a simple landscape (sky, hills, sun). The same for all.</summary>
        private static void Painting(string name, Vector3 centre, Vector3 facing, bool isInteresting)
        {
            var root = new GameObject(name).transform;
            root.SetPositionAndRotation(centre, Quaternion.LookRotation(facing));
            Material frame = Mat("Frame", new Color(0.45f, 0.28f, 0.12f));
            const float w = 1.2f, h = 0.9f, t = 0.1f;

            Box("Top", root, new Vector3(0f, h / 2f, 0f), new Vector3(w + t, t, t), frame, local: true);
            Box("Bottom", root, new Vector3(0f, -h / 2f, 0f), new Vector3(w + t, t, t), frame, local: true);
            Box("Left", root, new Vector3(-w / 2f, 0f, 0f), new Vector3(t, h, t), frame, local: true);
            Box("Right", root, new Vector3(w / 2f, 0f, 0f), new Vector3(t, h, t), frame, local: true);
            Box("Sky", root, new Vector3(0f, h / 8f, 0.02f), new Vector3(w, h * 0.75f, 0.02f), Mat("PaintingSky", new Color(0.45f, 0.65f, 0.85f)), local: true);
            Box("Hills", root, new Vector3(0f, -h * 3f / 8f, 0.02f), new Vector3(w, h / 4f, 0.02f), Mat("PaintingHills", new Color(0.3f, 0.55f, 0.3f)), local: true);
            Box("Sun", root, new Vector3(w * 0.25f, h * 0.2f, 0.035f), new Vector3(0.18f, 0.18f, 0.02f), Mat("PaintingSun", new Color(1f, 0.85f, 0.35f)), local: true);

            root.gameObject.AddComponent<InterestMarker>().IsInteresting = isInteresting;
        }

        // ------------------------------------------------------------------ Helpers

        private static void NewScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        private static void Save(string name)
        {
            Directory.CreateDirectory(ScenesFolder);
            string path = $"{ScenesFolder}/{name}.unity";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);

            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.Where(s => s.path != path).ToList();
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.OrderBy(s => s.path).ToArray();
            Debug.Log($"Gaze: scene built: {path}");
        }

        // ------------------------------------------------------------------ The dungeon: room, light and fire

        /// <summary>
        /// A stone room: a floor of two-tone tiles with dark joints and four walls, no ceiling (the camera looks
        /// down from above). The walls are quads drawn only from the inside, so the camera can go behind one
        /// without it blocking the view; a box collider behind each face still stops the character.
        /// </summary>
        private static void Room(float xMin, float xMax, float zMin, float zMax, float height)
        {
            var room = new GameObject("Room").transform;
            Material tileA = Mat("FloorTileA", new Color(0.40f, 0.37f, 0.33f));
            Material tileB = Mat("FloorTileB", new Color(0.33f, 0.31f, 0.28f));
            Material joint = Mat("FloorJoint", new Color(0.10f, 0.09f, 0.09f));
            Material wall = Mat("Wall", new Color(0.33f, 0.30f, 0.27f));

            float width = xMax - xMin, depth = zMax - zMin;
            var centre = new Vector3((xMin + xMax) / 2f, 0f, (zMin + zMax) / 2f);

            // The floor you stand on (top at y = 0) and, over it, the tiles: 2 cm taller, without colliders,
            // so the character slides over them and the gaps between tiles show the dark base as joints.
            Box("Floor", room, new Vector3(centre.x, -0.1f, centre.z), new Vector3(width, 0.2f, depth), joint);
            const float tile = 2f, gap = 0.06f;
            int tilesX = Mathf.CeilToInt(width / tile), tilesZ = Mathf.CeilToInt(depth / tile);
            var tiles = new GameObject("Tiles").transform;
            tiles.SetParent(room, false);
            for (int i = 0; i < tilesX; i++)
            for (int j = 0; j < tilesZ; j++)
            {
                float x0 = xMin + i * tile, z0 = zMin + j * tile;
                float w = Mathf.Min(tile, xMax - x0), d = Mathf.Min(tile, zMax - z0);
                GameObject t = Box($"Tile_{i}_{j}", tiles, new Vector3(x0 + w / 2f, 0.01f, z0 + d / 2f),
                                   new Vector3(w - gap, 0.02f, d - gap), (i + j) % 2 == 0 ? tileA : tileB);
                Object.DestroyImmediate(t.GetComponent<Collider>());
            }

            Wall("WallLeft", room, new Vector3(xMin, height / 2f, centre.z), Vector3.left, depth, height, wall);
            Wall("WallRight", room, new Vector3(xMax, height / 2f, centre.z), Vector3.right, depth, height, wall);
            Wall("WallFront", room, new Vector3(centre.x, height / 2f, zMin), Vector3.back, width, height, wall);
            Wall("WallBack", room, new Vector3(centre.x, height / 2f, zMax), Vector3.forward, width, height, wall);
        }

        /// <summary>
        /// A dark ceiling for a room that is seen from low cameras (the Paintings scene, also filmed vertically): without it the
        /// top of the picture is a black void over the wall. It is drawn only from below, casts no shadow (it must not block the
        /// moonlight) and has no collider.
        /// </summary>
        private static void Ceiling(float xMin, float xMax, float zMin, float zMax, float height)
        {
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ceiling.name = "Ceiling";
            Object.DestroyImmediate(ceiling.GetComponent<Collider>());
            // Turned so its visible side (local -Z) looks down; local X is world X and local Y is world -Z.
            ceiling.transform.SetPositionAndRotation(new Vector3((xMin + xMax) / 2f, height, (zMin + zMax) / 2f),
                                                     Quaternion.Euler(-90f, 0f, 0f));
            ceiling.transform.localScale = new Vector3(xMax - xMin, zMax - zMin, 1f);
            var renderer = ceiling.GetComponent<Renderer>();
            renderer.sharedMaterial = Mat("Ceiling", new Color(0.2f, 0.18f, 0.17f));
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        /// <summary>A wall whose face looks to the inside of the room; <paramref name="outward"/> points away from it.</summary>
        private static void Wall(string name, Transform parent, Vector3 centre, Vector3 outward, float width, float height, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
            wall.name = name;
            Object.DestroyImmediate(wall.GetComponent<Collider>());
            wall.transform.SetParent(parent, false);
            // A Quad is seen from its local -Z side: pointing its +Z outwards makes the inside the visible one.
            wall.transform.SetPositionAndRotation(centre, Quaternion.LookRotation(outward));
            wall.transform.localScale = new Vector3(width, height, 1f);
            wall.GetComponent<Renderer>().sharedMaterial = material;

            var collider = wall.AddComponent<BoxCollider>();
            collider.size = new Vector3(1f, 1f, 0.4f);      // 40 cm thick, behind the face
            collider.center = new Vector3(0f, 0f, 0.2f);
        }

        /// <summary>
        /// Dark room mood: no sky, dim cold ambient light and moonlight, a little fog and a black background.
        /// The warm light comes from the torches and braziers.
        /// </summary>
        private static void Dungeon()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.24f, 0.23f, 0.28f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.fogColor = new Color(0.04f, 0.04f, 0.05f);

            Camera camera = Camera.main;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.02f, 0.03f);

            foreach (Light light in Object.FindObjectsByType<Light>())
            {
                if (light.type != LightType.Directional)
                    continue;
                light.color = new Color(0.65f, 0.72f, 1f);
                light.intensity = 0.55f;
                light.transform.rotation = Quaternion.Euler(55f, -35f, 0f);
            }
        }

        private static readonly Color FireColour = new Color(1f, 0.58f, 0.22f);

        /// <summary>A wall torch: a wooden stick, a flame and a warm light. <paramref name="inward"/> points into the room.</summary>
        private static void Torch(Vector3 position, Vector3 inward)
        {
            var root = new GameObject("Torch").transform;
            root.position = position;
            Material wood = Mat("TorchWood", new Color(0.25f, 0.15f, 0.08f));

            GameObject stick = Box("Stick", root, new Vector3(0f, 0f, 0f), new Vector3(0.1f, 0.5f, 0.1f), wood, local: true);
            stick.transform.position = position + inward * 0.08f;
            stick.transform.rotation = Quaternion.LookRotation(inward) * Quaternion.Euler(-15f, 0f, 0f);
            Object.DestroyImmediate(stick.GetComponent<Collider>());

            Flame(root, position + inward * 0.2f + Vector3.up * 0.32f, 0.2f);
            Glow(root, position + inward * 0.45f + Vector3.up * 0.4f, 4.5f, 11f);
        }

        /// <summary>A stone brazier on the floor with fire inside.</summary>
        private static void Brazier(Vector3 floorPosition)
        {
            var root = new GameObject("Brazier").transform;
            root.position = floorPosition;
            Material stone = Mat("BrazierStone", new Color(0.22f, 0.21f, 0.2f));

            Box("Pillar", root, new Vector3(0f, 0.45f, 0f), new Vector3(0.45f, 0.9f, 0.45f), stone, local: true);
            Box("Bowl", root, new Vector3(0f, 0.95f, 0f), new Vector3(0.8f, 0.14f, 0.8f), stone, local: true);
            Flame(root, floorPosition + Vector3.up * 1.2f, 0.42f);
            Glow(root, floorPosition + Vector3.up * 1.7f, 5f, 12f);
        }

        /// <summary>A small glowing ball, with no collider: the flame.</summary>
        private static void Flame(Transform parent, Vector3 position, float size)
        {
            GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flame.name = "Flame";
            Object.DestroyImmediate(flame.GetComponent<Collider>());
            flame.transform.SetParent(parent, false);
            flame.transform.position = position;
            flame.transform.localScale = new Vector3(size, size * 1.4f, size);
            flame.GetComponent<Renderer>().sharedMaterial = FlameMaterial();
        }

        private static Light Glow(Transform parent, Vector3 position, float intensity, float range)
        {
            var go = new GameObject("FireLight");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = FireColour;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }

        private static Material FlameMaterial()
        {
            Directory.CreateDirectory(MaterialsFolder);
            const string path = MaterialsFolder + "/Flame.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", FireColour);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", FireColour * 3f);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Transform Anchor(string name, Transform parent, Vector3 localPosition)
        {
            var anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = localPosition;
            return anchor;
        }

        private static GameObject Box(string name, Transform parent, Vector3 position, Vector3 size, Material material, bool local = false)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            if (parent != null)
                box.transform.SetParent(parent, false);
            if (local)
                box.transform.localPosition = position;
            else
                box.transform.position = position;
            if (local)
                box.transform.localRotation = Quaternion.identity;
            box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }

        /// <summary>
        /// A PathWalker on the object, with its waypoints as children of a "Path" object.
        /// startsOnItsOwn = false: it waits for key P (the character's path for the takes).
        /// </summary>
        private static void Waypoints(GameObject target, PathWalker.Ending ending, float speed, bool faceDirection,
                                      float startDelay, bool startsOnItsOwn, params Vector3[] points)
        {
            target.transform.position = points[0];
            var path = new GameObject($"{target.name}Path").transform;
            var transforms = new Transform[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                transforms[i] = new GameObject($"Waypoint{i}").transform;
                transforms[i].SetParent(path, false);
                transforms[i].position = points[i];
            }

            var walker = target.AddComponent<PathWalker>();
            var so = new SerializedObject(walker);
            SerializedProperty list = so.FindProperty("waypoints");
            list.arraySize = transforms.Length;
            for (int i = 0; i < transforms.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = transforms[i];
            so.FindProperty("speed").floatValue = speed;
            so.FindProperty("startDelay").floatValue = startDelay;
            so.FindProperty("ending").enumValueIndex = (int)ending;
            so.FindProperty("faceDirection").boolValue = faceDirection;
            so.ApplyModifiedPropertiesWithoutUndo();
            walker.enabled = startsOnItsOwn;
        }

        /// <summary>A URP Lit material of that colour, created once in Materials/Environment.</summary>
        private static Material Mat(string name, Color colour)
        {
            Directory.CreateDirectory(MaterialsFolder);
            string path = $"{MaterialsFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", colour);
            material.SetFloat("_Smoothness", 0.1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(field);
            if (property == null)
                throw new System.ArgumentException($"{target.GetType().Name} has no field '{field}'.");
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
