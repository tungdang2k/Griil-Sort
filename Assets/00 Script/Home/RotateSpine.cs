using UnityEngine;

public class RotateSpine : MonoBehaviour
{

    [SerializeField] private float m_speed = 10f;
   

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0f, 0f, -m_speed * Time.deltaTime);
    }
}
