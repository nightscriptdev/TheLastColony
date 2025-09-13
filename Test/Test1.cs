using UnityEngine;

namespace Test
{
    public class Test1 : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1)) Time.timeScale = 1;
            if(Input.GetKeyDown(KeyCode.Alpha2)) Time.timeScale = 2;
            if(Input.GetKeyDown(KeyCode.Alpha3)) Time.timeScale = 3;
        }
    }
}
