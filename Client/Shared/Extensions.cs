using System;

namespace PondSharp.Client.Shared
{
    public static class Extensions
    {
        public static ArraySegment<T> Segment<T>(this T[] arr) =>
            new ArraySegment<T>(arr);
        
        public static ArraySegment<T> Segment<T>(this T[] arr, int offset, int count) => 
            new ArraySegment<T>(arr, offset, count);
    }
}