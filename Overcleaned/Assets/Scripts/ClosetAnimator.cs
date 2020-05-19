using UnityEngine;

public class ClosetAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator closetAnimator;

    [SerializeField]
    private MeshRenderer[] renderers;

    [SerializeField]
    private Material dirty_Material;

    [SerializeField]
    private Material clean_Material;

    public void SetStateDoor(bool isOpen) 
    {
        const string BOOL_NAME = "IsOpen";
        closetAnimator.SetBool(BOOL_NAME, isOpen);

        foreach (MeshRenderer renderer in renderers) 
        {
            renderer.material = isOpen ? dirty_Material : clean_Material;
        }
    }
}
