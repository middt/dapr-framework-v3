# Workflow Engine

## Overview
The Workflow Engine provides a flexible system for defining and executing business workflows. It supports state management, task execution, and version control.

## Core Components
1. **Definitions**: Versioned workflow templates
2. **States**: Nodes with type/subtype classification
3. **Transitions**: State-to-state movement rules
4. **Instances**: Running workflow executions
5. **Tasks**: State-specific actions

## Key Features
- **State Management**: Track workflow progress through defined states
- **Task Execution**: Execute state-specific tasks with configurable parameters
- **Version Control**: Manage workflow versions and client compatibility
- **Subflows**: Support for nested workflows
- **Audit Trail**: Track state changes and task execution

## Workflow Lifecycle
1. Create workflow definition
2. Define states and transitions
3. Configure state-specific tasks
4. Create workflow instances
5. Execute tasks and transition states


`dapr run --app-id "workflow-api" --app-port "5007" --dapr-grpc-port "50001" --dapr-http-port "3500" --components-path "./AppHost/components" --config "./AppHost/config.yaml"`

