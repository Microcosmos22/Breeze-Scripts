using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlaneControl : MonoBehaviour, IVehicleControl
{
    public Rigidbody rb;
    public BoxCollider collider;
    public Vector3 airspeed;
    public Vector3 total_vel;
    
    private ParticleSystem windParticles;
    
    public float omega;
    public float thermals;
    public float cloud_exp;
    public float climb_vel;
    private Terrain land;
    public float slope_exp;
    public float slope_const;
    public float glide_desc;
    public float sensitivity;
    public float wind_step;
    public float thermal_step;
    public Vector3 cloud_suction;
    public float h_overterrain;
    private float roll_angle;
    private float roll_angularvel;
    public Vector3 atm_wind = new Vector3(0f, 0f, 0f);
    public Vector3 tornado;
    
    private UIVirtualJoystick virtualJoystick;
    
    
    public bool crashed = false;
    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 200;
    
        rb = this.GetComponent<Rigidbody> ();
        if (GetComponentInChildren<ParticleSystem>() != null){
            windParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        GameObject terrainObject = GameObject.FindWithTag("Terrain");
        
        land = terrainObject.GetComponent<Terrain>();
        
        if (terrainObject != null)
            {land = terrainObject.GetComponent<Terrain>();
        }
        
        
        roll_angle = 0f;
        roll_angularvel = 0.2f; // degrees/frame
        
        rb = this.GetComponent<Rigidbody> ();
        collider = this.GetComponent<BoxCollider> ();
        climb_vel = 2.0f;
        sensitivity = 0.5f;
        if (GameObject.Find("UI_Virtual_Joystick") != null)
{        virtualJoystick = GameObject.Find("UI_Virtual_Joystick").GetComponent<UIVirtualJoystick>();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    
        float steer = Input.GetAxisRaw("Horizontal");
        float pitch = Input.GetAxisRaw("Vertical");
        airspeed = steer_plane(airspeed, steer, pitch);
        bool breaks = Input.GetKey(KeyCode.B);
        
        
        
        
        Vector3 slope_vel = wind();
        total_vel = airspeed + slope_vel;
        total_vel = total_vel + new Vector3(0.0f, glide_desc, 0.0f);
        total_vel = total_vel + cloud_suction + tornado;
        
        if(breaks == true){
            total_vel = total_vel + new Vector3(0.0f, -4f, 0.0f);
            if((h_overterrain < 0.07f)){
                print("breaking in runway");
                print(airspeed);
                print(total_vel);
               
                airspeed = airspeed*0.99f;
                
                // Recalculate total velocity after applying brake force
                total_vel = airspeed + slope_vel;
                total_vel += new Vector3(0.0f, glide_desc, 0.0f);
                total_vel += cloud_suction;
            }
        }
        
        bool elevator = Input.GetKey(KeyCode.T);
        if(elevator == true){
            total_vel = total_vel + new Vector3(0.0f, +10f, 0.0f);
        }
        
        
        
        UpdateParticleTrajectories();
        
        Vector3 forward = Vector3.Normalize(airspeed);
        plane_orientation(steer, pitch, forward);
        rb.linearVelocity = total_vel;
        
        cloud_suction = new Vector3(0f,0f,0f);
    }
    
    public void set_init_vel(Vector3 velocity){
        rb = this.GetComponent<Rigidbody> ();
        airspeed = velocity;
        total_vel = velocity;
        rb.linearVelocity = velocity;
    }
    
    void UpdateParticleTrajectories()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[windParticles.particleCount];
        int numParticles = windParticles.GetParticles(particles);
    
        // Modify each particle's velocity based on its position
        for (int i = 0; i < numParticles; i++)
            {Vector3 particlePos = particles[i].position;
            // Example trajectory logic: particles accelerate upward and to the right based on position
            Vector3 newVelocity = simple_slopewind(atm_wind, particlePos);
            particles[i].velocity = newVelocity*3f;}
    
    // Apply the modified particle states back to the system
    windParticles.SetParticles(particles, numParticles);
}

    public void set_tornado_component(Vector3 tor){
        tornado = tor;
    }
    
    public void set_cloud_suction(float cloudbase, float cloudsize){
        float distance_tobase = (float)Math.Abs(cloudbase-transform.position.y);
        cloud_suction = cloud_suction + new Vector3(0f, 0.01f*thermals*cloudsize*(float)Math.Exp(-cloud_exp*distance_tobase), 0f);
    }
    
    public void set_atm_wind(Vector3 setted_atm_wind){
        atm_wind = setted_atm_wind;
    }
    
    public Vector3 wind(){
        Vector3 pos = rb.position;
        Vector3 slope_vel = simple_slopewind(atm_wind, rb.position);
    
        return slope_vel;}
    
    public Vector3 simple_slopewind(Vector3 w, Vector3 position){
        //Computes the wind-terrain effect, in a simple manner.
        // Wind always blows in the predefined wind-direction (x,z)
        // The vertical component will depend on the dot product
        // w.e_grad, where w is the wind vector and e_grad the direction of steepest terrain descent
        
        float w_strength = w.magnitude;
        Vector3 e_w = w.normalized;
        
        //////////   REPEAT 5 TIMES
        
        
        Vector3 pos; // compute gradient at this point (one sided)
        pos = position + Vector3.zero;
        Vector3 pospx;
        Vector3 pospz;
        
        pospx = pos + new Vector3(1.0f, 0.0f, 0.0f); // Neighbours
        pospz = pos + new Vector3(0.0f, 0.0f, 1.0f);
        
        // Gradient calculation
        float ho = land.SampleHeight(pos); // terrain height at position
        float hx = land.SampleHeight(pospx); // height at x+dx
        float hy = land.SampleHeight(pospz); // height at y+dy
        h_overterrain = transform.position.y - ho; // plane height over terrain
        
        //dx = 1. Now calc the gradient direction (ascent?) and strength:
        Vector3 gradient = new Vector3(1.0f*(hx-ho), 0.0f, 1.0f*(hy-ho));
        Vector3 e_grad = gradient.normalized;
        float steepness = gradient.magnitude;
        
        // Compute the vertical slope component. exp decays over height.
        float wy = (float)Vector3.Dot(e_w, e_grad)*steepness;
        wy = (float)Math.Exp(-slope_exp*h_overterrain)*wy*slope_const;
        
        // Instantiate experienced slope wind. Normalize to original wind strength
        Vector3 w_exp = new Vector3(w.x, wy, w.z);
        w_exp = w_exp.normalized*w_strength;
        
        
        return w_exp;
    }
    
    public void set_parameters(float setcloudexp, float setslopeexp){
        thermals = 2.56f;
        cloud_exp = setcloudexp;
        slope_exp = setslopeexp;
        slope_const = 4.2f;
        glide_desc = -0.35f;
    }
    
    public Vector3 steer_plane(Vector3 vel_0, float steer, float pitch){
        
        // Transform angular_vel (my FPS) to angular_vel (target FPS)
        float my_angular_vel = 0.0055f;
        float omega = my_angular_vel * Time.deltaTime / 0.00625f; // TURN RATE
        omega = omega*steer*(-1);
        
        
        
        // Steering: Lets the horizontal pointer rotate while pressed
        
        vel_0 = new Vector3((float)vel_0.x * (float)Math.Cos(omega) - (float)vel_0.z * (float)Math.Sin(omega), 0, (float)vel_0.x * (float)Math.Sin(omega) + (float)vel_0.z * (float)Math.Cos(omega));
        
        float my_climb_vel = 3.0f * Time.deltaTime / 0.00625f;
        float climb = 3.0f * pitch * (-1);
        // Climbing changes hor. vel.
        
        float v_hor = (float)Math.Sqrt((float)Math.Pow((float)vel_0.x,2.0f)+(float)Math.Pow((float)vel_0.z,2.0f));
        
        if((climb > 0) && (v_hor<5)){           // Can't climb, too slow
            float dv_hor = 0;
            climb = 0;
        }else if((climb < 0) && (v_hor > 25)){  // Cant descend, too fast
            float dv_hor = 0;
            climb = 0;
        
        }else{
        
            float dv_hor = 0.2f*(float)Math.Sqrt((float)Time.deltaTime*climb_vel);
            vel_0.y += climb;
            
            if(pitch<0){
                float kappa = 1-dv_hor/v_hor;
                vel_0.x = vel_0.x*kappa;
                vel_0.z = vel_0.z*kappa;
            }else if(pitch >0){
                float kappa = 1+dv_hor/v_hor;
                vel_0.x = vel_0.x*kappa;
                vel_0.z = vel_0.z*kappa;
        }}
        
        return vel_0;
    }
        void plane_orientation(float hor_input, float vert_input, Vector3 forward){
        
        // This is the azimutal travel angle
        float phi = -(float)Math.Atan2((float)forward.z, (float)forward.x) + 3.1416f/2.0f;
        
        if (Math.Abs(hor_input) > 0){ // User inputting
            
            if (Math.Abs(roll_angle) < 30f){
                roll_angle += roll_angularvel*(-1f)*hor_input;
            }
        }else{
            
            if (Math.Abs(roll_angle) > 0){
                roll_angle = roll_angle*0.99f;}
        }
        
        transform.eulerAngles = new Vector3(
            0,
            phi*180.0f/3.1416f,
            roll_angle
        );}
        
        public void OnTriggerStay(Collider collider) // checks if aircraft is landed at airport
        {

            print("Crashed aircraft");
            crashed = true;

        }
    
        public bool has_crashed(){   // Aircraft has actually crashed
            return crashed;
        }
}
