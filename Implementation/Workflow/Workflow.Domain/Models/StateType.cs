namespace Workflow.Domain.Models;

public enum StateType
{
    // Main state types
    Initial,        // Starting state of the workflow
    Intermediate,   // Middle states where work is being done
    Finish,         // End states of the workflow
    Subflow,        // State that executes another workflow
}

public enum StateSubType
{
    None,           // No specific subtype
    Success,        // Successful completion
    Error,          // Error condition
    Terminated,     // Manually terminated
    Suspended       // Temporarily suspended
}