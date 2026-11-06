using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Enemies;
using EndlessCrowdRunner.Gates;
using EndlessCrowdRunner.Weapons;
using EndlessCrowdRunner.Pickups;
using EndlessCrowdRunner.Environment;
using EndlessCrowdRunner.Environment.Obstacles;
using EndlessCrowdRunner.UI;

namespace EndlessCrowdRunner.Visuals
{
    /// <summary>
    /// Master procedural factory creating 3D low-poly stickmen, weapons, gates,
    /// obstacles, road chunks, and GPU-instanced materials at runtime or editor time.
    /// Guarantees the game runs out-of-the-box with zero missing assets!
    /// </summary>
    public class VisualAssetFactory : MonoBehaviour
    {
        public static VisualAssetFactory Instance { get; private set; }

        private static Material _playerMat;
        private static Material _enemyMat;
        private static Material _gatePositiveMat;
        private static Material _gateNegativeMat;
        private static Material _gateConsumedMat;
        private static Material _gateFramePosMat;
        private static Material _gateFrameNegMat;
        private static Material _roadMat;
        private static Material _curbMat;
        private static Material _playerBulletMat;
        private static Material _enemyBulletMat;
        private static Material _goldPickupMat;
        private static Material _obstacleMat;
        private static Material _bossMat;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        #region Material Generators (GPU Instanced)

        private static Shader FindStandardOrUrpShader()
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Mobile/Diffuse");
            if (s == null) s = Shader.Find("Diffuse");
            return s;
        }

        public static Material CreateInstancedMaterial(Color color, float smoothness = 0.4f)
        {
            Material mat = new Material(FindStandardOrUrpShader());
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            mat.enableInstancing = true;
            return mat;
        }

        public static Material GetPlayerMaterial()
        {
            if (_playerMat == null) _playerMat = CreateInstancedMaterial(new Color(0.12f, 0.48f, 0.96f), 0.5f);
            return _playerMat;
        }

        public static Material GetEnemyMaterial()
        {
            if (_enemyMat == null) _enemyMat = CreateInstancedMaterial(new Color(0.92f, 0.18f, 0.18f), 0.4f);
            return _enemyMat;
        }

        public static Material GetGateMaterial(bool positive)
        {
            if (positive)
            {
                if (_gatePositiveMat == null) _gatePositiveMat = CreateInstancedMaterial(new Color(0.15f, 0.88f, 0.35f, 0.85f), 0.7f);
                return _gatePositiveMat;
            }
            if (_gateNegativeMat == null) _gateNegativeMat = CreateInstancedMaterial(new Color(0.95f, 0.22f, 0.22f, 0.85f), 0.7f);
            return _gateNegativeMat;
        }

        public static Material GetGateFrameMaterial(bool positive)
        {
            if (positive)
            {
                if (_gateFramePosMat == null) _gateFramePosMat = CreateInstancedMaterial(new Color(0.3f, 1.0f, 0.5f), 0.8f);
                return _gateFramePosMat;
            }
            if (_gateFrameNegMat == null) _gateFrameNegMat = CreateInstancedMaterial(new Color(1.0f, 0.2f, 0.2f), 0.8f);
            return _gateFrameNegMat;
        }

        public static Material GetGateConsumedMaterial()
        {
            if (_gateConsumedMat == null) _gateConsumedMat = CreateInstancedMaterial(new Color(0.4f, 0.4f, 0.4f, 0.4f), 0.1f);
            return _gateConsumedMat;
        }

        public static Material GetRoadMaterial()
        {
            if (_roadMat == null) _roadMat = CreateInstancedMaterial(new Color(0.18f, 0.19f, 0.22f), 0.2f);
            return _roadMat;
        }

        public static Material GetCurbMaterial()
        {
            if (_curbMat == null) _curbMat = CreateInstancedMaterial(new Color(0.85f, 0.85f, 0.88f), 0.3f);
            return _curbMat;
        }

        public static Material GetBulletMaterial(bool isPlayer)
        {
            if (isPlayer)
            {
                if (_playerBulletMat == null) _playerBulletMat = CreateInstancedMaterial(new Color(1.0f, 0.85f, 0.1f), 0.9f);
                return _playerBulletMat;
            }
            if (_enemyBulletMat == null) _enemyBulletMat = CreateInstancedMaterial(new Color(1.0f, 0.15f, 0.15f), 0.9f);
            return _enemyBulletMat;
        }

        public static Material GetGoldPickupMaterial()
        {
            if (_goldPickupMat == null) _goldPickupMat = CreateInstancedMaterial(new Color(1.0f, 0.8f, 0.05f), 0.9f);
            return _goldPickupMat;
        }

        public static Material GetObstacleMaterial()
        {
            if (_obstacleMat == null) _obstacleMat = CreateInstancedMaterial(new Color(0.7f, 0.2f, 0.2f), 0.5f);
            return _obstacleMat;
        }

        public static Material GetBossMaterial()
        {
            if (_bossMat == null) _bossMat = CreateInstancedMaterial(new Color(0.8f, 0.1f, 0.1f), 0.6f);
            return _bossMat;
        }

        #endregion

        #region Prefab Builders

        public static StickmanUnit CreatePlayerStickmanPrefab(Transform parent = null)
        {
            GameObject go = new GameObject("PlayerStickman");
            if (parent != null) go.transform.SetParent(parent);

            CapsuleCollider col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.65f, 0f);
            col.radius = 0.28f;
            col.height = 1.3f;
            go.layer = LayerMask.NameToLayer("PlayerCrowd");
            go.tag = "PlayerCrowd";

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform);
            visual.transform.localPosition = Vector3.zero;

            // Capsule body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visual.transform);
            body.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            body.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
            DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = GetPlayerMaterial();

            // Sphere head
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visual.transform);
            head.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            head.transform.localScale = Vector3.one * 0.35f;
            DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = GetPlayerMaterial();

            // Weapon mount point
            GameObject mount = new GameObject("WeaponMount");
            mount.transform.SetParent(visual.transform);
            mount.transform.localPosition = new Vector3(0.28f, 0.65f, 0.2f);

            var animator = go.AddComponent<StickmanAnimator>();
            animator.Initialize(visual.transform);

            var unit = go.AddComponent<StickmanUnit>();
            return unit;
        }

        public static EnemyUnit CreateEnemyStickmanPrefab(Transform parent = null)
        {
            GameObject go = new GameObject("EnemyStickman");
            if (parent != null) go.transform.SetParent(parent);

            CapsuleCollider col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.65f, 0f);
            col.radius = 0.28f;
            col.height = 1.3f;
            go.layer = LayerMask.NameToLayer("EnemyCrowd");
            go.tag = "EnemyCrowd";

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform);
            visual.transform.localPosition = Vector3.zero;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visual.transform);
            body.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            body.transform.localScale = new Vector3(0.35f, 0.45f, 0.35f);
            DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = GetEnemyMaterial();

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visual.transform);
            head.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            head.transform.localScale = Vector3.one * 0.35f;
            DestroyImmediate(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = GetEnemyMaterial();

            GameObject mount = new GameObject("WeaponMount");
            mount.transform.SetParent(visual.transform);
            mount.transform.localPosition = new Vector3(0.28f, 0.65f, 0.2f);

            var animator = go.AddComponent<StickmanAnimator>();
            animator.Initialize(visual.transform);

            var unit = go.AddComponent<EnemyUnit>();
            return unit;
        }

        public static Bullet CreateBulletPrefab(Transform parent = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Bullet";
            if (parent != null) go.transform.SetParent(parent);
            go.transform.localScale = new Vector3(0.22f, 0.22f, 0.55f);

            Collider col = go.GetComponent<Collider>();
            col.isTrigger = true;
            go.GetComponent<Renderer>().sharedMaterial = GetBulletMaterial(true);

            TrailRenderer tr = go.AddComponent<TrailRenderer>();
            tr.time = 0.12f;
            tr.startWidth = 0.15f;
            tr.endWidth = 0.02f;
            tr.material = GetBulletMaterial(true);

            Bullet bullet = go.AddComponent<Bullet>();
            return bullet;
        }

        public static ParticleBurst CreateDeathFxPrefab(Transform parent = null)
        {
            return CreateGenericParticlePrefab("DeathFx", Color.cyan, 20, 0.45f, parent);
        }

        public static ParticleBurst CreateSpawnFxPrefab(Transform parent = null)
        {
            return CreateGenericParticlePrefab("SpawnFx", Color.yellow, 15, 0.35f, parent);
        }

        public static ParticleBurst CreateHitFxPrefab(Transform parent = null)
        {
            return CreateGenericParticlePrefab("HitFx", Color.yellow, 8, 0.25f, parent);
        }

        private static ParticleBurst CreateGenericParticlePrefab(string name, Color color, int burstCount, float duration, Transform parent = null)
        {
            GameObject go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = duration;
            main.startLifetime = duration;
            main.startSpeed = 4.0f;
            main.startSize = 0.18f;
            main.startColor = color;
            main.gravityModifier = 1.5f;
            main.loop = false;
            main.playOnAwake = false;

            var emission = ps.emission;
            emission.enabled = true;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, burstCount) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            var pb = go.AddComponent<ParticleBurst>();
            return pb;
        }

        public static FloatingText CreateFloatingTextPrefab(Transform parent = null)
        {
            GameObject go = new GameObject("FloatingText");
            if (parent != null) go.transform.SetParent(parent);

            TextMesh tm = go.AddComponent<TextMesh>();
            tm.fontSize = 48;
            tm.characterSize = 0.1f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.white;

            FloatingText ft = go.AddComponent<FloatingText>();
            return ft;
        }

        public static RoadChunk CreateRoadChunkPrefab(Transform parent = null)
        {
            GameObject go = new GameObject("RoadChunk");
            if (parent != null) go.transform.SetParent(parent);

            float width = 11.0f;
            float length = 50.0f;

            // Road surface
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Surface";
            road.transform.SetParent(go.transform);
            road.transform.localPosition = new Vector3(0f, -0.1f, length * 0.5f);
            road.transform.localScale = new Vector3(width, 0.2f, length);
            road.GetComponent<Renderer>().sharedMaterial = GetRoadMaterial();

            // Left curb
            GameObject leftCurb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftCurb.name = "LeftCurb";
            leftCurb.transform.SetParent(go.transform);
            leftCurb.transform.localPosition = new Vector3(-width * 0.5f - 0.2f, 0.15f, length * 0.5f);
            leftCurb.transform.localScale = new Vector3(0.4f, 0.5f, length);
            leftCurb.GetComponent<Renderer>().sharedMaterial = GetCurbMaterial();

            // Right curb
            GameObject rightCurb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightCurb.name = "RightCurb";
            rightCurb.transform.SetParent(go.transform);
            rightCurb.transform.localPosition = new Vector3(width * 0.5f + 0.2f, 0.15f, length * 0.5f);
            rightCurb.transform.localScale = new Vector3(0.4f, 0.5f, length);
            rightCurb.GetComponent<Renderer>().sharedMaterial = GetCurbMaterial();

            // Dashed center stripes
            for (float z = 3.0f; z < length; z += 6.0f)
            {
                GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = "Stripe";
                stripe.transform.SetParent(go.transform);
                stripe.transform.localPosition = new Vector3(0f, 0.01f, z);
                stripe.transform.localScale = new Vector3(0.25f, 0.02f, 2.5f);
                DestroyImmediate(stripe.GetComponent<Collider>());
                stripe.GetComponent<Renderer>().sharedMaterial = GetCurbMaterial();
            }

            GameObject content = new GameObject("Content");
            content.transform.SetParent(go.transform);
            content.transform.localPosition = Vector3.zero;

            RoadChunk chunk = go.AddComponent<RoadChunk>();
            return chunk;
        }

        #endregion

        #region Game Object Instances

        public static Gate CreateGate(Transform parent, Vector3 localPos, float width)
        {
            GameObject go = new GameObject("Gate");
            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;

            // Semi-transparent panel
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Panel";
            panel.transform.SetParent(go.transform);
            panel.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            panel.transform.localScale = new Vector3(width, 3.2f, 0.2f);
            DestroyImmediate(panel.GetComponent<Collider>());

            // Glowing top arch/frame
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(go.transform);
            frame.transform.localPosition = new Vector3(0f, 3.25f, 0f);
            frame.transform.localScale = new Vector3(width + 0.3f, 0.3f, 0.3f);
            DestroyImmediate(frame.GetComponent<Collider>());

            // Text display
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(go.transform);
            textObj.transform.localPosition = new Vector3(0f, 1.8f, -0.15f);
            TextMesh tm = textObj.AddComponent<TextMesh>();
            tm.fontSize = 52;
            tm.characterSize = 0.12f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;

            Gate gate = go.AddComponent<Gate>();
            return gate;
        }

        public static GameObject CreateEnemyGroup(Vector3 worldPos, int count, bool shooters)
        {
            GameObject go = new GameObject("EnemyGroup");
            go.transform.position = worldPos;

            // Floating badge
            GameObject badge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            badge.name = "Badge";
            badge.transform.SetParent(go.transform);
            badge.transform.localPosition = new Vector3(0f, 2.8f, 0f);
            badge.transform.localScale = new Vector3(1.6f, 0.7f, 0.1f);
            DestroyImmediate(badge.GetComponent<Collider>());
            badge.GetComponent<Renderer>().sharedMaterial = GetEnemyMaterial();

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(badge.transform);
            textObj.transform.localPosition = new Vector3(0f, 0f, -0.6f);
            TextMesh tm = textObj.AddComponent<TextMesh>();
            tm.fontSize = 44;
            tm.characterSize = 0.14f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.white;

            EnemyGroup group = go.AddComponent<EnemyGroup>();
            group.Setup(count, shooters);
            return go;
        }

        public static GameObject CreateBoss(Vector3 worldPos, int hp)
        {
            GameObject go = new GameObject("Boss");
            go.transform.position = worldPos;

            // Giant low-poly stickman titan
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform);
            visual.transform.localScale = Vector3.one * 3.2f;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(visual.transform);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            body.transform.localScale = new Vector3(0.45f, 0.6f, 0.45f);
            body.GetComponent<Renderer>().sharedMaterial = GetBossMaterial();

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visual.transform);
            head.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            head.transform.localScale = Vector3.one * 0.45f;
            head.GetComponent<Renderer>().sharedMaterial = GetBossMaterial();

            BoxCollider col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 2.5f, 0f);
            col.size = new Vector3(2.5f, 5.0f, 2.0f);

            // Floating health bar
            GameObject barRoot = new GameObject("HealthBar");
            barRoot.transform.SetParent(go.transform);
            barRoot.transform.localPosition = new Vector3(0f, 5.5f, 0f);

            GameObject barBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barBg.name = "Bg";
            barBg.transform.SetParent(barRoot.transform);
            barBg.transform.localScale = new Vector3(4.2f, 0.4f, 0.1f);
            DestroyImmediate(barBg.GetComponent<Collider>());
            barBg.GetComponent<Renderer>().sharedMaterial = GetObstacleMaterial();

            GameObject barFill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barFill.name = "Fill";
            barFill.transform.SetParent(barRoot.transform);
            barFill.transform.localPosition = new Vector3(0f, 0f, -0.05f);
            barFill.transform.localScale = new Vector3(4.0f, 0.35f, 0.1f);
            DestroyImmediate(barFill.GetComponent<Collider>());
            barFill.GetComponent<Renderer>().sharedMaterial = GetGateMaterial(true);

            GameObject nameObj = new GameObject("BossName");
            nameObj.transform.SetParent(barRoot.transform);
            nameObj.transform.localPosition = new Vector3(0f, 0.5f, -0.1f);
            TextMesh nameTm = nameObj.AddComponent<TextMesh>();
            nameTm.text = "TITAN CRUSHER";
            nameTm.fontSize = 44;
            nameTm.characterSize = 0.12f;
            nameTm.alignment = TextAlignment.Center;
            nameTm.anchor = TextAnchor.MiddleCenter;
            nameTm.color = Color.yellow;

            GameObject hpObj = new GameObject("HpText");
            hpObj.transform.SetParent(barRoot.transform);
            hpObj.transform.localPosition = new Vector3(0f, 0f, -0.15f);
            TextMesh hpTm = hpObj.AddComponent<TextMesh>();
            hpTm.fontSize = 32;
            hpTm.characterSize = 0.1f;
            hpTm.alignment = TextAlignment.Center;
            hpTm.anchor = TextAnchor.MiddleCenter;

            BossController boss = go.AddComponent<BossController>();
            boss.Setup(hp);
            return go;
        }

        public static GameObject CreateSpinningBlade(Vector3 worldPos)
        {
            GameObject go = new GameObject("SpinningBlade");
            go.transform.position = worldPos;

            GameObject rotor = new GameObject("Rotor");
            rotor.transform.SetParent(go.transform);
            rotor.transform.localPosition = Vector3.zero;

            // Two-bladed propeller
            GameObject blade1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade1.name = "Blade1";
            blade1.transform.SetParent(rotor.transform);
            blade1.transform.localScale = new Vector3(2.4f, 0.1f, 0.4f);
            DestroyImmediate(blade1.GetComponent<Collider>());
            blade1.GetComponent<Renderer>().sharedMaterial = GetObstacleMaterial();

            GameObject blade2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade2.name = "Blade2";
            blade2.transform.SetParent(rotor.transform);
            blade2.transform.localScale = new Vector3(0.4f, 0.1f, 2.4f);
            DestroyImmediate(blade2.GetComponent<Collider>());
            blade2.GetComponent<Renderer>().sharedMaterial = GetObstacleMaterial();

            go.AddComponent<SpinningBlade>();
            return go;
        }

        public static GameObject CreateWallObstacle(Vector3 worldPos, float gapX, float gapWidth)
        {
            GameObject go = new GameObject("WallObstacle");
            go.transform.position = worldPos;

            float roadW = 11.0f;
            float halfRoad = roadW * 0.5f;
            float leftEdge = -halfRoad;
            float rightEdge = halfRoad;

            float gapMin = gapX - gapWidth * 0.5f;
            float gapMax = gapX + gapWidth * 0.5f;

            // Left wall segment
            float leftWidth = Mathf.Max(0.1f, gapMin - leftEdge);
            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "LeftWall";
            leftWall.transform.SetParent(go.transform);
            leftWall.transform.localPosition = new Vector3(leftEdge + leftWidth * 0.5f, 1.2f, 0f);
            leftWall.transform.localScale = new Vector3(leftWidth, 2.4f, 0.8f);
            leftWall.GetComponent<Renderer>().sharedMaterial = GetObstacleMaterial();

            // Right wall segment
            float rightWidth = Mathf.Max(0.1f, rightEdge - gapMax);
            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "RightWall";
            rightWall.transform.SetParent(go.transform);
            rightWall.transform.localPosition = new Vector3(gapMax + rightWidth * 0.5f, 1.2f, 0f);
            rightWall.transform.localScale = new Vector3(rightWidth, 2.4f, 0.8f);
            rightWall.GetComponent<Renderer>().sharedMaterial = GetObstacleMaterial();

            WallObstacle wo = go.AddComponent<WallObstacle>();
            wo.Setup(gapX, gapWidth);
            return go;
        }

        public static GameObject CreatePitObstacle(Vector3 worldPos, float width, float length)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PitObstacle";
            go.transform.position = worldPos;
            go.transform.localScale = new Vector3(width, 0.05f, length);
            go.GetComponent<Renderer>().sharedMaterial = GetObstacleMaterial();

            PitObstacle po = go.AddComponent<PitObstacle>();
            po.Setup(width, length);
            return go;
        }

        public static GameObject CreateWeaponPickup(Vector3 worldPos, WeaponType type)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "WeaponPickup_" + type;
            go.transform.position = worldPos;
            go.transform.localScale = Vector3.one * 0.75f;
            go.GetComponent<Renderer>().sharedMaterial = GetGoldPickupMaterial();

            GameObject label = new GameObject("Label");
            label.transform.SetParent(go.transform);
            label.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            TextMesh tm = label.AddComponent<TextMesh>();
            tm.text = type.ToString().ToUpper();
            tm.fontSize = 38;
            tm.characterSize = 0.1f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.yellow;

            WeaponPickup wp = go.AddComponent<WeaponPickup>();
            wp.Setup(type);
            return go;
        }

        public static GameObject CreateSquadPickup(Vector3 worldPos, PickupType type)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "SquadPickup_" + type;
            go.transform.position = worldPos;
            go.transform.localScale = new Vector3(0.8f, 0.35f, 0.8f);
            go.GetComponent<Renderer>().sharedMaterial = GetGoldPickupMaterial();

            GameObject label = new GameObject("Label");
            label.transform.SetParent(go.transform);
            label.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            TextMesh tm = label.AddComponent<TextMesh>();
            tm.text = (type == PickupType.Squad10) ? "+10 SQUAD" : "HEAVY GUNNER";
            tm.fontSize = 38;
            tm.characterSize = 0.1f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.yellow;

            SquadPickup sp = go.AddComponent<SquadPickup>();
            sp.Setup(type);
            return go;
        }

        public static GameObject CreateWeaponModel(WeaponType type, Transform mount)
        {
            GameObject gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gun.name = type.ToString();
            gun.transform.SetParent(mount);
            gun.transform.localPosition = Vector3.zero;
            gun.transform.localRotation = Quaternion.identity;
            DestroyImmediate(gun.GetComponent<Collider>());
            gun.GetComponent<Renderer>().sharedMaterial = GetCurbMaterial();

            switch (type)
            {
                case WeaponType.Pistol:
                    gun.transform.localScale = new Vector3(0.08f, 0.12f, 0.28f);
                    break;
                case WeaponType.SMG:
                    gun.transform.localScale = new Vector3(0.1f, 0.14f, 0.42f);
                    break;
                case WeaponType.Shotgun:
                    gun.transform.localScale = new Vector3(0.12f, 0.12f, 0.55f);
                    break;
                case WeaponType.AssaultRifle:
                    gun.transform.localScale = new Vector3(0.1f, 0.15f, 0.65f);
                    break;
                case WeaponType.Minigun:
                    gun.transform.localScale = new Vector3(0.2f, 0.2f, 0.75f);
                    gun.GetComponent<Renderer>().sharedMaterial = GetObstacleMaterial();
                    break;
            }

            return gun;
        }

        public static GameObject CreateEnemyGunModel(Transform mount)
        {
            GameObject gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gun.name = "EnemyRifle";
            gun.transform.SetParent(mount);
            gun.transform.localPosition = Vector3.zero;
            gun.transform.localScale = new Vector3(0.09f, 0.12f, 0.45f);
            DestroyImmediate(gun.GetComponent<Collider>());
            gun.GetComponent<Renderer>().sharedMaterial = GetEnemyMaterial();
            return gun;
        }

        public static GameObject CreateSparkEffect(Vector3 pos, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.4f;
            DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = CreateInstancedMaterial(color);
            Destroy(go, 0.25f);
            return go;
        }

        public static void CreateShockwaveRing(Vector3 pos)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Shockwave";
            ring.transform.position = pos;
            ring.transform.localScale = new Vector3(1f, 0.05f, 1f);
            DestroyImmediate(ring.GetComponent<Collider>());
            ring.GetComponent<Renderer>().sharedMaterial = GetGateMaterial(false);

            // Animate expand and fade
            var anim = ring.AddComponent<ShockwaveAnimator>();
            anim.Initialize(14f, 0.45f);
        }

        #endregion
    }

    public class ShockwaveAnimator : MonoBehaviour
    {
        private float _maxScale;
        private float _duration;
        private float _timer;

        public void Initialize(float maxScale, float duration)
        {
            _maxScale = maxScale;
            _duration = duration;
            _timer = 0f;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            float t = Mathf.Clamp01(_timer / _duration);
            float scale = Mathf.Lerp(1f, _maxScale, t);
            transform.localScale = new Vector3(scale, 0.05f, scale);

            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
