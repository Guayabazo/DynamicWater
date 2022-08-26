using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterRipples : MonoBehaviour
{
    private WaterPlane waves;    
    private bool inOut;
    private bool pointUnderWater;
    public float distanceToSurface = 1f;
        
    private Vector3 WaterLinePoint;

    private Vector3 lastPos = Vector3.zero;

    public float force = 0.1f;
    public float maxForce = 0.2f;
    
    private Vector3 lastPoint;



    // Start is called before the first frame update
    void Start()
    {
        waves = FindObjectOfType<WaterPlane>();
        inOut = pointUnderWater;
        lastPoint = Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        float speed = (pos - lastPos).magnitude;
 
        WaterLinePoint = transform.position;
        WaterLinePoint.y = waves.GetHeight(transform.position);   
        
        if (WaterLinePoint.y > transform.position.y)
            pointUnderWater = true;
        else        
            pointUnderWater = false;
        
        
        if (pointUnderWater && inOut == pointUnderWater/* && Vector3.Distance(lastPoint, point) > 0.1f*/)
        {
            Vector3 actualPoint = waves.GetPoint(transform.position);
            if (actualPoint != lastPoint)
            {
                float actualForce = 0f;
                if (WaterLinePoint.y - transform.position.y < distanceToSurface)
                    actualForce = force * (distanceToSurface - (WaterLinePoint.y - transform.position.y)) * speed * Time.deltaTime * 100f;

                if (actualForce > maxForce)
                    actualForce = maxForce;
                if (actualForce < -maxForce)
                    actualForce = -maxForce;
                
                waves.AddDeformingForce(transform.position, actualForce);                                
                lastPoint = actualPoint;
            }

        }

        if (inOut != pointUnderWater)
        {
            Vector3 actualPoint = waves.GetPoint(transform.position);            
            inOut = pointUnderWater;
            float actualForce = 0f;
            if (WaterLinePoint.y - transform.position.y < distanceToSurface)
                actualForce = force * speed * Time.deltaTime * 100f;
            
            if (actualForce > maxForce)
                actualForce = maxForce;
            
            waves.AddDeformingForce(transform.position, actualForce);
            inOut = pointUnderWater;
            lastPoint = actualPoint;
        }

        lastPos = pos;
    }


}
