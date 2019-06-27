using System;
using System.Collections.Generic;
using System.Text;

namespace LogUserAction
{
    /// <summary>
    /// Using for Entity table class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LogEntityAttribute : Attribute
    {
    }
}