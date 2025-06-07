using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class DynamicDepthOfField : MonoBehaviour
{
    public Volume volume;
    private DepthOfField depthOfField;

    private RaycastHit hit;
    void Start()
    {
        
        volume.profile.TryGet<DepthOfField>(out depthOfField);
    }
    void Update()
    {
        Physics.Raycast(transform.position, transform.forward, out hit);
        if (hit.collider != null) 
        {
            depthOfField.focusDistance.value = Mathf.Lerp(depthOfField.focusDistance.value, hit.distance, Time.deltaTime * 10f);
        }   
    }
}
