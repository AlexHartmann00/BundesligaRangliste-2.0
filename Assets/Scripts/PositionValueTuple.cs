using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionValueTuple
{
    float left, right, center;
     public PositionValueTuple(float l, float c, float r)
    {
        left = l;
        right = r;
        center = c;
    }

    public PositionValueTuple(float x)
    {
        left = x;
        right = x;
        center = x;
    }

    public float Left()
    {
        return left;
    }

    public float Center()
    {
        return center;
    }    

    public float Right()
    {
        return right;
    }

    public float Max()
    {
        return Mathf.Max(new float[]{ left,center,right});
    }

    public float Min()
    {
        return Mathf.Min(new float[]{ left,center,right});
    }

    public float Average()
    {
        return (left + center + right) / 3.0f;
    }

    public float Sum()
    {
        return left + center + right;
    }

    public void Set(float x)
    {
        this.left = x;
        this.right = x;
        this.center = x;
    }

    public float AverageRoot()
    {
        return (Mathf.Sqrt(left) + Mathf.Sqrt(center) + Mathf.Sqrt(right)) / 3.0f;
    }

    public float AverageSquare()
    {
        return (Mathf.Pow(left, 2) + Mathf.Pow(center, 2) + Mathf.Pow(right, 2)) / 3.0f;
    }

    public static PositionValueTuple operator +(PositionValueTuple x, PositionValueTuple y)
    {
        PositionValueTuple n = new PositionValueTuple(0, 0, 0);
        n.left = x.Left() + y.Left();
        n.center = x.Center() + y.Center();
        n.right = x.Right() + y.Right();
        return n;
    }

    public static PositionValueTuple operator *(PositionValueTuple x, float y)
    {
        PositionValueTuple n = new PositionValueTuple(0, 0, 0);
        n.left = y* x.Left();
        n.center = y* x.Center();
        n.right = y * x.Right();
        return n;
    }

    public static PositionValueTuple operator /(PositionValueTuple x, float y)
    {
        PositionValueTuple n = new PositionValueTuple(0, 0, 0);
        n.left = x.Left() / y;
        n.center = x.Center() / y;
        n.right = x.Right() / y;
        return n;

    }

    public static PositionValueTuple operator +(PositionValueTuple x, float y)
    {
        PositionValueTuple n = new PositionValueTuple(0, 0, 0);
        n.left = x.Left() + y;
        n.center = x.Center() + y;
        n.right= x.Right() + y;
        return n;
    }

    public static PositionValueTuple operator -(PositionValueTuple x, float y)
    {
        return x + (-y);
    }

    public static PositionValueTuple operator /(PositionValueTuple x, PositionValueTuple y)
    {
        PositionValueTuple n = new PositionValueTuple(0, 0, 0);
        n.left = x.left / y.left;
        n.center = x.center / y.center;
        n.right = x.right / y.right;
        return n;
    }

    public PositionValueTuple ApplySkewness(float skew)
    {
        PositionValueTuple ret = this;
        ret.left *= 1f - skew;
        ret.center *= 1f - 2f*Mathf.Abs(skew - 0.5f);
        ret.right *= skew;
        return ret;
    }
    public override string ToString()
    {
        return "(" + Mathf.Round(left) + "," + Mathf.Round(center) + "," + Mathf.Round(right) + ")";
    }
}
