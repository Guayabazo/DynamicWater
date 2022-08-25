using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    //public properties
    public float AirDrag = 0.1f;
    public float WaterDrag = 1.5f;
    public bool AffectDirection = true;
    public bool AttachToSurface = false;
    public Transform[] FloatPoints;
    public bool[] bajoAgua;
    public float floatingPower = 15f;
    public bool underwater;
    public bool wasOnWater;

    //used components
    protected Rigidbody rb;
    public WaterPlane Waves;

    //water line
    protected float WaterLine;
    protected Vector3[] WaterLinePoints;

    //help Vectors
    protected Vector3 smoothVectorRotation;
    protected Vector3 TargetUp;
    protected Vector3 centerOffset;

    public Vector3 Center { get { return transform.position + centerOffset; } }

    // Start is called before the first frame update
    void Awake()
    {
        //get components
        Waves = FindObjectOfType<WaterPlane>();
        rb = GetComponent<Rigidbody>();
        //rb.useGravity = false;

        //compute center
        WaterLinePoints = new Vector3[FloatPoints.Length];
        for (int i = 0; i < FloatPoints.Length; i++)
            WaterLinePoints[i] = FloatPoints[i].position;
        centerOffset = PhysicsHelper.GetCenter(WaterLinePoints) - transform.position;

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        /*
        //default water surface
        var newWaterLine = 0f;
        var pointUnderWater = false;

        //set WaterLinePoints and WaterLine
        for (int i = 0; i < FloatPoints.Length; i++)
        {
            //height
            WaterLinePoints[i] = FloatPoints[i].position;
            WaterLinePoints[i].y = Waves.GetHeight(FloatPoints[i].position);
            newWaterLine += WaterLinePoints[i].y / FloatPoints.Length;
            if (WaterLinePoints[i].y > FloatPoints[i].position.y)
                pointUnderWater = true;
        }

        var waterLineDelta = newWaterLine - WaterLine;
        WaterLine = newWaterLine;

        //compute up vector
        TargetUp = PhysicsHelper.GetNormal(WaterLinePoints);

        //gravity
        var gravity = Physics.gravity;
        rb.drag = AirDrag;
        if (WaterLine > Center.y)
        {
            rb.drag = WaterDrag;
            //under water
            if (AttachToSurface)
            {
                //attach to water surface
                rb.position = new Vector3(rb.position.x, WaterLine - centerOffset.y, rb.position.z);
            }
            else
            {
                //go up
                gravity = AffectDirection ? TargetUp * -Physics.gravity.y : -Physics.gravity;
                transform.Translate(Vector3.up * waterLineDelta * 0.9f);
            }
        }
        rb.AddForce(gravity * Mathf.Clamp(Mathf.Abs(WaterLine - Center.y), 0, 1));

        //rotation
        if (pointUnderWater)
        {
            //attach to water surface
            TargetUp = Vector3.SmoothDamp(transform.up, TargetUp, ref smoothVectorRotation, 0.2f);
            rb.rotation = Quaternion.FromToRotation(transform.up, TargetUp) * rb.rotation;
        }
        */




        if (Waves != null)
        {
            underwater = false;
            for (int i = 0; i < FloatPoints.Length; i++)
            {
                WaterLinePoints[i] = FloatPoints[i].position;
                WaterLinePoints[i].y = Waves.GetHeight(FloatPoints[i].position);
                //float difference = FloatPoints[i].position.y - Waves.GetHeight(FloatPoints[i].position);

                Vector3 height = Vector3.Lerp(FloatPoints[i].position, new Vector3(FloatPoints[i].position.x, Waves.GetHeight(FloatPoints[i].position), FloatPoints[i].position.z), 10f * Time.deltaTime);
                float difference = FloatPoints[i].position.y - height.y;


                if (difference < 0)
                {

                    underwater = true;
                    //bajoAgua[i] = true;


                    float submersion = Mathf.Clamp01(Mathf.Abs(difference));
                    rb.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(submersion), FloatPoints[i].position, ForceMode.Force);
                }
            }
        }
        if (underwater)
            rb.drag = WaterDrag;
        else
            rb.drag = AirDrag;






    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (FloatPoints == null)
            return;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            if (FloatPoints[i] == null)
                continue;

            if (Waves != null)
            {

                //draw cube
                Gizmos.color = Color.red;
                Gizmos.DrawCube(WaterLinePoints[i], Vector3.one * 0.1f);
            }

            //draw sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(FloatPoints[i].position, 0.1f);

        }

        //draw center
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(Center.x, WaterLine, Center.z), Vector3.one * 0.1f);
            Gizmos.DrawRay(new Vector3(Center.x, WaterLine, Center.z), TargetUp * 0.1f);
        }
    }
}
