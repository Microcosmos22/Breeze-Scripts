using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Instr_update : MonoBehaviour
{
    private GameObject player;
    private Rigidbody rb; // Reference to the parent Rigidbody
    
    public Slider Airspeedslider; // Reference to the Slider component
    public Slider verticalVelocitySlider; // Reference to the Slider component
    
    public GameObject GreenBar;
    private RectTransform GreenRectTrans;
    public GameObject RedBar;
    private RectTransform RedRectTrans;
    
    public float minAltitude = 0f;    // Minimum altitude (adjust as needed)
    public float maxAltitude = 850f; // Maximum altitude (adjust as needed)
    public Image pointerImage;        // Reference to the pointer Image component
    public float currentAltitude;
    public Image pointer1;
    public Image pointer2;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        GreenRectTrans = GreenBar.GetComponent<RectTransform>();
        RedRectTrans = RedBar.GetComponent<RectTransform>();
        
        //Airspeedslider = GetComponent<Slider>();
        
        // Get current altitude of the parent Rigidbody
        
        //pointer1 = GameObject.Find("PointAlt").GetComponent<Image>(); //GetComponentInChildren<PointAlt>();
        //pointer2 = GameObject.Find("PointAlt_fast").GetComponent<Image>(); // GetComponentInChildren<PointAlt_fast>();
    
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (rb != null){
        // Get the vertical component of the velocity
        float verticalVelocity = rb.linearVelocity.y;
        // Update the slider value with the current vertical velocity
        verticalVelocitySlider.value = -verticalVelocity; //rectTransform.localScale;
        
        
        if (verticalVelocity > 0){
            
            Vector3 newScale = GreenRectTrans.localScale;
            GreenRectTrans.localScale = new Vector3((float)verticalVelocity, newScale.y, newScale.z);
            RedRectTrans.localScale = new Vector3(0f, newScale.y, newScale.z);
        }else{
            
            Vector3 newScale = RedRectTrans.localScale;
            RedRectTrans.localScale = new Vector3((float)verticalVelocity, newScale.y, newScale.z);
            GreenRectTrans.localScale = new Vector3(0f, newScale.y, newScale.z);
        }
        
        
        // Get the velocity
        float vel = (float)rb.linearVelocity.magnitude;

        // Update the slider value with the current vertical velocity
        Airspeedslider.value = vel;

        currentAltitude = rb.position.y;
        // Calculate normalized value between 0 and 1
        float normalizedAltitude = Mathf.InverseLerp(minAltitude, maxAltitude, currentAltitude);

        // Calculate angle based on normalized altitude (360 degrees corresponds to full circle)
        float targetAngle = normalizedAltitude * 360f;

        // Rotate the pointer Image around the Z-axis
        pointer1.rectTransform.rotation = Quaternion.Euler(0f, 0f, -targetAngle);
        pointer2.rectTransform.rotation = Quaternion.Euler(0f, 0f, -targetAngle*10f);
            
        
        }
    }
    
    public void set_player(GameObject player)
    {
        this.player = player;
        Debug.Log("Passing player's RB to instruments");
        Debug.Log("Player has PlaneControl: " + (player.GetComponent<PlaneControl>() != null));
        Debug.Log("Player has GliderControl: " + (player.GetComponent<GliderControl>() != null));

        if (player.GetComponent<GliderControl>() != null)
        {
            this.rb = player.GetComponent<Rigidbody>();
            Debug.Log("Rigidbody set from GliderControl");
        }
        else if (player.GetComponent<PlaneControl>() != null)
        {
            this.rb = player.GetComponent<Rigidbody>();
            Debug.Log("Rigidbody set from PlaneControl");
        }
        else
        {
            Debug.LogError("Instrument found NO PLAYER SCRIPT");
        }

        if (this.rb == null)
        {
            Debug.LogError("Rigidbody is still null after attempting to set it.");
        }
        else
        {
            Debug.Log("Rigidbody successfully set.");
        }
    }
}
