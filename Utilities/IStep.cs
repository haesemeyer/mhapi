
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHApi.Utilities
{

    /// <summary>
    /// Determines if a protocol step describes a goto command or experimental condition
    /// </summary>
    public enum ProtocolStepType { Action = 0, Goto = 1 };

    /// <summary>
    /// Provides properties describing a step-type and goto descriptors in a protocol
    /// </summary>
    public interface IStep
    {
        /// <summary>
        /// The type of the protocol step
        /// </summary>
        ProtocolStepType StepType { get; set; }

        /// <summary>
        /// The step in the protocol to which the goto points
        /// </summary>
        int GotoStep { get; set; }

        /// <summary>
        /// The number of times the goto should be repeated
        /// </summary>
        uint RepeatCount { get; set; }
 
    }
}
