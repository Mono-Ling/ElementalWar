using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ElementUtility
{
    public static class Content
    {
        public const float STRONG = 2f;
        public const float WEAK = 1f;
        public const float ATTENUA_SPEED = 0.8f;// c/s
        public const float ATTENUA_DELAY = 1f;
    }
    public static bool TryToElementType(int number, out ElementType elementType)
    {
        var element = (ElementType)number;
        elementType = default;
        if (!Enum.IsDefined(typeof(ElementType), element))
            return false;
        elementType = element;
        return true;
    }
    public static int ToNumber(ElementType elementType)
    => (int)elementType;
}
