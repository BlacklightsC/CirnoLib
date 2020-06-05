using System;

namespace CirnoLib
{
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SerialData : Attribute
    {
        /// <summary>
        /// 해당 데이터를 읽는 우선 순위입니다.
        /// </summary>
        public int Index;

        /// <summary>
        /// 데이터를 읽기 전에 지나칠 선행 공백입니다.
        /// </summary>
        public int LeadSpace;

        /// <summary>
        /// 데이터를 읽은 후에 지나칠 후행 공백입니다.
        /// </summary>
        public int TrailSpace;

        /// <summary>
        /// 데이터가 배열과 같은 방식으로 인하여 연속되는지에 대해서 나타냅니다.
        /// </summary>
        public bool Continuous;

        /// <summary>
        /// 데이터가 연속될 경우 반복되는 횟수를 나타내는 변수의 이름입니다.
        /// </summary>
        public string ContinueCountTarget;

        public SerialData(int Index)
        {
            this.Index = Index;
            this.LeadSpace = 0;
            this.TrailSpace = 0;
            this.Continuous = false;
            this.ContinueCountTarget = null;
        }
    }
}
