using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    //public properties
    public float AirDrag = 0.1f;
    public float WaterDrag = 1.5f;    
    public Transform[] FloatPoints;    
    public float floatingPower = 15f;
    [System.NonSerialized]
    public bool underwater;
    
    //used components
    private Rigidbody rb;
    public WaterPlane Waves;

    //water line    
    private Vector3[] WaterLinePoints;

    
        

    // Start is called before the first frame update
    void Awake()
    {
        //get components
        Waves = FindObjectOfType<WaterPlane>();
        rb = GetComponent<Rigidbody>();        
        
        WaterLinePoints = new Vector3[FloatPoints.Length];
        for (int i = 0; i < FloatPoints.Length; i++)
            WaterLinePoints[i] = FloatPoints[i].position;        
    }

    
    void FixedUpdate()
    {        
        if (Waves != null)
        {
            underwater = false;
            for (int i = 0; i < FloatPoints.Length; i++)
            {
                WaterLinePoints[i] = FloatPoints[i].position;
                WaterLinePoints[i].y = Waves.GetHeight(FloatPoints[i].position);
                

                Vector3 height = Vector3.Lerp(FloatPoints[i].position, new Vector3(FloatPoints[i].position.x, WaterLinePoints[i].y, FloatPoints[i].position.z), 10f * Time.deltaTime);
                float difference = FloatPoints[i].position.y - height.y; //to make it softer

                //float difference = FloatPoints[i].position.y - WaterLinePoints[i].y;
                if (difference < 0)
                {
                    underwater = true;                    
                    float submersion = Mathf.Clamp01(Mathf.Abs(difference));
                    rb.AddForceAtPosition(Vector3.up * floatingPower * submersion, FloatPoints[i].position, ForceMode.Force);
                }
            }
        }
        if (underwater)
            rb.drag = WaterDrag;
        else
            rb.drag = AirDrag;
    }


    /*
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
        
    }
    */
}
