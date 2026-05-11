using UnityEngine;
using MarbleRace.Data;
using MarbleRace.Runtime.Track;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class TrackGenerator
{
    private static PhysicsMaterial _trackPhysMat;
    private static TrackType _currentType;

    public static GameObject GenerateTrack(TrackType type, PhysicsMaterial trackPhysMat)
    {
        _trackPhysMat = trackPhysMat;
        _currentType = type;
        return BuildCurvedTrack();
    }

    // Sample a smooth parametric curve — shape varies by track type
    static Vector3 CurvePoint(float t)
    {
        float trackLength = 80f;
        float z = t * trackLength;
        float x, y;

        switch (_currentType)
        {
            case TrackType.Zigzag:
                // Sharp zigzag using triangle wave, steeper drop
                y = Mathf.Lerp(4.0f, -5.0f, t);
                float tri = Mathf.PingPong(t * 4f, 1f) * 2f - 1f;
                x = tri * 2.5f;
                break;

            case TrackType.Spiral:
                // Gentle spiral that winds outward then back
                y = Mathf.Lerp(3.0f, -5.0f, t);
                float radius = 1.5f + Mathf.Sin(t * Mathf.PI) * 1.5f;
                x = Mathf.Sin(t * Mathf.PI * 4f) * radius;
                break;

            case TrackType.Funnel:
                // Wide at start, narrows, then opens to bucket
                y = Mathf.Lerp(3.0f, -4.5f, t);
                float amp = 2.5f * (1f - Mathf.Abs(t - 0.5f) * 2f);
                x = Mathf.Sin(t * Mathf.PI * 3f) * Mathf.Max(amp, 0.5f);
                break;

            default: // Downhill + MultiPath use standard S-curve
                y = Mathf.Lerp(3.0f, -4.5f, t);
                x = Mathf.Sin(t * Mathf.PI * 3f) * 2f;
                break;
        }

        return new Vector3(x, y, z);
    }

    static GameObject BuildCurvedTrack()
    {
        var track = new GameObject("Track");

        var floorMat = MakeMat("FloorMat", new Color(0.15f, 0.18f, 0.3f), 0.4f, 0.7f);
        var wallMat = MakeMat("WallMat", new Color(0.08f, 0.08f, 0.12f), 0.6f, 0.85f);

        int segmentCount = 40;
        float trackWidth = 5f;
        float wallHeight = 3.5f;

        // Sample the curve at many points for smooth transitions
        Vector3[] points = new Vector3[segmentCount + 1];
        for (int i = 0; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            points[i] = CurvePoint(t);
        }

        // Build a short segment between each consecutive pair of points
        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 start = points[i];
            Vector3 end = points[i + 1];
            Vector3 center = (start + end) / 2f;
            Vector3 direction = (end - start).normalized;
            float length = Vector3.Distance(start, end) + 0.15f; // tiny overlap to seal gaps

            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            // Floor slab
            var floor = MakeCube(track, $"Floor_{i}", center, new Vector3(trackWidth, 0.5f, length), floorMat);
            floor.transform.rotation = rotation;

            // Left wall
            Vector3 leftOffset = rotation * new Vector3(-(trackWidth / 2f + 0.25f), wallHeight / 2f, 0f);
            var wallL = MakeCube(track, $"WallL_{i}", center + leftOffset, new Vector3(0.5f, wallHeight, length), wallMat);
            wallL.transform.rotation = rotation;

            // Right wall
            Vector3 rightOffset = rotation * new Vector3((trackWidth / 2f + 0.25f), wallHeight / 2f, 0f);
            var wallR = MakeCube(track, $"WallR_{i}", center + rightOffset, new Vector3(0.5f, wallHeight, length), wallMat);
            wallR.transform.rotation = rotation;
        }

        // OBSTACLES — emissive glow so they stand out
        var obstacleMat = MakeEmissiveMat("ObstacleMat", new Color(0.9f, 0.15f, 0.1f), new Color(1f, 0.2f, 0.05f), 1.5f, 0.7f, 0.3f);
        var spinnerMat = MakeEmissiveMat("SpinnerMat", new Color(1f, 0.6f, 0f), new Color(1f, 0.5f, 0f), 2f, 0.8f, 0.5f);

        // Obstacle 1: Round bumper post at 1/3, offset right
        Vector3 obs1Pos = CurvePoint(0.33f) + new Vector3(0.8f, 0.6f, 0f);
        var bumper1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        bumper1.name = "Bumper_1";
        bumper1.transform.parent = track.transform;
        bumper1.transform.position = obs1Pos;
        bumper1.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        bumper1.GetComponent<Renderer>().material = obstacleMat;
        if (_trackPhysMat != null) bumper1.GetComponent<Collider>().material = _trackPhysMat;
        var bumperHazard = bumper1.AddComponent<TrackHazard>();
        SetHazardProperties(bumperHazard, HazardType.Bumper, 3f, 0f);

        // Obstacle 2: Spinning bar at 2/3 — marble height, trigger collider so it knocks not blocks
        Vector3 obs2Pos = CurvePoint(0.66f) + new Vector3(0f, 0.5f, 0f);
        var spinnerPivot = new GameObject("Spinner_1");
        spinnerPivot.transform.parent = track.transform;
        spinnerPivot.transform.position = obs2Pos;
        // Kinematic Rigidbody on pivot — solid child collider physically pushes marbles
        var spinnerRb = spinnerPivot.AddComponent<Rigidbody>();
        spinnerRb.isKinematic = true;
        // The spinning arm — solid collider so it reliably hits marbles
        var arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "SpinnerArm";
        arm.transform.parent = spinnerPivot.transform;
        arm.transform.localPosition = Vector3.zero;
        arm.transform.localScale = new Vector3(5f, 0.6f, 0.6f);
        arm.GetComponent<Renderer>().material = spinnerMat;
        // No isTrigger — kinematic Rigidbody + solid collider pushes marbles physically
        var spinnerHazard = spinnerPivot.AddComponent<TrackHazard>();
        SetHazardProperties(spinnerHazard, HazardType.Spinner, 4f, 120f);

        // Obstacle 3: Two round bumper posts staggered apart so marbles deflect, not stuck
        Vector3 obs3a = CurvePoint(0.45f) + new Vector3(-1.2f, 0.6f, 0f);
        var postA = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        postA.name = "Post_A";
        postA.transform.parent = track.transform;
        postA.transform.position = obs3a;
        postA.transform.localScale = new Vector3(1f, 1.2f, 1f);
        postA.GetComponent<Renderer>().material = obstacleMat;
        if (_trackPhysMat != null) postA.GetComponent<Collider>().material = _trackPhysMat;
        postA.AddComponent<TrackHazard>();
        SetHazardProperties(postA.GetComponent<TrackHazard>(), HazardType.Bumper, 2.5f, 0f);

        Vector3 obs3b = CurvePoint(0.55f) + new Vector3(1.2f, 0.6f, 0f);
        var postB = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        postB.name = "Post_B";
        postB.transform.parent = track.transform;
        postB.transform.position = obs3b;
        postB.transform.localScale = new Vector3(1f, 1.2f, 1f);
        postB.GetComponent<Renderer>().material = obstacleMat;
        if (_trackPhysMat != null) postB.GetComponent<Collider>().material = _trackPhysMat;
        postB.AddComponent<TrackHazard>();
        SetHazardProperties(postB.GetComponent<TrackHazard>(), HazardType.Bumper, 2.5f, 0f);

        // Obstacle 4: Boost pad at 80% — green pad that launches marbles forward
        var boostMat = MakeEmissiveMat("BoostMat", new Color(0.1f, 0.9f, 0.3f), new Color(0f, 1f, 0.3f), 2.5f, 0.2f, 0.9f);
        Vector3 boostPos = CurvePoint(0.8f) + new Vector3(0f, 0.3f, 0f);
        var boostPad = MakeCube(track, "BoostPad", boostPos, new Vector3(4f, 0.2f, 2f), boostMat);
        boostPad.GetComponent<BoxCollider>().isTrigger = true;
        var boostHazard = boostPad.AddComponent<TrackHazard>();
        SetHazardProperties(boostHazard, HazardType.BoostPad, 6f, 0f);

        // BUCKET at the end to catch marbles — fully enclosed box
        Vector3 lastPoint = points[segmentCount];
        Vector3 bucketCenter = lastPoint + new Vector3(0f, -2f, 2f);
        MakeCube(track, "BucketFloor", bucketCenter, new Vector3(7, 0.5f, 5), floorMat);
        MakeCube(track, "BucketBack", bucketCenter + new Vector3(0, 2f, 2.75f), new Vector3(7, 4f, 0.5f), wallMat);
        MakeCube(track, "BucketFront", bucketCenter + new Vector3(0, 0.75f, -2.75f), new Vector3(7, 1f, 0.5f), wallMat);
        MakeCube(track, "BucketLeft", bucketCenter + new Vector3(-3.75f, 2f, 0), new Vector3(0.5f, 4f, 5), wallMat);
        MakeCube(track, "BucketRight", bucketCenter + new Vector3(3.75f, 2f, 0), new Vector3(0.5f, 4f, 5), wallMat);

        return track;
    }

    static void SetHazardProperties(TrackHazard hazard, HazardType type, float force, float spin)
    {
#if UNITY_EDITOR
        var so = new SerializedObject(hazard);
        so.FindProperty("hazardType").enumValueIndex = (int)type;
        so.FindProperty("forceStrength").floatValue = force;
        so.FindProperty("spinSpeed").floatValue = spin;
        so.ApplyModifiedProperties();
#endif
    }

    static GameObject MakeCube(GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.parent = parent.transform;
        obj.transform.position = pos;
        obj.transform.localScale = scale;

        if (_trackPhysMat != null)
        {
            var col = obj.GetComponent<BoxCollider>();
            if (col != null) col.material = _trackPhysMat;
        }

        if (mat != null)
            obj.GetComponent<Renderer>().material = mat;

        return obj;
    }

    static Material MakeMat(string name, Color color, float metallic = 0.3f, float smoothness = 0.6f)
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

    static Material MakeEmissiveMat(string name, Color color, Color emissionColor, float intensity = 2f, float metallic = 0.5f, float smoothness = 0.8f)
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

    public static Vector3 GetFinishLinePosition(TrackType type)
    {
        // Bucket is 2 below last curve point (y=-4.5) at z=82
        return new Vector3(0f, -6.5f, 82f);
    }

    public static Vector3 GetStartGatePosition(TrackType type)
    {
        // Gate just above the surface at the start (curve starts at y=3.0)
        Vector3 p = CurvePoint(0.05f); // slightly ahead of start
        return new Vector3(p.x, 3.75f, p.z);
    }

    // Surface height at the start of the track (for spawn points)
    public static float GetStartSurfaceY()
    {
        // Curve starts at y=3.0, floor is 0.5 thick, surface = 3.0 + 0.25
        return 3.25f;
    }
}
