// using System;
//
// namespace Common
// {
//     public static class ArrayUtility
//     {
//         public static void InsertAt<T>(ref T[] array, int index, T element)
//         {
//             if (array == null) throw new NullReferenceException();
//             if (index < 0 || index > array.Length) throw new IndexOutOfRangeException();
//             
//             var newArray = new T[array.Length + 1];
//
//             if (index == 0)
//             {
//                 Array.Copy(array, 0, newArray, 1, array.Length);
//             }
//             else if (index == array.Length)
//             {
//                 Array.Copy(array, 0, newArray, 0, array.Length);
//             }
//             else
//             {
//                 Array.Copy(array, 0, newArray, 0, index);
//                 Array.Copy(array, index, newArray, index + 1, array.Length - index);
//             }
//
//             newArray[index] = element;
//             array = newArray;
//         }
//     }
// }