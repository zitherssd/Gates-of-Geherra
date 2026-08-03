using UnityEngine;
#if UNITY_EDITOR
#endif

[RequireComponent(typeof(MeshCollider))]
[ExecuteAlways]
public class RhombusMeshCollider : MonoBehaviour
{
    public enum PlaneOrientation { XY, XZ }

    [Tooltip("Horizontal diagonal span of the rhombus (X axis)")]
    public float width = 1f;
    [Tooltip("Other diagonal span of the rhombus (Z axis when on XZ plane)")]
    public float height = 1f;
    [Tooltip("Depth (thickness) used when the rhombus lies on the XY plane (Z thickness)")]
    public float thickness = 0.1f;
    [Tooltip("Vertical height of the extruded rhombus (Y axis). The rhombus spans Y = [floorOffset, floorOffset + verticalHeight].")]
    public float verticalHeight = 1f;
    
    [Tooltip("Which plane the rhombus lies on: XY (upright) or XZ (flat on floor)")]
    public PlaneOrientation plane = PlaneOrientation.XZ;
    [Tooltip("Whether the MeshCollider should be convex (required for non-kinematic rigidbodies)")]
    public bool convex = true;
    [Header("Floor & Physics Options")]
    [Tooltip("When true, keep an existing CapsuleCollider for physics (do not remove it). Useful to use the capsule for movement and the rhombus for hit detection.")]
    public bool keepCapsuleForPhysics = true;
    [Tooltip("When true and no CapsuleCollider is present, create a child CapsuleCollider to act as the physics collider")]
    public bool createCapsuleIfMissing = false;
    [Tooltip("Local Y position of the rhombus base (floor). The rhombus spans Y = [floorOffset, floorOffset + verticalHeight]")]
    public float floorOffset = 0f;

    MeshCollider mc;

    void Reset() => Generate();
    void OnValidate() => Generate();
    void Awake() => Generate();
    void OnEnable() => Generate();

    void Generate()
    {
        mc = GetComponent<MeshCollider>();
        if (mc == null) mc = gameObject.AddComponent<MeshCollider>();

        Vector3[] verts;

        if (plane == PlaneOrientation.XY)
        {
            // original orientation: diamond in XY plane, thickness along Z
            Vector3 tf = new Vector3(0f, height * 0.5f,  thickness * 0.5f); // top front
            Vector3 rf = new Vector3(width * 0.5f, 0f,  thickness * 0.5f);   // right front
            Vector3 bf = new Vector3(0f, -height * 0.5f, thickness * 0.5f); // bottom front
            Vector3 lf = new Vector3(-width * 0.5f, 0f, thickness * 0.5f);  // left front
            Vector3 tb = new Vector3(0f, height * 0.5f, -thickness * 0.5f); // top back
            Vector3 rb = new Vector3(width * 0.5f, 0f, -thickness * 0.5f);  // right back
            Vector3 bb = new Vector3(0f, -height * 0.5f, -thickness * 0.5f); // bottom back
            Vector3 lb = new Vector3(-width * 0.5f, 0f, -thickness * 0.5f);  // left back

            verts = new Vector3[] { tf, rf, bf, lf, tb, rb, bb, lb };
        }
        else // XZ plane: flat on floor, verticalHeight along Y
        {
            float halfW = width * 0.5f;
            float halfD = height * 0.5f; // interpret height as depth when on XZ plane

            // floorOffset defines the base Y; top is base + verticalHeight
            float botY = floorOffset;
            float topY = floorOffset + verticalHeight;

            Vector3 fTop = new Vector3(0f,  topY,  halfD); // front top
            Vector3 rTop = new Vector3( halfW, topY, 0f); // right top
            Vector3 bTop = new Vector3(0f,  topY, -halfD); // back top
            Vector3 lTop = new Vector3(-halfW, topY, 0f); // left top

            Vector3 fBot = new Vector3(0f, botY,  halfD); // front bottom
            Vector3 rBot = new Vector3( halfW,botY, 0f); // right bottom
            Vector3 bBot = new Vector3(0f, botY, -halfD); // back bottom
            Vector3 lBot = new Vector3(-halfW,botY, 0f); // left bottom

            verts = new Vector3[] { fTop, rTop, bTop, lTop, fBot, rBot, bBot, lBot };
        }

        int[] tris = new int[] {
            // front face (0,1,2) (0,2,3)
            0,1,2, 0,2,3,
            // back face (4,6,5) (4,7,6) -- reversed winding so normals point out
            4,6,5, 4,7,6,
                // sides
                0,4,5, 0,5,1,   // top-right quad
                1,5,6, 1,6,2,   // right-bottom quad
                2,6,7, 2,7,3,   // bottom-left quad
                3,7,4, 3,4,0    // left-top quad
        };

        Mesh mesh = new Mesh();
        mesh.name = "RhombusColliderMesh";
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Clean up previous generated mesh if we own it
#if UNITY_EDITOR
        if (mc.sharedMesh != null && mc.sharedMesh.name == mesh.name)
        {
            DestroyImmediate(mc.sharedMesh, true);
        }
#else
        if (mc.sharedMesh != null && mc.sharedMesh.name == mesh.name)
        {
            Destroy(mc.sharedMesh);
        }
#endif

        mc.sharedMesh = mesh;
        mc.convex = convex;

        // post-generation: configure optional physics helpers
        if (keepCapsuleForPhysics && plane == PlaneOrientation.XZ)
        {
            // keep the rhombus as a normal collider; capsule remains available for physics if present
            mc.isTrigger = false;

            // compute capsule center Y as middle of the rhombus vertical span
            float capCenterY = floorOffset + verticalHeight * 0.5f;

            // prefer an existing CapsuleCollider on the same GameObject
            CapsuleCollider existing = GetComponent<CapsuleCollider>();
            if (existing == null)
            {
                existing = GetComponentInChildren<CapsuleCollider>(true);
            }

            if (existing == null && createCapsuleIfMissing)
            {
                string capName = "RhombusPhysicsCapsule";
                Transform capT = transform.Find(capName);
                GameObject capGo;
                if (capT == null)
                {
                    capGo = new GameObject(capName);
                    capGo.transform.SetParent(transform, false);
                }
                else capGo = capT.gameObject;

                float halfW = width * 0.5f;
                float halfD = height * 0.5f;
                float radius = Mathf.Max(0.01f, Mathf.Min(halfW, halfD) * 0.5f);
                float capHeight = Mathf.Max(verticalHeight, radius * 2f);

                CapsuleCollider cc = capGo.GetComponent<CapsuleCollider>();
                if (cc == null) cc = capGo.AddComponent<CapsuleCollider>();
                cc.direction = 1; // Y axis
                cc.radius = radius;
                cc.height = capHeight;
                cc.center = new Vector3(0f, capCenterY, 0f);
                cc.isTrigger = false;
            }
        }
        else
        {
            mc.isTrigger = false;
        }
    }

    void OnDestroy()
    {
        if (mc == null) mc = GetComponent<MeshCollider>();
        if (mc == null) return;
#if UNITY_EDITOR
        if (mc.sharedMesh != null && mc.sharedMesh.name == "RhombusColliderMesh") DestroyImmediate(mc.sharedMesh, true);
#else
        if (mc.sharedMesh != null && mc.sharedMesh.name == "RhombusColliderMesh") Destroy(mc.sharedMesh);
#endif
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        var t = transform;
        if (plane == PlaneOrientation.XY)
        {
            Vector3 p0 = t.TransformPoint(new Vector3(0f, height * 0.5f, 0f));
            Vector3 p1 = t.TransformPoint(new Vector3(width * 0.5f, 0f, 0f));
            Vector3 p2 = t.TransformPoint(new Vector3(0f, -height * 0.5f, 0f));
            Vector3 p3 = t.TransformPoint(new Vector3(-width * 0.5f, 0f, 0f));
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);
        }
        else
        {
            float halfW = width * 0.5f;
            float halfD = height * 0.5f;
            float topY = floorOffset + verticalHeight;
            Vector3 p0 = t.TransformPoint(new Vector3(0f, topY,  halfD));
            Vector3 p1 = t.TransformPoint(new Vector3( halfW, topY, 0f));
            Vector3 p2 = t.TransformPoint(new Vector3(0f, topY, -halfD));
            Vector3 p3 = t.TransformPoint(new Vector3(-halfW, topY, 0f));
            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);
        }
    }
}
