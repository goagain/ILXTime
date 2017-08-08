using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloWorld : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log("Hello World ..3");
	}

    int count = 0;
    // Update is called once per frame
    void Update()
    {
        if ((count++) % 100 == 0)
        {
            Debug.Log("10 + 20 = " + Add(10, 20));
        }
    }
 
    static int Add(int a, int b)
    {
        return a + b;
    }
}
