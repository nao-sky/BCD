using System;
using System.Collections.Generic;
using System.Text;
using static System.Math;

namespace BCDLib
{
    public struct Once
    {
        public readonly static Once Zero = new Once();
        public readonly static Once Ten = new Once() { Val = 10 };

        public byte Val;
        public sbyte Carry;
        
        public void ValueAdd(int val)
        {
            int a = this.Val + val;

            int c = a / 10;

            Carry += (sbyte)c;

            this.Val = (byte)(a % 10);

        }

        public void ValueSub(int val)
        {
            int a = this.Val - val;

            int c = a < 0 ? Abs(a) : 0;

            Carry -= (sbyte)c;

            this.Val = (byte)((10+a) % 10);
        }
        public static Once operator + (Once left, Once right)
        {
            int a = left.Val + right.Val;

            sbyte b = (sbyte)(a / 10);

            byte c = (byte)(a % 10);

            return new Once() { Val = c, Carry = b };
        }

        public static Once operator -(Once left, Once right)
        {
            int a = left.Val - right.Val;

            byte b = (byte)(Abs((10+a) % 10));

            sbyte c = (sbyte)(a < 0 ? -1 : 0);

            return new Once() { Val = b, Carry = c };
        }

        public static Once operator * (Once left, Once right)
        {
            int a = left.Val * right.Val;

            sbyte b = (sbyte)(a / 10);

            byte c = (byte)(a % 10);

            return new Once() { Val = c, Carry = b };
        }

        public static Once operator / (Once left, Once right)
        {
            if (right.Val == 0)
            {
                return new Once() { Val = 0, Carry = (sbyte)left.Val };
            }
            else
            {
                if(left.Val < right.Val)
                {
                    return new Once() { Val = 0, Carry = (sbyte)(-right.Val) };
                }
                else
                {
                    int a = left.Val / right.Val;
                    int b = left.Val % right.Val;

                    return new Once() { Val = (byte)a, Carry = (sbyte)b };
                }
            }
        }

        public override string ToString()
        {
            return $"val={this.Val} carry={this.Carry}";
        }
    }
}
