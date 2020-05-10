using System;
using System.Runtime.InteropServices;
using Common;
using Unity.Mathematics;

namespace Logic
{
    [StructLayout(LayoutKind.Sequential)]
    public struct InputState : IEquatable<InputState>
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Package
        {
            public int Frame;
            public InputState State;
        }
        
        public int2 Direction;
        public bool A, B, C, D;

        public bool Equals(InputState other)
        {
            return Direction.Equals(other.Direction)
                && A == other.A 
                && B == other.B 
                && C == other.C 
                && D == other.D;
        }
        
        public byte[] CreatePackage(int frame)
        {
            return new Package {Frame = frame, State = this}.ToBytes();
        }

        public static (int frame, InputState state) ReadPackage(byte[] bytes)
        {
            var package = bytes.ToStruct<Package>();
            return (package.Frame, package.State);
        }
    }
}