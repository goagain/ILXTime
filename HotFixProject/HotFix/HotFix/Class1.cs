using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix
{
    public class HelloWorld_HotFix
    {
        public static void Start(HelloWorld self)
        {
            Debug.Log("Hello ILXTime, this method is injected");
        }

        public static int Add(HelloWorld self, int a , int b)
        {
            return a * b;
        }
    } 
}