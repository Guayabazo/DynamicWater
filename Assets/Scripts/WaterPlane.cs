using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaterPlane : MonoBehaviour
{
    public int dimension = 10;
    //public int resolution = 10;
    public MeshFilter meshFilter;
    public Mesh mesh;



    public Octave[] octaves;
    public float UVScale = 5f;
    public Vector3[] verts;



    //Vector3[] originalVertices, displacedVertices;
    //Vector3[] vertexVelocities;    
    public float damping = 1f;
    float uniformScale = 1f;


    [SerializeField]
    float
        springConstant = 5f,
        spread = 0.005f;        

    float[] velocities, accelerations;


    public int lastX, lastY;
    // Start is called before the first frame update
    void Awake()
    {        
        lastX = 0;
        lastY = 0;
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
        lastX = 0;
        lastY = 0;
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



        //meshFilter.mesh = mesh;



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

    private Vector3[] GenerateVelocities()
    {
        var velocities = new Vector3[(dimension + 1) * (dimension + 1)];
        // equaly distributed verts
        for (int x = 0; x <= dimension; x++)
            for (int z = 0; z <= dimension; z++)
                velocities[Index(x, z)] = Vector3.zero;
        return velocities;
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
                tris[Index(x, z) * 6 + 0] = Index(x, z);
                tris[Index(x, z) * 6 + 1] = Index(x + 1, z + 1);
                tris[Index(x, z) * 6 + 2] = Index(x + 1, z);
                tris[Index(x, z) * 6 + 3] = Index(x, z);
                tris[Index(x, z) * 6 + 4] = Index(x, z + 1);
                tris[Index(x, z) * 6 + 5] = Index(x + 1, z + 1);
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
                //var vec = new Vector2((x / UVScale) % 2, (z / UVScale) % 2);
                //uvs[Index(x, z)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
                //var vec = new Vector2((x / dimension), (z / dimension));
                uvs[Index(x, z)] = new Vector2(x / UVScale, z / UVScale);

            }
        }
        return uvs;
    }

    // Update is called once per frame
    void Update()
    {

        uniformScale = transform.localScale.x;
        //uniformScale = 1;

        verts = mesh.vertices;

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
                //var y = verts[Index(x, z)].y;


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


                        /*
                        Vector3 worldPos = transform.TransformPoint(verts[Index(x, z)]);
                        var perl = Mathf.PerlinNoise((worldPos.x * octaves[i].scale.x + Time.time * octaves[i].speed.x) / dimension,
                            (worldPos.z * octaves[i].scale.y + Time.time * octaves[i].speed.y) / dimension) - 0.5f; // -0.5f because perlin noise is between 0 and 1 and if we substract 0.5 then we have a value between -0.5f and 0.5f
                        y += perl * octaves[i].height;
                        */
                    }
                }



                //Mathf.Clamp(velocities[Index(x, z)], -0.1f, 0.1f);

                //verts[Index(x, z)] = new Vector3(x, y, z);



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
        {
            
            velocities[Index((int)p1.x, (int)p1.z)] += force;

            /*
            if ((int)p1.x > 0)
                velocities[Index((int)p1.x - 1, (int)p1.z)] += (force * 0.5f);
            if ((int)p1.x < dimension)
                velocities[Index((int)p1.x + 1, (int)p1.z)] += (force * 0.5f);
            if ((int)p1.z > 0)
                velocities[Index((int)p1.x, (int)p1.z - 1)] += (force * 0.5f);
            if ((int)p1.z < dimension)
                velocities[Index((int)p1.x, (int)p1.z + 1)] += (force * 0.5f);
            */
            //StartCoroutine(Ripple((int)p1.x, (int)p1.z, force));


        }

        if (p2.x > 0 && p2.x < dimension && p2.z > 0 && p2.z < dimension)
        {
            velocities[Index((int)p2.x, (int)p2.z)] += force;
        }
        if (p3.x > 0 && p3.x < dimension && p3.z > 0 && p3.z < dimension)
        {
            velocities[Index((int)p3.x, (int)p3.z)] += force;
        }
        if (p4.x > 0 && p4.x < dimension && p4.z > 0 && p4.z < dimension)
        {
            velocities[Index((int)p4.x, (int)p4.z)] += force;
        }

    }


    /*
    void AddForceToVertex(int i, Vector3 point, float force)
    {
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((point - transform.position), scale);
        displacedVertices[i] = mesh.vertices[i];
        Vector3 pointToVertex = displacedVertices[i] - localPos;
        pointToVertex *= uniformScale;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.deltaTime;
        //vertexVelocities[i] += pointToVertex.normalized * velocity;
        //vertexVelocities[i] += Vector3.down * velocity;
        velocities[i] -= force * velocity;
    }
    */

    /*
       void checkInput()
       {
           if (Input.GetMouseButton(0))
           {
               RaycastHit hit;
               if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
               {                
                   splashAtPoint((int)GetPos(hit.point).x, (int)GetPos(hit.point).y);
               }
           }
       }
       */

    IEnumerator Ripple(int x, int z, float force)
    {
        int frame = 0;
        float maxForce = 0.1f;
        float actualForce = 0f;

        while (frame < 20f && actualForce < maxForce && (x != lastX || z != lastY))
        {
            velocities[Index(x, z)] += force * 2f;
            frame++;
            actualForce += force * 2f;
            lastX = x;
            lastY = z;
            yield return null;
        }


        /*
        yield return null;
        velocities[Index(x, z)] += force;
        */
    }



    public Vector2 GetPos(Vector3 pos)
    {
        //scale factor and position in local space
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((pos - transform.position), scale);

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

        return new Vector2(p1.x, p1.z);
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
        Update();
    }

    private void OnValidate()
    {
        UpdateWater();        
    }
#endif
}
