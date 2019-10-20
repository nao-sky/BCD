using System;
using System.Collections.Generic;
using System.Text;
using LinkedArray;

namespace BCDLib
{
    public class ExtendLinkedArray : LinkedArray<Once>
    {
        private int virtualCount;

        public int VirtualCount
        {
            get
            {
                return Math.Max(virtualCount, base.Count);
            }
            set
            {
                if (base.Count < value)
                    virtualCount = value;
            }
        }


        public override IEnumerator<Once> GetEnumerator()
        {
            int sub = VirtualCount - base.Count;

            for (int i = 0; sub > i; i++)
                yield return Once.Zero;

            IEnumerator<Once> enm = base.GetEnumerator();

            while (enm.MoveNext())
                yield return enm.Current;
        }

        public override IEnumerator<Once> GetReverseEnumerator()
        {
            //int sub = VirtualCount - base.Count;

            IEnumerator<Once> enm = base.GetReverseEnumerator();

            while (enm.MoveNext())
                yield return enm.Current;

            //for (int i = 0; sub > i; i++)
            //    yield return Once.Zero;
        }

    }
}
