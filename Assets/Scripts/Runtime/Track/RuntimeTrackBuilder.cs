using UnityEngine;
using MarbleRace.Data;

namespace MarbleRace.Runtime.Track
{
    public static class RuntimeTrackBuilder
    {
        private static TrackType _currentType;

        public static GameObject BuildTrack(TrackType type, PhysicsMaterial trackPhysMat)
        {
            _currentType = type;

            var track = new GameObject("Track");

            // Unique color theme per track type
            Color floorColor, wallColor;
            switch (type)
            {
                case TrackType.Zigzag:
                    floorColor = new Color(0.25f, 0.12f, 0.12f); // deep red
                    wallColor = new Color(0.12f, 0.05f, 0.05f);
                    break;
                case TrackType.Spiral:
                    floorColor = new Color(0.12f, 0.2f, 0.28f); // ocean blue
                    wallColor = new Color(0.05f, 0.08f, 0.15f);
                    break;
                case TrackType.Funnel:
                    floorColor = new Color(0.2f, 0.15f, 0.28f); // purple
                    wallColor = new Color(0.08f, 0.05f, 0.14f);
                    break;
                case TrackType.MultiPath:
                    floorColor = new Color(0.12f, 0.22f, 0.12f); // forest green
                    wallColor = new Color(0.05f, 0.1f, 0.05f);
                    break;
                case TrackType.Serpentine:
                    floorColor = new Color(0.28f, 0.2f, 0.1f); // golden brown
                    wallColor = new Color(0.14f, 0.1f, 0.05f);
                    break;
                case TrackType.Racetrack:
                    floorColor = new Color(0.18f, 0.18f, 0.18f); // asphalt dark gray
                    wallColor = new Color(0.1f, 0.1f, 0.1f);
                    break;
                default: // Downhill
                    floorColor = new Color(0.15f, 0.18f, 0.3f); // original blue-gray
                    wallColor = new Color(0.08f, 0.08f, 0.12f);
                    break;
            }
            var floorMat = MakeMat("FloorMat", floorColor, 0.4f, 0.7f);
            var wallMat = MakeMat("WallMat", wallColor, 0.6f, 0.85f);

            // More segments for tighter curves, prevents gaps between blocks
            int segmentCount = (type == TrackType.Zigzag || type == TrackType.Spiral || type == TrackType.Serpentine) ? 100
                : type == TrackType.Racetrack ? 160 : 80;
            float trackWidth = type == TrackType.Racetrack ? 8f : 5f;
            float wallHeight = 3.5f;

            Vector3[] points = new Vector3[segmentCount + 1];
            for (int i = 0; i <= segmentCount; i++)
            {
                float t = (float)i / segmentCount;
                points[i] = CurvePoint(t);
            }

            // Neon accent color per track type for light strips
            Color neonColor;
            switch (type)
            {
                case TrackType.Zigzag: neonColor = new Color(1f, 0.3f, 0.2f); break;
                case TrackType.Spiral: neonColor = new Color(0.2f, 0.6f, 1f); break;
                case TrackType.Funnel: neonColor = new Color(0.7f, 0.3f, 1f); break;
                case TrackType.MultiPath: neonColor = new Color(0.2f, 1f, 0.4f); break;
                case TrackType.Serpentine: neonColor = new Color(1f, 0.8f, 0.2f); break;
                case TrackType.Racetrack: neonColor = new Color(1f, 0.1f, 0.1f); break; // racing red
                default: neonColor = new Color(0.3f, 0.7f, 1f); break;
            }
            var neonMat = MakeEmissiveMat("NeonMat", neonColor, neonColor, 3f, 0f, 0.95f);

            for (int i = 0; i < segmentCount; i++)
            {
                Vector3 start = points[i];
                Vector3 end = points[i + 1];
                Vector3 center = (start + end) / 2f;
                Vector3 direction = (end - start).normalized;
                // Generous overlap to eliminate gaps on curved sections
                float length = Vector3.Distance(start, end) + 0.5f;

                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                var floor = MakeCube(track, $"Floor_{i}", center, new Vector3(trackWidth, 0.5f, length), floorMat, trackPhysMat);
                floor.transform.rotation = rotation;

                Vector3 leftOffset = rotation * new Vector3(-(trackWidth / 2f + 0.25f), wallHeight / 2f, 0f);
                var wallL = MakeCube(track, $"WallL_{i}", center + leftOffset, new Vector3(0.5f, wallHeight, length), wallMat, trackPhysMat);
                wallL.transform.rotation = rotation;

                Vector3 rightOffset = rotation * new Vector3((trackWidth / 2f + 0.25f), wallHeight / 2f, 0f);
                var wallR = MakeCube(track, $"WallR_{i}", center + rightOffset, new Vector3(0.5f, wallHeight, length), wallMat, trackPhysMat);
                wallR.transform.rotation = rotation;

                // Neon light strips along wall base every 5 segments
                if (i % 5 == 0)
                {
                    Vector3 neonLeftPos = rotation * new Vector3(-(trackWidth / 2f), 0.05f, 0f) + center;
                    var neonL = MakeCube(track, $"NeonL_{i}", neonLeftPos, new Vector3(0.15f, 0.15f, length), neonMat, null);
                    neonL.transform.rotation = rotation;

                    Vector3 neonRightPos = rotation * new Vector3((trackWidth / 2f), 0.05f, 0f) + center;
                    var neonR = MakeCube(track, $"NeonR_{i}", neonRightPos, new Vector3(0.15f, 0.15f, length), neonMat, null);
                    neonR.transform.rotation = rotation;

                    // Disable colliders on neon strips (decoration only)
                    Object.Destroy(neonL.GetComponent<BoxCollider>());
                    Object.Destroy(neonR.GetComponent<BoxCollider>());
                }
            }

            // Obstacles
            var obstacleMat = MakeEmissiveMat("ObstacleMat", new Color(0.9f, 0.15f, 0.1f), new Color(1f, 0.2f, 0.05f), 1.5f, 0.7f, 0.3f);
            var spinnerMat = MakeEmissiveMat("SpinnerMat", new Color(1f, 0.6f, 0f), new Color(1f, 0.5f, 0f), 2f, 0.8f, 0.5f);

            // Bumper at 1/3
            Vector3 obs1Pos = CurvePoint(0.33f) + new Vector3(0.8f, 0.6f, 0f);
            var bumper1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bumper1.name = "Bumper_1";
            bumper1.transform.parent = track.transform;
            bumper1.transform.position = obs1Pos;
            bumper1.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            bumper1.GetComponent<Renderer>().material = obstacleMat;
            if (trackPhysMat != null) bumper1.GetComponent<Collider>().material = trackPhysMat;
            bumper1.AddComponent<TrackHazard>().Initialize(HazardType.Bumper, 3f, 0f);

            // Spinner at 2/3
            Vector3 obs2Pos = CurvePoint(0.66f) + new Vector3(0f, 0.5f, 0f);
            var spinnerPivot = new GameObject("Spinner_1");
            spinnerPivot.transform.parent = track.transform;
            spinnerPivot.transform.position = obs2Pos;
            var spinnerRb = spinnerPivot.AddComponent<Rigidbody>();
            spinnerRb.isKinematic = true;
            var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arm.name = "SpinnerArm";
            arm.transform.parent = spinnerPivot.transform;
            arm.transform.localPosition = Vector3.zero;
            arm.transform.localScale = new Vector3(5f, 0.6f, 0.6f);
            arm.GetComponent<Renderer>().material = spinnerMat;
            spinnerPivot.AddComponent<TrackHazard>().Initialize(HazardType.Spinner, 4f, 120f);

            // Two staggered posts
            Vector3 obs3a = CurvePoint(0.45f) + new Vector3(-1.2f, 0.6f, 0f);
            var postA = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postA.name = "Post_A";
            postA.transform.parent = track.transform;
            postA.transform.position = obs3a;
            postA.transform.localScale = new Vector3(1f, 1.2f, 1f);
            postA.GetComponent<Renderer>().material = obstacleMat;
            if (trackPhysMat != null) postA.GetComponent<Collider>().material = trackPhysMat;
            postA.AddComponent<TrackHazard>().Initialize(HazardType.Bumper, 2.5f, 0f);

            Vector3 obs3b = CurvePoint(0.55f) + new Vector3(1.2f, 0.6f, 0f);
            var postB = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postB.name = "Post_B";
            postB.transform.parent = track.transform;
            postB.transform.position = obs3b;
            postB.transform.localScale = new Vector3(1f, 1.2f, 1f);
            postB.GetComponent<Renderer>().material = obstacleMat;
            if (trackPhysMat != null) postB.GetComponent<Collider>().material = trackPhysMat;
            postB.AddComponent<TrackHazard>().Initialize(HazardType.Bumper, 2.5f, 0f);

            // Boost pad at 80%
            var boostMat = MakeEmissiveMat("BoostMat", new Color(0.1f, 0.9f, 0.3f), new Color(0f, 1f, 0.3f), 2.5f, 0.2f, 0.9f);
            Vector3 boostPos = CurvePoint(0.8f) + new Vector3(0f, 0.3f, 0f);
            var boostPad = MakeCube(track, "BoostPad", boostPos, new Vector3(4f, 0.2f, 2f), boostMat, trackPhysMat);
            boostPad.GetComponent<BoxCollider>().isTrigger = true;
            boostPad.AddComponent<TrackHazard>().Initialize(HazardType.BoostPad, 6f, 0f);

            // Start line marking
            var startLineMat = MakeEmissiveMat("StartLineMat", Color.white, Color.white, 1f, 0f, 0.9f);
            Vector3 startLinePos = CurvePoint(0.005f);
            Vector3 startLineAhead = CurvePoint(0.015f);
            Quaternion startLineRot = Quaternion.LookRotation((startLineAhead - startLinePos).normalized, Vector3.up);
            var startLine = MakeCube(track, "StartLine", startLinePos + new Vector3(0, 0.26f, 0), new Vector3(trackWidth, 0.02f, 0.3f), startLineMat, null);
            startLine.transform.rotation = startLineRot;

            // Finish line marking (just before the bucket drop)
            var finishLineMat = MakeEmissiveMat("FinishLineMat", new Color(1f, 0.85f, 0f), new Color(1f, 0.9f, 0f), 2f, 0f, 0.9f);
            Vector3 finishLinePos = CurvePoint(0.96f);
            Vector3 finishLineAhead = CurvePoint(0.97f);
            Quaternion finishLineRot = Quaternion.LookRotation((finishLineAhead - finishLinePos).normalized, Vector3.up);
            var finishLine = MakeCube(track, "FinishLine", finishLinePos + new Vector3(0, 0.26f, 0), new Vector3(trackWidth, 0.02f, 0.4f), finishLineMat, null);
            finishLine.transform.rotation = finishLineRot;

            // Bucket
            Vector3 lastPoint = points[segmentCount];
            Vector3 bucketCenter = lastPoint + new Vector3(0f, -2f, 2f);
            MakeCube(track, "BucketFloor", bucketCenter, new Vector3(7, 0.5f, 5), floorMat, trackPhysMat);
            MakeCube(track, "BucketBack", bucketCenter + new Vector3(0, 2f, 2.75f), new Vector3(7, 4f, 0.5f), wallMat, trackPhysMat);
            MakeCube(track, "BucketFront", bucketCenter + new Vector3(0, 0.75f, -2.75f), new Vector3(7, 1f, 0.5f), wallMat, trackPhysMat);
            MakeCube(track, "BucketLeft", bucketCenter + new Vector3(-3.75f, 2f, 0), new Vector3(0.5f, 4f, 5), wallMat, trackPhysMat);
            MakeCube(track, "BucketRight", bucketCenter + new Vector3(3.75f, 2f, 0), new Vector3(0.5f, 4f, 5), wallMat, trackPhysMat);

            return track;
        }

        public static Vector3 CurvePoint(float t)
        {
            float trackLength = 80f;
            float x, y, z;

            switch (_currentType)
            {
                case TrackType.Serpentine:
                    // Snake-like S-curves with linear z progression
                    float serpLaps = 2f;
                    float serpAngle = t * serpLaps * Mathf.PI * 2f;
                    float serpRadiusX = 12f;
                    z = t * trackLength;
                    x = Mathf.Sin(serpAngle) * serpRadiusX;
                    y = Mathf.Lerp(6.0f, -6.0f, t);
                    break;

                case TrackType.Racetrack:
                    // True horse-racing oval: 1 full lap around a large ellipse, then exit ramp.
                    // Oval centered at (0, y, 40), semi-major=35 (Z), semi-minor=20 (X).
                    // Start at bottom (z=5), lap clockwise, exit straight to z=82.
                    float ovalPhase = 0.92f; // 92% of track is the oval loop
                    if (t <= ovalPhase)
                    {
                        float ovalT = t / ovalPhase;
                        float totalAngle = ovalT * Mathf.PI * 2f; // 1 full lap
                        float startAngle = -Mathf.PI / 2f; // start at bottom
                        float currentAngle = startAngle + totalAngle;
                        float rX = 20f; // wide oval
                        float rZ = 35f; // long oval
                        x = rX * Mathf.Cos(currentAngle);
                        z = 40f + rZ * Mathf.Sin(currentAngle);
                    }
                    else
                    {
                        // Exit ramp: straight from bottom of oval (0, y, 5) to z=82
                        float exitT = (t - ovalPhase) / (1f - ovalPhase);
                        // Smooth transition from oval end position to exit
                        x = Mathf.Lerp(0f, 0f, exitT);
                        z = Mathf.Lerp(5f, 82f, exitT);
                    }
                    y = Mathf.Lerp(3.0f, -5.0f, t);
                    break;

                case TrackType.Zigzag:
                    z = t * trackLength;
                    y = Mathf.Lerp(4.0f, -5.0f, t);
                    // Use sine wave instead of triangle wave for smoother curves
                    x = Mathf.Sin(t * Mathf.PI * 5f) * 2f;
                    break;

                case TrackType.Spiral:
                    z = t * trackLength;
                    y = Mathf.Lerp(3.0f, -5.0f, t);
                    float radius = 1.2f + Mathf.Sin(t * Mathf.PI) * 1.2f;
                    x = Mathf.Sin(t * Mathf.PI * 4f) * radius;
                    break;

                case TrackType.Funnel:
                    z = t * trackLength;
                    y = Mathf.Lerp(3.0f, -4.5f, t);
                    float amp = 2f * (1f - Mathf.Abs(t - 0.5f) * 2f);
                    x = Mathf.Sin(t * Mathf.PI * 3f) * Mathf.Max(amp, 0.5f);
                    break;

                default: // Downhill + MultiPath
                    z = t * trackLength;
                    y = Mathf.Lerp(3.0f, -4.5f, t);
                    x = Mathf.Sin(t * Mathf.PI * 3f) * 1.8f;
                    break;
            }

            return new Vector3(x, y, z);
        }

        public static float GetStartSurfaceY()
        {
            // Dynamic based on current track type
            return CurvePoint(0f).y + 0.75f;
        }

        public static string GetTrackName(TrackType type)
        {
            switch (type)
            {
                case TrackType.Zigzag: return "Zigzag Canyon";
                case TrackType.Spiral: return "Spiral Mountain";
                case TrackType.Funnel: return "The Funnel";
                case TrackType.MultiPath: return "Split Path";
                case TrackType.Serpentine: return "Serpentine";
                case TrackType.Racetrack: return "The Racetrack";
                default: return "Downhill Rush";
            }
        }

        private static GameObject MakeCube(GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat, PhysicsMaterial physMat)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.parent = parent.transform;
            obj.transform.position = pos;
            obj.transform.localScale = scale;

            if (physMat != null)
            {
                var col = obj.GetComponent<BoxCollider>();
                if (col != null) col.material = physMat;
            }

            if (mat != null)
                obj.GetComponent<Renderer>().material = mat;

            return obj;
        }

        private static Material MakeMat(string name, Color color, float metallic = 0.3f, float smoothness = 0.6f)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var mat = new Material(shader);
            mat.name = name;
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        private static Material MakeEmissiveMat(string name, Color color, Color emissionColor, float intensity = 2f, float metallic = 0.5f, float smoothness = 0.8f)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var mat = new Material(shader);
            mat.name = name;
            mat.color = color;
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionColor * intensity);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return mat;
        }
    }
}
