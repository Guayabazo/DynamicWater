using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    private float horizontal;
    private float vertical;    
    public float speed;            
    public float turnSpeed;
            
    private float rotation;
    


    


    private Rigidbody rb;







    public bool underWater; //comprueba contacto suelo
    

    public GameObject turnHelper;
    public Floater floater;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        
        rotation = Vector3.Angle(Vector3.up, transform.TransformDirection(Vector3.up));

        //comprobar grouded


        if (floater.underwater)
        {
            underWater = true;
            //rb.drag = floater.WaterDrag;
        }
        else
        {
            underWater = false;
            //rb.drag = floater.AirDrag;
        }


        //fin comprobar grounded

        //movimiento
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;


        if (underWater)
        {
            rb.AddTorque(transform.up * horizontal * 100f * turnSpeed * Time.deltaTime); //giro
            if (turnHelper != null)
            {                
                rb.AddForce(turnHelper.transform.forward * speed * 0.05f * vertical * Time.deltaTime * 300f, ForceMode.Force);  //lo que te mueve                
            }
        }


    }
    // Update is called once per frame
    void Update()
    {
        //para entrada de movimiento
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
                
    }
}
