using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;


public class CloudFinder : MonoBehaviour
{
    private PostProcessVolume ppVolume;
    
    public Vector3 atm_wind;
    public float thermals;
    public float wind_step;
    public float thermal_step;
    public float cloud_exp;
    
    public GameObject[] instantiatedClouds; // Array to store instantiated clouds
    private MeshCollider cloudCollider;

    public float cloudbase;
    
    public List<PlaneControl> pcs = new List<PlaneControl>();
    public List<GliderControl> gcs = new List<GliderControl>();
    
    public Terrain terrain;             // Reference to the Terrain
    
    private float mapWidth;             // Width of the map
    private float mapLength;            // Length of the maps
    
    private float clouds_overhead_player;
    
    public float cloud_sucksize;
    public int i = 0;
    public int i_update_atm = 600;
    public ParticleForceField[] forcefields;
    
    private bool tornado_present;

    // Start is called before the first frame update
    void Start()
    {
        
        ppVolume = GetComponent<PostProcessVolume>();
        ppVolume.enabled = true;
    
        // Find Terrain
        GameObject terrainObject = GameObject.FindWithTag("Terrain");
        terrain = terrainObject.GetComponent<Terrain>();
        
        // Find all players in Scene
        PlaneControl[] planeControls = FindObjectsOfType<PlaneControl>();
        pcs = planeControls.ToList();
        GliderControl[] gliderControls = FindObjectsOfType<GliderControl>();
        gcs = gliderControls.ToList();
        forcefields = FindObjectsOfType<ParticleForceField>();
        
        // Get the dimensions of the terrain
        mapWidth = terrain.terrainData.size.x;
        mapLength = terrain.terrainData.size.z;
        
        // Find clouds
        instantiatedClouds = GameObject.FindGameObjectsWithTag("Clouds");

        
        
        print("Setting atm wind in pcs and particles");
        // Set wind for planes and gliders
        set_all_atm_winds(atm_wind);
        
    }
    
    public void set_all_atm_winds(Vector3 scene_atm_wind){
        atm_wind = scene_atm_wind;
        
        print("Setting winds for elements:");
    
        foreach (PlaneControl pc in pcs)
            {pc.set_atm_wind(scene_atm_wind);}
        foreach (ParticleForceField pff in forcefields)
            {pff.set_atm_wind(scene_atm_wind*0.3f);
            print(pff.gameObject.name);}
        foreach (GliderControl gc in gcs)
            {gc.set_atm_wind(scene_atm_wind);}
    }
    
    public void set_quickatm_winds(Vector3 scene_atm_wind){
        atm_wind = scene_atm_wind;
    
        foreach (PlaneControl pc in pcs)
            {pc.set_atm_wind(scene_atm_wind);}
        
        foreach (GliderControl gc in gcs)
            {gc.set_atm_wind(scene_atm_wind);}
    }
    
public void set_tornado_present(bool init_tornado){
    tornado_present = init_tornado;
}
    
public Vector3 CalculateTornadoVelocity(Vector3 tornadoCenter, float x, float y, float z, float k = 1000000.0f, float alpha = 100.0f, float beta = 0.0f, float H = 300.0f)
    {                                   // Computes the tornado component, depending on the plane's position
        // Calculate the relative position
        float relX = x - tornadoCenter.x;
        float relZ = z - tornadoCenter.z;
        
        // Calculate radial distance
        float r = Mathf.Sqrt(relX * relX + relZ * relZ);
        
        
        // Calculate velocity components
        float vX = k * (relZ / (r*r));  // Radial component in X
        float vZ = -k * (relX / (r*r));    // Radial component in Z
        float vY = Mathf.Clamp(alpha * Mathf.Exp(-beta * z) / r, 0f, 7f);
         ; // Vertical component, decaying with height
        
        return new Vector3(vX, vY, vZ);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (PlaneControl pc in pcs)
        { 
            // Check if plane(s) somewhere under a cloud
            
            if (pc.rb != null){
            clouds_overhead_player = is_pc_undercloud(pc)* 5f ;
            

            if (tornado_present){
                Vector3 pos = pc.rb.transform.position;
                Vector3 tornado = CalculateTornadoVelocity(new Vector3(500f, 0f, 500f), pos.x, pos.y, pos.z, 500.0f, 300.0f, 0.003465f, 300.0f);
                pc.set_tornado_component(tornado);
            }
            pc.set_cloud_suction(cloudbase, clouds_overhead_player);
            set_quickatm_winds(atm_wind);
            }
        }
        
        foreach (GliderControl gc in gcs)
        {  
            if (gc.rb != null){
            // Check if plane(s) somewhere under a cloud
            clouds_overhead_player = is_gc_undercloud(gc)* 5f ;
            

            if (tornado_present){
                Vector3 pos = gc.rb.transform.position;
                Vector3 tornado = CalculateTornadoVelocity(new Vector3(500f, 0f, 500f), pos.x, pos.y, pos.z, 500.0f, 300.0f, 0.003465f, 300.0f);
                gc.set_tornado_component(tornado);
            }
            gc.set_cloud_suction(cloudbase, clouds_overhead_player);
            set_quickatm_winds(atm_wind);
            }
        }
    }
    
    public void set_cloudbase(float setcloudbase){
        cloudbase = setcloudbase;
    }
    
    
    public void find_planecontrolscript(){
        PlaneControl[] planeControls = FindObjectsOfType<PlaneControl>();
        pcs = planeControls.ToList();
    }
    
    private float is_pc_undercloud(PlaneControl pc)
    {
        float total_cloud_overhead_player = 0f;
        
        foreach (GameObject cloud in instantiatedClouds)
        {
            if (cloud == null) continue;

            float r = cloud.transform.lossyScale.x * cloud_sucksize; //radius of the cloud
            Vector3 dist = cloud.transform.position - pc.rb.position; // vector player cloud
            float cpdistance = (float)Math.Sqrt((float)Math.Pow(dist.x, 2f) + (float)Math.Pow(dist.z, 2f));
            
            if (cpdistance < r) // We are in the cloud cylinder
            {
                float Cs = cloud.transform.lossyScale.x;
                float slope = -Cs / (2 * r);
                
                total_cloud_overhead_player += Cs * 5 / 4 + cpdistance * slope; //returns size/strength of the cloud
                
                // Now are we also INSIDE THE CLOUD?
                cloudCollider = cloud.GetComponent<MeshCollider>();
                
                /*if (cloudCollider.bounds.Contains(pc.rb.position))
                {
                    // Player is inside the cloud, enable the post-processing effect
                    ppVolume.enabled = true;
                }
                else
                {
                    // Player is outside the cloud, disable the post-processing effect
                    ppVolume.enabled = false;
                }*/
            
            }
        }
        return total_cloud_overhead_player;
    }
    
    private float is_gc_undercloud(GliderControl gc)
    {
        float total_cloud_overhead_player = 0f;
        
        foreach (GameObject cloud in instantiatedClouds)
        {
            if (cloud == null) continue;

            float r = cloud.transform.lossyScale.x * cloud_sucksize; //radius of the cloud
            Vector3 dist = cloud.transform.position - gc.rb.position; // vector player cloud
            float cpdistance = (float)Math.Sqrt((float)Math.Pow(dist.x, 2f) + (float)Math.Pow(dist.z, 2f));
            
            if (cpdistance < r)
            {
                float Cs = cloud.transform.lossyScale.x;
                float slope = -Cs / r;
                
                total_cloud_overhead_player += Cs * 3 / 2 + cpdistance * slope; //returns size/strength of the cloud
                
                // Now are we also INSIDE THE CLOUD?
                cloudCollider = cloud.GetComponent<MeshCollider>();
                
                
                
            }
            
        }
        return total_cloud_overhead_player;
    }
    
}
