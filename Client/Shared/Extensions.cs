using System;

namespace PondSharp.Client.Shared
{
    public static class Extensions
    {
        public static ArraySegment<T> Segment<T>(this T[] arr)
            => new(arr);
        
        public static ArraySegment<T> Segment<T>(this T[] arr, int offset, int count)
            => new(arr, offset, count);
    }
}