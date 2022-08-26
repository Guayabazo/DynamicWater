using System;

using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaterPlane : MonoBehaviour
{
    public int dimension = 50;
    //public int resolution = 10;
    public MeshFilter meshFilter;
    public Mesh mesh;

    public Octave[] octaves;
    public float UVScale = 5f;
    public Vector3[] verts;

    [SerializeField]
    private float
        springConstant = 5f,
        damping = 1f,
        spread = 0.005f;        

    float[] velocities, accelerations;

    // Start is called before the first frame update
    void Awake()
    {        
        
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = gameObject.name;
        }

        mesh.vertices = GenerateVertices();
        mesh.triangles = GenerateTriangles();
        mesh.uv = GenerateUVs();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        meshFilter.mesh = mesh;


        
        accelerations = GenerateAccelerations();
        velocities = accelerations;        
    }

    void UpdateWater()
    {        
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = gameObject.name;
        }

        mesh.vertices = GenerateVertices();
        mesh.triangles = GenerateTriangles();
        mesh.uv = GenerateUVs();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        accelerations = GenerateAccelerations();
        velocities = accelerations;
    }

    

    //para physics
    
    private float[] GenerateAccelerations()
    {
        var accelerations = new float[(dimension + 1) * (dimension + 1)];
        // equaly distributed verts
        for (int x = 0; x <= dimension; x++)
            for (int z = 0; z <= dimension; z++)
                accelerations[Index(x, z)] = 0f;
        return accelerations;
    }    
    //fin physics

    private Vector3[] GenerateVertices()
    {
        verts = new Vector3[(dimension + 1) * (dimension + 1)];
        // equaly distributed verts
        for (int x = 0; x <= dimension; x++)
            for (int z = 0; z <= dimension; z++)
                verts[Index(x, z)] = new Vector3(x, 0, z);
        return verts;
    }

    public int Index(int x, int z)
    {
        return x * (dimension + 1) + z;
    }

    private int[] GenerateTriangles()
    {
        var tris = new int[mesh.vertices.Length * 6];
        //two triangles are one tile
        for (int x = 0; x < dimension; x++)
        {
            for (int z = 0; z < dimension; z++)
            {
                tris[Index(x, z) * 6 + 2] = Index(x + 1, z);
                tris[Index(x, z) * 6 + 1] = Index(x + 1, z + 1);
                tris[Index(x, z) * 6 + 0] = Index(x, z);

                tris[Index(x, z) * 6 + 5] = Index(x + 1, z + 1);
                tris[Index(x, z) * 6 + 4] = Index(x, z + 1);
                tris[Index(x, z) * 6 + 3] = Index(x, z);
            }
        }
        return tris;
    }

    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[mesh.vertices.Length];

        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {                
                uvs[Index(x, z)] = new Vector2(x / UVScale, z / UVScale);
            }
        }
        return uvs;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //spread         
        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {
                if (x > 0)
                {
                    float left = spread * (verts[Index(x, z)].y - verts[Index(x - 1, z)].y);
                    velocities[Index(x - 1, z)] += left;
                }

                if (x < dimension)
                {
                    float right = spread * (verts[Index(x, z)].y - verts[Index(x + 1, z)].y);
                    velocities[Index(x + 1, z)] += right;
                }

                if (z > 0)
                {
                    float up = spread * (verts[Index(x, z)].y - verts[Index(x, z - 1)].y);
                    velocities[Index(x, z - 1)] += up;
                }

                if (z < dimension)
                {
                    float down = spread * (verts[Index(x, z)].y - verts[Index(x, z + 1)].y);
                    velocities[Index(x, z + 1)] += down;
                }
            }
        }


        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {
                var y = 0f;               
                for (int i = 0; i < octaves.Length; i++)
                {
                    if (octaves[i].alternate)
                    {
                        var perl = Mathf.PerlinNoise((x * octaves[i].scale.x) / dimension, (z * octaves[i].scale.y) / dimension) * Mathf.PI * 2f;
                        y += Mathf.Cos(perl + octaves[i].speed.magnitude * Time.time) * octaves[i].height;
                    }
                    else
                    {
                        var perl = Mathf.PerlinNoise((x * octaves[i].scale.x + Time.time * octaves[i].speed.x) / dimension,
                            (z * octaves[i].scale.y + Time.time * octaves[i].speed.y) / dimension) - 0.5f; // -0.5f because perlin noise is between 0 and 1 and if we substract 0.5 then we have a value between -0.5f and 0.5f
                        y += perl * octaves[i].height;
                    }
                }

                verts[Index(x, z)].y += velocities[Index(x, z)];

                float force = ((springConstant * 0.0001f) * (verts[Index(x, z)].y - y) - velocities[Index(x, z)] / (2 + (damping * 0.01f)));
                accelerations[Index(x, z)] = -force;
                velocities[Index(x, z)] += accelerations[Index(x, z)];
            }
        }

        //mesh.vertices = displacedVertices;
        mesh.vertices = verts;
        mesh.RecalculateNormals();
    }


    public Vector3 GetPoint(Vector3 point)
    {
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((point - transform.position), scale);

        //get edge points
        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //clamp if thye position is outside the plain
        p1.x = Mathf.Clamp(p1.x, 0, dimension);
        p1.z = Mathf.Clamp(p1.z, 0, dimension);
        p2.x = Mathf.Clamp(p2.x, 0, dimension);
        p2.z = Mathf.Clamp(p2.z, 0, dimension);
        p3.x = Mathf.Clamp(p3.x, 0, dimension);
        p3.z = Mathf.Clamp(p3.z, 0, dimension);
        p4.x = Mathf.Clamp(p4.x, 0, dimension);
        p4.z = Mathf.Clamp(p4.z, 0, dimension);
        return new Vector3((int)p1.x, (int)p1.y, (int)p1.z);
    }

    public void AddDeformingForce(Vector3 point, float force)
    {
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((point - transform.position), scale);

        //get edge points
        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //clamp if thye position is outside the plain
        p1.x = Mathf.Clamp(p1.x, 0, dimension);
        p1.z = Mathf.Clamp(p1.z, 0, dimension);
        p2.x = Mathf.Clamp(p2.x, 0, dimension);
        p2.z = Mathf.Clamp(p2.z, 0, dimension);
        p3.x = Mathf.Clamp(p3.x, 0, dimension);
        p3.z = Mathf.Clamp(p3.z, 0, dimension);
        p4.x = Mathf.Clamp(p4.x, 0, dimension);
        p4.z = Mathf.Clamp(p4.z, 0, dimension);
        
        if (p1.x > 0 && p1.x < dimension && p1.z > 0 && p1.z < dimension)                    
            velocities[Index((int)p1.x, (int)p1.z)] += force;        
        if (p2.x > 0 && p2.x < dimension && p2.z > 0 && p2.z < dimension)        
            velocities[Index((int)p2.x, (int)p2.z)] += force;        
        if (p3.x > 0 && p3.x < dimension && p3.z > 0 && p3.z < dimension)        
            velocities[Index((int)p3.x, (int)p3.z)] += force;        
        if (p4.x > 0 && p4.x < dimension && p4.z > 0 && p4.z < dimension)        
            velocities[Index((int)p4.x, (int)p4.z)] += force;        
    }
    
    public float GetHeight(Vector3 position)
    {
        //scale factor and position in local space
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((position - transform.position), scale);

        //get edge points
        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //clamp if thye position is outside the plain
        p1.x = Mathf.Clamp(p1.x, 0, dimension);
        p1.z = Mathf.Clamp(p1.z, 0, dimension);
        p2.x = Mathf.Clamp(p2.x, 0, dimension);
        p2.z = Mathf.Clamp(p2.z, 0, dimension);
        p3.x = Mathf.Clamp(p3.x, 0, dimension);
        p3.z = Mathf.Clamp(p3.z, 0, dimension);
        p4.x = Mathf.Clamp(p4.x, 0, dimension);
        p4.z = Mathf.Clamp(p4.z, 0, dimension);

        /*
        var height = (mesh.vertices[Index((int)p1.x, (int)p1.z)].y + 
            mesh.vertices[Index((int)p2.x, (int)p2.z)].y + 
            mesh.vertices[Index((int)p3.x, (int)p3.z)].y + 
            mesh.vertices[Index((int)p4.x, (int)p4.z)].y) / 4;
        return height * transform.lossyScale.y;
        */
        
        
        //get the max distance to one of the edges and take that to compute max - dist
        var max = Mathf.Max(Vector3.Distance(p1, localPos),
            Vector3.Distance(p2, localPos),
            Vector3.Distance(p3, localPos),
            Vector3.Distance(p4, localPos) + Mathf.Epsilon);

        var dist = (max - Vector3.Distance(p1, localPos))
                 + (max - Vector3.Distance(p2, localPos))
                 + (max - Vector3.Distance(p3, localPos))
                 + (max - Vector3.Distance(p4, localPos) + Mathf.Epsilon);

        //weighted sum
        var height = mesh.vertices[Index((int)p1.x, (int)p1.z)].y * (max - Vector3.Distance(p1, localPos))
                   + mesh.vertices[Index((int)p2.x, (int)p2.z)].y * (max - Vector3.Distance(p2, localPos))
                   + mesh.vertices[Index((int)p3.x, (int)p3.z)].y * (max - Vector3.Distance(p3, localPos))
                   + mesh.vertices[Index((int)p4.x, (int)p4.z)].y * (max - Vector3.Distance(p4, localPos));

        //scale
        return height * transform.lossyScale.y / dist;
        
    }


    [Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        FixedUpdate();
    }

    private void OnValidate()
    {
        UpdateWater();        
    }
#endif

}
