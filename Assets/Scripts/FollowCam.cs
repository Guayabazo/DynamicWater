using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform Target;
    public Transform player;
    public float Damping;
    public float aimSpeed;
    public float zoom;
    public float currentZoom;
    public Vector3 point;
    private Vector3 movilPoint;


    public Vector3 m_CurrentVelocity;
    Vector3 m_DampedPos;



    public Transform mainCamera;

    public float rotacion;
    public float rotacionVertical;

    public float alejamiento;
    private float newDamping;




    //private Vector3 playerPosition;
    //private bool onScreen;
    //Camera cam;//Camera Used To Detect Enemies On Screen

    void Start()
    {        
        currentZoom = zoom;                
    }

    void OnEnable()
    {
        if (Target != null)
            m_DampedPos = mainCamera.position;

        newDamping = 0.5f;
    }



    void Awake()
    {
        transform.position = mainCamera.position;
        transform.rotation = mainCamera.rotation;
    }




    void Update()
    {

        newDamping -= Time.deltaTime;
        if (newDamping < Damping + 0.1f)
        {
            newDamping = Damping;
        }

        alejamiento = Vector3.Distance(transform.position, player.transform.position);
        //playerPosition = cam.WorldToViewportPoint(Target.position);
        //onScreen = playerPosition.z > 0 && playerPosition.x > 0 && playerPosition.x < 1 && playerPosition.y > 0 && playerPosition.y < 1;

        if (Target != null)
        {

            //rotation
            Vector3 positionDirection = player.position - Target.position;
            float distancia = positionDirection.magnitude;
            positionDirection.Normalize();
            point = Target.position - (positionDirection * currentZoom);

            //rotacion = Vector3.Angle(player.transform.TransformDirection(Vector3.forward), transform.TransformDirection(Vector3.forward));
            //rotacionVertical = Vector3.Angle(player.transform.TransformDirection(Vector3.up), (transform.position - player.position)); //antigua. la normal es casi 90



            Vector3 desiredRot = new Vector3(0f, player.rotation.eulerAngles.y, 0f);
            Quaternion desiredFinal = Quaternion.Euler(desiredRot.x, desiredRot.y, desiredRot.z);


            transform.rotation = Quaternion.Slerp(transform.rotation, desiredFinal, aimSpeed * Time.deltaTime); //mira a donde mira el player
            //transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, aimSpeed * Time.deltaTime); //mira a donde mira el player

            //movement
            var pos = point;
            m_DampedPos = Damping < 0.01f
                ? pos : Vector3.SmoothDamp(m_DampedPos, pos, ref m_CurrentVelocity, newDamping);
            pos = m_DampedPos;

            transform.position = pos;


        }
    }
}
