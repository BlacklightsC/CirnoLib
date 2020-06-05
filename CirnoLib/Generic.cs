namespace CirnoLib
{
    public static class Generic
    {
        public static void Swap<T> (ref T A, ref T B)
        {
            T C = A;
            A = B;
            B = C;
        }
    }
}
