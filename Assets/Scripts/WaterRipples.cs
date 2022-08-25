using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterRipples : MonoBehaviour
{
    protected WaterPlane waves;


    public bool done;
    private bool inOut;
    private bool pointUnderWater;
    public float distanceToSurface = 1f;
    //water line
    protected float WaterLine;
    protected Vector3 WaterLinePoint;

    private Vector3 lastPos = Vector3.zero;



    public float force = 0.1f;
    public float maxForce = 0.2f;


    private float contador;
    private Vector3 lastPoint;



    // Start is called before the first frame update
    void Start()
    {
        waves = FindObjectOfType<WaterPlane>();
        inOut = pointUnderWater;
        lastPoint = Vector3.zero;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        float speed = (pos - lastPos).magnitude;
        contador += Time.deltaTime;
        var newWaterLine = 0f;

        inOut = pointUnderWater;

        WaterLinePoint = transform.position;
        WaterLinePoint.y = waves.GetHeight(transform.position);
        newWaterLine += WaterLinePoint.y;
        if (WaterLinePoint.y > transform.position.y)
            pointUnderWater = true;
        else
        {
            pointUnderWater = false;
        }


        Vector3 point = transform.position;
        if (pointUnderWater && inOut == pointUnderWater/* && Vector3.Distance(lastPoint, point) > 0.1f*/)
        {
            Vector3 actualPoint = waves.GetPoint(point);
            if (actualPoint != lastPoint)
            {
                float actualForce = 0f;
                if (WaterLinePoint.y - transform.position.y < distanceToSurface)
                    actualForce = force * (distanceToSurface - (WaterLinePoint.y - transform.position.y)) * speed * Time.deltaTime * 100f;

                if (actualForce > maxForce)
                    actualForce = maxForce;
                if (actualForce < -maxForce)
                    actualForce = -maxForce;
                //point += hit.normal * forceOffset;
                waves.AddDeformingForce(point, actualForce);
                done = true;

                contador = 0f;
                lastPoint = actualPoint;
            }

        }

        if (inOut != pointUnderWater)
        {
            Vector3 actualPoint = waves.GetPoint(point);
            waves.lastX = -1;
            waves.lastY = -1;
            inOut = pointUnderWater;
            float actualForce = 0f;
            if (WaterLinePoint.y - transform.position.y < distanceToSurface)
                actualForce = force * speed * Time.deltaTime * 100f;
            point = transform.position;
            if (actualForce > maxForce)
                actualForce = maxForce;
            //point += hit.normal * forceOffset;
            waves.AddDeformingForce(point, actualForce);
            done = true;
            contador = 0f;
            lastPoint = actualPoint;
        }

        lastPos = pos;
    }


}
