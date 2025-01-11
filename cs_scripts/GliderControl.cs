using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GliderControl : MonoBehaviour, IVehicleControl
{
    public float slopelift;
    public Vector3 drag;
    public Vector3 lift;
    public Vector3 airspeed = new Vector3(0f,0f,0f); // rb.velocity in plane frame ()
    public Vector3 restoring_torque = new Vector3(0f,0f,0f);
    public Vector3 aerodyn_force = new Vector3(0f,0f,0f);
    public Vector3 cloud_suction;
    public float rollRate;
    
    public Vector3 atm_wind;
    public Vector3 total_vel;
    public Vector3 exp_slopew;
    public Rigidbody rb;
    public float thermals;
    public float cloud_exp;
    private Terrain land;
    public float slope_exp;
    public float slope_const;
    private BoxCollider collider;
    int i;
    int i_update_atm;
    public float wind_step;
    public float thermal_step;
    
    public float h_overterrain;
    public float restore_coeff_pitch;
    public float restore_coeff_yaw;
    public float yaw_damping_factor;
    
    public bool crashed;
    
    private Vector3 gravity;
    private Vector3 total_force;
    
    private Vector3 drag_vec = new Vector3(0f,0f,-1f);
    private Vector3 lift_vec = new Vector3(0f, 0.98f, 0.17f);
    
    private Vector3 velangles_in_planeframe = new Vector3(0f,0f,0f);
    
    
    public Vector3 tornado;
    
    public float drag_coeff;
    public float lift_coeff;
    public float init_velocity;
    public float aerodyn_const;
    public float control_torque;
    
    
    // Start is called before the first frame update
    void Start()
    {
         // Find the GameObject with the name "Terrain"
        GameObject terrainObject = GameObject.FindGameObjectsWithTag("Terrain")[0];
        land = terrainObject.GetComponent<Terrain>();
        
        rb = this.GetComponent<Rigidbody> ();
        rb.constraints = RigidbodyConstraints.None; // Remove any constraints
        rb.isKinematic = false; // Ensure the Rigidbody is not kinematic
        rb.centerOfMass = Vector3.zero;
        i_update_atm = 600;
        i=0;
        collider = this.GetComponent<BoxCollider> ();
        
        System.Random r = new System.Random();
        rb.inertiaTensor = new Vector3(40f,40f,40f);
        rb.linearVelocity = transform.forward.normalized*init_velocity;
        
        print("INIT VELOCITY IN GLIDER FRAME AND WORLD FRAME:");
        print(Quaternion.Inverse(transform.rotation) * transform.forward*rb.linearVelocity.magnitude);
        print(rb.linearVelocity);
    }
    
    void FixedUpdate(){
        //    DRAG & LIFT ONLY DEPEND ON WINDSPEED, NOT ON TOTAL VEL
        airspeed = rb.linearVelocity - exp_slopew - cloud_suction ;
        float v_forw = Vector3.Dot(airspeed, transform.forward); // velocity projected on the forward
        
        drag = drag_vec*drag_coeff*(float)Math.Pow(v_forw,2);
        if (v_forw > 0){ // travelling forward
            lift = lift_vec*(lift_coeff*(float)Math.Pow(v_forw,2));
            if (lift.magnitude > 5.0f)
                lift = lift*(1/lift.magnitude)*5.0f;
        }else{lift = lift*0;
        }
        
        gravity = new Vector3(0f, -5f, 0f);
        
        
        //+transform.TransformDirection(total_force)
        rb.AddRelativeForce(total_force, ForceMode.Acceleration);
        rb.AddForce(gravity, ForceMode.Acceleration);
        aerodyn_force = calc_aerodynamics();
        rb.AddRelativeForce(aerodyn_force, ForceMode.Acceleration);
        total_force = drag+lift+aerodyn_force;
        
        // Input applies a torque on the local system, rotating the lift vector
        float roll = Input.GetAxisRaw("Horizontal");
        float pitch = Input.GetAxisRaw("Vertical");
        float yaw = Input.GetAxisRaw("Yaw");
        float rollRate = rb.angularVelocity.z; // Roll rate around the forward axis
        float yawCorrection = -rollRate * yaw_damping_factor;
        Vector3 torque = new Vector3(1.5f*pitch, 1f*yaw+yawCorrection, roll*(-1f))*control_torque;
        
    
        restoring_torque = calc_restoring_torque();
        rb.AddRelativeTorque(torque, ForceMode.Acceleration);
        rb.AddRelativeTorque(restoring_torque, ForceMode.Acceleration);
        
        print("forward velocity:");
        print(v_forw);
        print("Tot Force (plane frame)");
        print(total_force+aerodyn_force+transform.InverseTransformDirection(gravity));
        
        
        if (rb.linearVelocity.magnitude > 1000){
            Application.Quit();
        }
        exp_slopew = slopewind(); // contains atm + slopewind
        slopelift = exp_slopew[1];
        //rb.velocity = rb.velocity + ;
        rb.AddForce((exp_slopew + cloud_suction), ForceMode.Acceleration);
        //rb.velocity = rb.velocity; + cloud_suction;
        //rb.position += rb.position+(exp_slopew+cloud_suction)*Time.deltaTime;
        
        bool breaks = Input.GetKey(KeyCode.B);
        if((breaks == true) && (v_forw > 0.1f)){
            rb.AddRelativeForce(new Vector3(0f, 0f, -3f), ForceMode.Acceleration);
        }
        
        cloud_suction = new Vector3(0f,0f,0f); // set zero because CloudParents will += cloud_suction
        
    }       
    
    public void set_cloud_suction(float cloudbase, float cloudsize){
        float distance_tobase = (float)Math.Abs(cloudbase-rb.position.y);
        cloud_suction += new Vector3(0f, 0.01f*thermals*cloudsize*(float)Math.Exp(-cloud_exp*distance_tobase), 0f);
        
        if (cloud_suction.y > 2.8f){
            cloud_suction.y = 2.8f;
        }
        
        print("Setting cloud suction in Glider:");
        print(cloud_suction.y);
    }
    
    public void set_atm_wind(Vector3 setted_atm_wind){
        atm_wind = setted_atm_wind;
    }
    
     public void set_tornado_component(Vector3 tor){
        tornado = tor;
    }
    
    public Vector3 slopewind(){
        //Computes the wind-terrain effect, in a simple manner.
        // Wind always blows in the predefined wind-direction (x,z)
        // The vertical component will depend on the dot product
        // w.e_grad, where w is the wind vector and e_grad the direction of steepest terrain descent
        Vector3 w = atm_wind;
        Vector3 pos = rb.position; // compute gradient at this point (one sided)
        
        
        float w_strength = w.magnitude;
        Vector3 e_w = w.normalized;
        
        
        pos = pos + Vector3.zero;
        Vector3 pospx;
        Vector3 pospz;
        
        pospx = pos + new Vector3(1.0f, 0.0f, 0.0f);
        pospz = pos + new Vector3(0.0f, 0.0f, 1.0f);
        
        
        float ho = land.SampleHeight(pos); // terrain height at position
        float hx = land.SampleHeight(pospx);
        float hy = land.SampleHeight(pospz);
        h_overterrain = transform.position.y - ho; // plane height over terrain
        
        
        //dx = 1. Now calc the gradient direction (ascent?) and strength:
        Vector3 gradient = new Vector3(1.0f*(hx-ho), 0.0f, 1.0f*(hy-ho));
        Vector3 e_grad = gradient.normalized;
        float steepness = gradient.magnitude;
        
        // Compute the vertical slope component. exp decays over height.
        float wy = (float)Vector3.Dot(e_w, e_grad)*steepness;
        wy = (float)Math.Exp(-slope_exp*h_overterrain)*wy*slope_const;
        
        // Instantiate experienced slope wind. Normalize to original wind strength
        Vector3 w_exp = new Vector3(w.x*0.5f, wy, w.z*0.5f);
        w_exp = w_exp.normalized*w_strength;
        
        
        return w_exp;
    }
    
    private Vector3 calc_aerodynamics(){
        airspeed = Quaternion.Inverse(transform.rotation) * rb.linearVelocity;
        float v_speed = airspeed[1];
        float side_speed = airspeed[0];
        
        aerodyn_force = new Vector3(-(float)side_speed, -(float)v_speed, 0f);
        
        return aerodyn_force*aerodyn_const;
        
        }
   
    private Vector3 calc_restoring_torque(){
        airspeed = Quaternion.Inverse(transform.rotation) * rb.linearVelocity;
        velangles_in_planeframe = Quaternion.LookRotation(airspeed.normalized).eulerAngles;
        float speed = rb.linearVelocity.magnitude;
        
        float alpha = velangles_in_planeframe[0];
        float beta = velangles_in_planeframe[1];
        if (alpha > 270)
            alpha = alpha - 360f;
        else if((alpha)>90 && (alpha)<270)
            alpha = 0f;
        if (beta > 270)
            beta = beta - 360f;
        else if((beta)>90 && (beta)<270)
            beta = 0f;
        
        // Only pitch and yaw create restoring torques
        // altitudal angle creates torque in x
        // azimutal angle creates torque in y
        restoring_torque = new Vector3(alpha*restore_coeff_pitch*speed/10f, beta*restore_coeff_yaw*speed/10f, 0f);
        //
        //-velangles_in_planeframe[0]*restore_coeff_yaw
        
        print("wind deflection: ");
        print(alpha.ToString()+", "+beta.ToString());
        print("rest. torque:"); //  angle of velocity rel to planeframe
        print(restoring_torque);
        
        
        
        return restoring_torque;
        }
        
        public void OnTriggerStay(Collider collider) // checks if aircraft is landed at airport
        {

            print("Crashed aircraft");
            crashed = true;

        }
    
        public bool has_crashed(){   // Aircraft has actually crashed
            return crashed;
        }
}
