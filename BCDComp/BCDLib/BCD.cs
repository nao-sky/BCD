using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BCDLib
{
    public class BCDException : Exception
    {
        public readonly BCD[] Obj;

        public BCDException (string message, Exception exception) : base (message, exception)
        {
        }

        public BCDException(string message, BCD left, BCD right,Exception exception) : base(message, exception)
        {
            this.Obj = new BCD[] { left, right };
        }

        public BCDException(string message,BCD val,Exception exception) : base(message, exception)
        {
            this.Obj = new BCD[] { val };
        }
    }

    public class BCD : ICloneable
    {
        public static BCD One
        {
            get
            {
                return BCD.Parse("1");
            }
        }
        public static BCD Zero
        {
            get
            {
                return new BCD();
            }
        }

        private ExtendLinkedArray values = null;

        private ExtendLinkedArray BCDbyteValue
        {
            get
            {
                if (values == null)
                    values = new ExtendLinkedArray();
                return values;
            }
        }

        private BCD _rem = null;

        public BCD Rem
        {
            get
            {
                if (_rem is null)
                    _rem = new BCD();

                return _rem;
            }
            set
            {
                _rem = value;
            }
        }


        private int sign = 0;
        public int Sign
        {
            get
            {
                return sign;
            }
        }

        private int maxLen=1;
        private int MaxLen
        {
            get
            {
                return Math.Max(maxLen, BCDbyteValue.Count);
            }
            set
            {
                maxLen = value;
            }
        }

        private BCD()
        {}

        private void SetSign(int sign)
        {
            this.sign = sign;
        }

        public static BCD operator +(BCD l, BCD r)
        {
            try
            {
                int maxLen = Math.Max(l.Length, r.Length);
                l.MaxLen = maxLen;
                r.MaxLen = maxLen;


                if (l.Sign < 0)
                {
                    if (r.Sign < 0)
                    {
                        BCD a = Abs(l) + Abs(r);

                        a.SetSign(-1);

                        return a;
                    }
                    else
                    {
                        BCD a, b, c;

                        b = Abs(l);
                        c = Abs(r);

                        if (b == c)
                            return new BCD();
                        if (b > c)
                        {
                            a = b - c;
                            a.SetSign(l.Sign);
                        }
                        else
                        {
                            a = c - b;
                            a.SetSign(r.Sign);
                        }

                        return a;
                    }
                }
                else
                {
                    if (r.Sign < 0)
                    {
                        BCD newR = Abs(r);

                        return l - newR;
                    }
                    else
                    {
                        BCD ret = new BCD
                        {
                            MaxLen = maxLen
                        };

                        int carry = 0;
                        for (int i = maxLen - 1; i >= 0; i--)
                        {
                            Once a = l[i] + r[i];
                            a.ValueAdd(carry);

                            ret[i] = a;

                            carry = a.Carry;
                        }

                        if (carry > 0)
                        {
                            ret.MaxLen++;
                            ret[0] = new Once() { Val = (byte)carry };
                        }

                        return ret;
                    }
                }
            }
            catch(Exception ex)
            {
                throw new BCDException("operator + error.", l, r, ex);
            }
        }

        public static BCD operator -(BCD l, BCD r)
        {
            try
            {
                int maxLen = Math.Max(l.Length, r.Length);

                l.MaxLen = maxLen;
                r.MaxLen = maxLen;

                if (l.Sign < 0)
                {
                    if (r.Sign < 0)
                    {
                        BCD a = Abs(l) + Abs(r);
                        a.SetSign(-1);

                        return a;
                    }
                    else
                    {
                        return r + Abs(l);
                    }
                }
                else
                {
                    if (r.Sign < 0)
                    {
                        return l + Abs(r);
                    }
                    else
                    {
                        BCD ret = BCD.Zero;
                        ret.MaxLen = maxLen;

                        if (l > r)
                        {
                            int carry = 0;
                            for (int i = maxLen - 1; i >= 0; i--)
                            {
                                Once a = l[i] - r[i];
                                a.ValueSub(Math.Abs(carry));

                                ret[i] = a;

                                carry = a.Carry;
                            }

                            if (carry != 0)
                            {
                                ret[0].ValueSub(Math.Abs(carry));
                            }

                            return ret;
                        }
                        else
                        {
                            if (l == r)
                                return BCD.Zero;

                            BCD a = r - l;
                            a.SetSign(-1);
                            return a;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                throw new BCDException("operator - error.", l, r, ex);
            }
        }

        public static BCD operator *(BCD l, BCD r)
        {
            try
            {
                int maxLen = Math.Max(l.Length, r.Length);
                l.BCDbyteValue.VirtualCount = maxLen;
                r.BCDbyteValue.VirtualCount = maxLen;


                if (l.Sign < 0)
                {
                    if (r.Sign < 0)
                    {
                        return Abs(l) * Abs(r);
                    }
                    else
                    {
                        BCD a, b, c;

                        b = Abs(l);
                        c = Abs(r);

                        a = b * c;

                        a.SetSign(-1);

                        return a;
                    }
                }
                else
                {
                    if (r.Sign < 0)
                    {
                        BCD a, b, c;

                        b = Abs(l);
                        c = Abs(r);

                        a = b * c;

                        a.SetSign(-1);

                        return a;
                    }
                    else
                    {
                        if (IsZero(l) || IsZero(r))
                            return new BCD();

                        BCD gre = l > r ? l : r;
                        if (gre != l)
                        {
                            r = l;
                            l = gre;
                        }

                        BCD ans = new BCD();

                        IEnumerator<Once> re = r.BCDbyteValue.GetReverseEnumerator();
                        int digit = 0;


                        while (re.MoveNext())
                        {
                            BCD m1 = new BCD
                            {
                                MaxLen = maxLen
                            };

                            int carry = 0;
                            int idx = maxLen - 1;

                            Once m = re.Current;
                            if (m.Val == Once.Zero.Val)
                            {
                                digit++;
                                continue;
                            }

                            IEnumerator<Once> le = l.BCDbyteValue.GetReverseEnumerator();

                            while (le.MoveNext())
                            {
                                Once a = le.Current * m;

                                a.ValueAdd(carry);

                                m1[idx--] = a;

                                carry = a.Carry;
                            }

                            if (carry > 0)
                            {
                                if (idx >= 0)
                                {
                                    m1[idx] = new Once() { Val = (byte)carry };
                                }
                                else
                                {
                                    m1.BCDbyteValue.Insert(0, new Once() { Val = (byte)carry });
                                }
                            }

                            if (!BCD.IsZero(m1))
                            {
                                BCD tmp = Multiply10(m1, digit++);
                                ans += tmp;
                            }
                        }
                        return ans;
                    }
                }
            }
            catch(Exception ex)
            {
                throw new BCDException("operator * error.", l, r, ex);
            }
        }

        public static BCD operator / (BCD l, BCD r)
        {
            try
            {
                if (r == BCD.Zero)
                    throw new DivideByZeroException();

                if (l.Sign < 0)
                {
                    if (r.Sign < 0)
                    {
                        BCD cl = Abs(l);
                        BCD cr = Abs(r);
                        BCD ans3 = cl / cr;
                        ans3.Rem.SetSign(-1);
                        return ans3;
                    }
                    else
                    {
                        BCD cl = Abs(l);
                        BCD cr = Abs(r);
                        BCD ans = cl / cr;
                        ans.SetSign(-1);
                        ans.Rem.SetSign(-1);

                        return ans;
                    }
                }
                else
                {
                    if (r.Sign < 0)
                    {
                        BCD cl = Abs(l);
                        BCD cr = Abs(r);
                        BCD ans2 = cl / cr;
                        ans2.SetSign(-1);
                        ans2.Rem.SetSign(0);

                        return ans2;
                    }

                    if (l < r)
                    {
                        BCD ans1 = BCD.Zero;
                        ans1.Rem = l.Clone();
                        return ans1;
                    }

                    BCD tmp = r.Clone();
                    BCD ans = BCD.Zero;

                    if (l == tmp)
                        return BCD.One;
                    else
                    {
                        bool isEnd = false;
                        BCD ll = l.Clone();

                        while (!isEnd)
                        {
                            int dec = 0;
                            BCD tmp2 = r.Clone();

                            while (ll > tmp2)
                            {
                                tmp2.BCDbyteValue.Add(Once.Zero);
                                dec++;
                            }

                            if (ll != tmp2)
                            {
                                tmp2.BCDbyteValue.LastOut(out Once dum);
                                dec--;
                            }

                            tmpCount = 0;
                            BCD a = BCD.Zero;

                            while (ll > a)
                            {
                                a += tmp2;
                                tmpCount++;
                            }

                            if (ll == a)
                            {
                                ans.BCDbyteValue.Add(new Once() { Val = (byte)tmpCount });
                            }
                            else
                            {
                                a -= tmp2;
                                tmpCount--;
                                tmpCount = tmpCount < 0 ? 0 : tmpCount;

                                ans.BCDbyteValue.Add(new Once() { Val = (byte)tmpCount });
                            }

                            ll -= a;

                            if (ll < r)
                            {
                                for (int i = 0; dec > i; i++)
                                    ans.BCDbyteValue.Add(Once.Zero);

                                isEnd = true;
                                ans.Rem = ll;
                            }
                        }

                        return ans;
                    }

                }
            }
            catch(Exception ex)
            {
                throw new BCDException("operator / error.", l, r, ex);
            }
        }

        private static int tmpCount;
        public static bool operator > (BCD l, BCD r)
        {
            try
            {
                int maxLen = Math.Max(l.MaxLen, r.MaxLen);
                l.MaxLen = maxLen;
                r.MaxLen = maxLen;

                if (l.Sign < 0)
                {
                    if (r.Sign < 0)
                    {
                        bool gt = false;
                        for (int i = 0; maxLen > i; i++)
                        {
                            if (l[i].Val > r[i].Val)
                            {
                                gt = true;
                                break;
                            }
                        }

                        return !gt;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    for (int i = 0; maxLen > i; i++)
                    {
                        if (l[i].Val > r[i].Val)
                        {
                            return true;
                        }
                        else if (l[i].Val < r[i].Val)
                            return false;
                    }

                    return false;
                }
            }
            catch(Exception ex)
            {
                throw new BCDException("operator > error.", l, r, ex);
            }
        }

        public static bool operator <(BCD l, BCD r)
        {
            try
            {
                if (l == r)
                    return false;
                else
                    return !(l > r);
            }
            catch(Exception ex)
            {
                throw new BCDException("operator < error.", l, r, ex);
            }
        }

        public static bool operator ==(BCD l, BCD r)
        {
            try
            {
                if (l.Sign != r.Sign)
                    return false;

                int maxLen = Math.Max(l.MaxLen, r.MaxLen);
                l.MaxLen = maxLen;
                r.MaxLen = maxLen;

                for (int i = 0; maxLen > i; i++)
                {
                    if (l[i].Val != r[i].Val)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                throw new BCDException("operator == error.", l, r, ex);
            }
        }

        public static bool operator != (BCD l, BCD r)
        {
            try
            { 
                return !(l == r);
            }
            catch (Exception ex)
            {
                throw new BCDException("operator != error.", l, r, ex);
            }
        }

        public static bool IsZero(BCD bcd)
        {
            try
            {
                if (bcd is null)
                    return true;
                else if (bcd._rem is null)
                {
                    return false;
                }
                else
                {
                    foreach (Once once in bcd.BCDbyteValue)
                    {
                        if (once.Val != 0)
                            return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new BCDException("IsZero error.", bcd, ex);
            }
        }

        public static BCD Abs(BCD bcd)
        {
            try
            {
                BCD ret = bcd.Clone();
                ret.SetSign(0);
                return ret;
            }
            catch (Exception ex)
            {
                throw new BCDException("Abs error.", bcd, ex);
            }
        }

        public static BCD Multiply10(BCD org, int digit)
        {
            try
            {
                BCD ans = org.Clone();

                for (int i = 0; digit > i; i++)
                {
                    ans.BCDbyteValue.Add(Once.Zero);
                }
                ans.MaxLen = ans.BCDbyteValue.Count;

                return ans;
            }
            catch (Exception ex)
            {
                throw new BCDException("Multiply10 error.", org, ex);
            }
        }

        private Once this[int vindex]
        {
            get
            {
                try
                {
                    int sub = MaxLen - BCDbyteValue.Count;

                    int index = vindex - sub;

                    if (BCDbyteValue.Count > index && index >= 0)
                        return BCDbyteValue[index];
                    else
                        return new Once() { Val = 0 };
                }
                catch (Exception ex)
                {
                    throw new BCDException("this_get error.", null, ex);
                }
            }
            set
            {
                try
                {
                    int sub = MaxLen - BCDbyteValue.Count;
                    //int index = vindex - sub;

                    if (sub > 0)
                    {
                        for (int i = 0; sub > i; i++)
                            BCDbyteValue.Insert(0, Once.Zero);

                        BCDbyteValue[vindex] = value;
                    }
                    else
                        BCDbyteValue[vindex] = value;
                }
                catch (Exception ex)
                {
                    throw new BCDException("this_set error.", null, ex);
                }
            }
        }

        public int Length
        {
            get
            {
                return BCDbyteValue.Count;
            }
        }

        public static BCD Parse(string val)
        {
            try
            {
                if (!Regex.IsMatch(val, "[+-]?[0-9]+"))
                    throw new Exception("parse error.");

                BCD ret = new BCD();

                if (val.StartsWith("-"))
                {
                    ret.SetSign(-1);

                    for (int i = 1; val.Length > i; i++)
                        ret.BCDbyteValue.Add(new Once() { Val = (byte)(val[i] - '0') });

                    ret.MaxLen = val.Length - 1;
                    return ret;
                }
                else if (val.StartsWith("+"))
                {
                    ret.SetSign(0);

                    for (int i = 1; val.Length > i; i++)
                        ret.BCDbyteValue.Add(new Once() { Val = (byte)(val[i] - '0') });
                    ret.MaxLen = val.Length - 1;

                    return ret;

                }
                else
                {
                    for (int i = 0; val.Length > i; i++)
                        ret.BCDbyteValue.Add(new Once() { Val = (byte)(val[i] - '0') });

                    return ret;
                }
            }
            catch (Exception ex)
            {
                throw new BCDException("Parse error.", null, ex);
            }
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public BCD Clone()
        {
            try
            {
                BCD a = new BCD();

                a.SetSign(this.Sign);
                a.MaxLen = this.BCDbyteValue.Count;

                a.BCDbyteValue.AddRange(this.BCDbyteValue.ToArray());

                return a;
            }
            catch (Exception ex)
            {
                throw new BCDException("Clone error.", this, ex);
            }
        }

        public override bool Equals(object obj)
        {
            BCD a = obj as BCD;

            return a != null && this == a;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 0;

                foreach (Once once in this.BCDbyteValue)
                    h += once.Val;

                return h;
            }
        }

        public override string ToString()
        {
            if (IsZero(this))
                return "0";

            StringBuilder sb = new StringBuilder();

            bool isStarted = false;

            foreach(Once once in this.BCDbyteValue)
            {
                if (once.Val == 0 && !isStarted)
                { }
                else
                {
                    isStarted = true;
                    sb.Append(once.Val);
                }
            }

            if (sb.Length < 1)
            {
                sb.Append("0");
                if(!(_rem is null))
                    sb.Append(string.Format(" REM: {0}{1}", (_rem.Sign < 0 ? "-" : ""), _rem.ToString())); ; ;
                return sb.ToString();
            }
            else
            {
                string s = (this.Sign < 0 ? "-" : "") + sb.ToString();
                if (!(_rem is null))
                {
                    string sr = string.Format("REM: {0}{1}", (_rem.Sign < 0 ? "-" : ""), _rem.ToString());
                    return s + " " + sr;
                }
                else
                {
                    return s;
                }
            }
        }
    }
}
