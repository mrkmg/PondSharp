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
        public double Min = 0;
        
        /// <summary>
        /// The maximum value. Default: 100
        /// </summary>
        public double Max = 100;

        /// <summary>
        /// The step value. Default: 1
        /// </summary>
        public double Step = 1;
        
        /// <summary>
        /// The input type. Default: Slider
        /// </summary>
        public InputType Type = InputType.Slider;

        /// <summary>
        /// Show this selector on inherited entities. Default: true
        /// </summary>
        public bool Inheritable = true;
    }
}

/// <summary>
/// The input type
/// </summary>
public enum InputType
{
    /// <summary>
    /// A number input
    /// </summary>
    FreeForm,
    /// <summary>
    /// A range input
    /// </summary>
    Slider,
    /// <summary>
    /// A HTML5 color input
    /// </summary>
    RgbColorPicker,
}