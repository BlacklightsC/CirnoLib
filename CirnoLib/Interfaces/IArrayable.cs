namespace CirnoLib
{
    /// <summary>
    /// 해당 클래스가 바이트 배열의 형식으로 직렬화할 수 있음을 나타냅니다.
    /// </summary>
    public interface IArrayable
    {

        /// <summary>
        /// 해당 <see cref="IArrayable"/>을 바이트 배열로 반환합니다.
        /// </summary>
        /// <returns><see cref="IArrayable"/>의 바이트 배열입니다.</returns>
        byte[] ToArray();
    }
}
