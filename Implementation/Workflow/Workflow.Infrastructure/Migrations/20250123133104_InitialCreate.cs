using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkflowDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StateType = table.Column<int>(type: "integer", nullable: false),
                    SubType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStates_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubflowConfig",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WaitForCompletion = table.Column<bool>(type: "boolean", nullable: false),
                    _inputMapping = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    _outputMapping = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubflowConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubflowConfig_WorkflowDefinitions_SubflowDefinitionId",
                        column: x => x.SubflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubflowConfig_WorkflowStates_StateId",
                        column: x => x.StateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_WorkflowStates_CurrentStateId",
                        column: x => x.CurrentStateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Config = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Result = table.Column<string>(type: "jsonb", nullable: true),
                    WorkflowStateId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowStates_WorkflowStateId",
                        column: x => x.WorkflowStateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TriggerType = table.Column<int>(type: "integer", nullable: false),
                    _triggerConfig = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowStates_FromStateId",
                        column: x => x.FromStateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitions_WorkflowStates_ToStateId",
                        column: x => x.ToStateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowCorrelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowCorrelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowCorrelations_WorkflowInstances_ParentInstanceId",
                        column: x => x.ParentInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowCorrelations_WorkflowInstances_SubflowInstanceId",
                        column: x => x.SubflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowCorrelations_WorkflowStates_ParentStateId",
                        column: x => x.ParentStateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowHumanTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Assignee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowHumanTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowHumanTasks_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowHumanTasks_WorkflowStates_StateId",
                        column: x => x.StateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceData_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStateData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StateId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    _data = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStateData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStateData_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowStateData_WorkflowStates_StateId",
                        column: x => x.StateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DaprBindingTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BindingName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Operation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaprBindingTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DaprBindingTasks_WorkflowTasks_Id",
                        column: x => x.Id,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DaprHttpEndpointTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EndpointName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Path = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Headers = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaprHttpEndpointTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DaprHttpEndpointTasks_WorkflowTasks_Id",
                        column: x => x.Id,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DaprPubSubTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PubSubName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaprPubSubTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DaprPubSubTasks_WorkflowTasks_Id",
                        column: x => x.Id,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DaprServiceTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MethodName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HttpVerb = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false),
                    QueryString = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaprServiceTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DaprServiceTasks_WorkflowTasks_Id",
                        column: x => x.Id,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HttpTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Method = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Headers = table.Column<string>(type: "jsonb", nullable: false),
                    Body = table.Column<string>(type: "jsonb", nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    ValidateSSL = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HttpTasks_WorkflowTasks_Id",
                        column: x => x.Id,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HumanTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: false),
                    AssignedTo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Form = table.Column<string>(type: "jsonb", nullable: false),
                    ReminderIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    EscalationTimeoutMinutes = table.Column<int>(type: "integer", nullable: false),
                    EscalationAssignee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HumanTasks_WorkflowTasks_Id",
                        column: x => x.Id,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowFunctions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    StateId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponseMapping = table.Column<string>(type: "text", nullable: true),
                    EnrichStateData = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    WorkflowDefinitionId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowFunctions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowFunctions_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WorkflowFunctions_WorkflowDefinitions_WorkflowDefinitionId1",
                        column: x => x.WorkflowDefinitionId1,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowFunctions_WorkflowTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    StateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskName = table.Column<string>(type: "text", nullable: false),
                    TaskType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    Result = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceTasks_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceTasks_WorkflowStates_StateId",
                        column: x => x.StateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceTasks_WorkflowTasks_WorkflowTaskId",
                        column: x => x.WorkflowTaskId,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowViews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Target = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WorkflowVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StateId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowViews_WorkflowDefinitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "WorkflowDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowViews_WorkflowStates_StateId",
                        column: x => x.StateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkflowViews_WorkflowTransitions_TransitionId",
                        column: x => x.TransitionId,
                        principalTable: "WorkflowTransitions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowFunctionStates",
                columns: table => new
                {
                    StatesId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowFunctionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowFunctionStates", x => new { x.StatesId, x.WorkflowFunctionId });
                    table.ForeignKey(
                        name: "FK_WorkflowFunctionStates_WorkflowFunctions_WorkflowFunctionId",
                        column: x => x.WorkflowFunctionId,
                        principalTable: "WorkflowFunctions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowFunctionStates_WorkflowStates_StatesId",
                        column: x => x.StatesId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubflowConfig_StateId",
                table: "SubflowConfig",
                column: "StateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubflowConfig_SubflowDefinitionId",
                table: "SubflowConfig",
                column: "SubflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowCorrelations_ParentInstanceId",
                table: "WorkflowCorrelations",
                column: "ParentInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowCorrelations_ParentStateId",
                table: "WorkflowCorrelations",
                column: "ParentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowCorrelations_SubflowInstanceId",
                table: "WorkflowCorrelations",
                column: "SubflowInstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFunctions_TaskId",
                table: "WorkflowFunctions",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFunctions_WorkflowDefinitionId",
                table: "WorkflowFunctions",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFunctions_WorkflowDefinitionId1",
                table: "WorkflowFunctions",
                column: "WorkflowDefinitionId1");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowFunctionStates_WorkflowFunctionId",
                table: "WorkflowFunctionStates",
                column: "WorkflowFunctionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHumanTasks_StateId",
                table: "WorkflowHumanTasks",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowHumanTasks_WorkflowInstanceId",
                table: "WorkflowHumanTasks",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceData_WorkflowInstanceId",
                table: "WorkflowInstanceData",
                column: "WorkflowInstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_CurrentStateId",
                table: "WorkflowInstances",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_WorkflowDefinitionId",
                table: "WorkflowInstances",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceTasks_StateId",
                table: "WorkflowInstanceTasks",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceTasks_WorkflowInstanceId",
                table: "WorkflowInstanceTasks",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceTasks_WorkflowTaskId",
                table: "WorkflowInstanceTasks",
                column: "WorkflowTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStateData_StateId",
                table: "WorkflowStateData",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStateData_WorkflowInstanceId",
                table: "WorkflowStateData",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStates_WorkflowDefinitionId",
                table: "WorkflowStates",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowStateId",
                table: "WorkflowTasks",
                column: "WorkflowStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_FromStateId",
                table: "WorkflowTransitions",
                column: "FromStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_ToStateId",
                table: "WorkflowTransitions",
                column: "ToStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitions_WorkflowDefinitionId",
                table: "WorkflowTransitions",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowViews_StateId",
                table: "WorkflowViews",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowViews_TransitionId",
                table: "WorkflowViews",
                column: "TransitionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowViews_WorkflowDefinitionId",
                table: "WorkflowViews",
                column: "WorkflowDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DaprBindingTasks");

            migrationBuilder.DropTable(
                name: "DaprHttpEndpointTasks");

            migrationBuilder.DropTable(
                name: "DaprPubSubTasks");

            migrationBuilder.DropTable(
                name: "DaprServiceTasks");

            migrationBuilder.DropTable(
                name: "HttpTasks");

            migrationBuilder.DropTable(
                name: "HumanTasks");

            migrationBuilder.DropTable(
                name: "SubflowConfig");

            migrationBuilder.DropTable(
                name: "WorkflowCorrelations");

            migrationBuilder.DropTable(
                name: "WorkflowFunctionStates");

            migrationBuilder.DropTable(
                name: "WorkflowHumanTasks");

            migrationBuilder.DropTable(
                name: "WorkflowInstanceData");

            migrationBuilder.DropTable(
                name: "WorkflowInstanceTasks");

            migrationBuilder.DropTable(
                name: "WorkflowStateData");

            migrationBuilder.DropTable(
                name: "WorkflowViews");

            migrationBuilder.DropTable(
                name: "WorkflowFunctions");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");

            migrationBuilder.DropTable(
                name: "WorkflowTransitions");

            migrationBuilder.DropTable(
                name: "WorkflowTasks");

            migrationBuilder.DropTable(
                name: "WorkflowStates");

            migrationBuilder.DropTable(
                name: "WorkflowDefinitions");
        }
    }
}
