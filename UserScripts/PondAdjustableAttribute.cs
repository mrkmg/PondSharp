using System;

namespace PondSharp.UserScripts
{
    /// <summary>
    /// Defines default parameters for the entities creation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PondAdjustableAttribute : Attribute
    {
        /// <summary>
        /// Override the name Displayed in the selector. Default: The property name
        /// </summary>
        public string Name;
        
        /// <summary>
        /// The minimum value. Default: 0
        /// </summary>
        public int Min = 0;
        
        /// <summary>
        /// The maximum value. Default: 100
        /// </summary>
        public int Max = 100;
    }
}