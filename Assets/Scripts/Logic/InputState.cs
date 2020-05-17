using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Common;
using Unity.Mathematics;
using UnityEngine;

namespace Logic
{
    [Flags]
    public enum Input
    {
        Up    = 0x01,
        Down  = 0x02,
        Left  = 0x04,
        Right = 0x08,
        A     = 0x10,
        B     = 0x20,
        C     = 0x40,
        D     = 0x80
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct InputState : IEquatable<InputState>
    {
        public Input Inputs;

        public bool HasInput(Input input) => (Inputs & input) == input;
        public bool Equals(InputState other) => Inputs == other.Inputs;

        public static InputState ReadLocalInputs()
        {
            var direction = new int2
            (
                (UnityEngine.Input.GetKey(KeyCode.D) ? 1 : 0) + (UnityEngine.Input.GetKey(KeyCode.A) ? -1 : 0),
                (UnityEngine.Input.GetKey(KeyCode.W) ? 1 : 0) + (UnityEngine.Input.GetKey(KeyCode.S) ? -1 : 0)
            );
            
            return new InputState
            {
                Inputs 
                    = (direction.x > 0 ? Input.Right : 0)
                    | (direction.x < 0 ? Input.Left  : 0)
                    | (direction.y > 0 ? Input.Up    : 0)
                    | (direction.y < 0 ? Input.Down  : 0)
                    | (UnityEngine.Input.GetKey(KeyCode.U) ? Input.A : 0)
                    | (UnityEngine.Input.GetKey(KeyCode.I) ? Input.B : 0) 
                    | (UnityEngine.Input.GetKey(KeyCode.O) ? Input.C : 0) 
                    | (UnityEngine.Input.GetKey(KeyCode.P) ? Input.D : 0) 
            };
        }
    }
}