using System;
using UnityEngine;

namespace Code.Scripts.Attributes
{
    /// <summary>
    /// Marks a public or serialized field in Unity as <b>required</b>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class Required : PropertyAttribute { }
}