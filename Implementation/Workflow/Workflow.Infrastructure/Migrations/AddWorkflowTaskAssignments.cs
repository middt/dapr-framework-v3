using System;
using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddWorkflowTaskAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WorkflowTaskAssignments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                StateId = table.Column<Guid>(type: "uuid", nullable: true),
                TransitionId = table.Column<Guid>(type: "uuid", nullable: true),
                FunctionId = table.Column<Guid>(type: "uuid", nullable: true),
                Trigger = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkflowTaskAssignments", x => x.Id);
                table.ForeignKey(
                    name: "FK_WorkflowTaskAssignments_WorkflowTasks_TaskId",
                    column: x => x.TaskId,
                    principalTable: "WorkflowTasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_WorkflowTaskAssignments_WorkflowStates_StateId",
                    column: x => x.StateId,
                    principalTable: "WorkflowStates",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_WorkflowTaskAssignments_WorkflowTransitions_TransitionId",
                    column: x => x.TransitionId,
                    principalTable: "WorkflowTransitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_WorkflowTaskAssignments_WorkflowFunctions_FunctionId",
                    column: x => x.FunctionId,
                    principalTable: "WorkflowFunctions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowTaskAssignments_TaskId",
            table: "WorkflowTaskAssignments",
            column: "TaskId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowTaskAssignments_StateId",
            table: "WorkflowTaskAssignments",
            column: "StateId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowTaskAssignments_TransitionId",
            table: "WorkflowTaskAssignments",
            column: "TransitionId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowTaskAssignments_FunctionId",
            table: "WorkflowTaskAssignments",
            column: "FunctionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WorkflowTaskAssignments");
    }
} 