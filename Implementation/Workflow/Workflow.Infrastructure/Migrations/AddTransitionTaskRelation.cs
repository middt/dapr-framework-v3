using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddTransitionTaskRelation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "TaskId",
            table: "WorkflowTransitions",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_WorkflowTransitions_TaskId",
            table: "WorkflowTransitions",
            column: "TaskId");

        migrationBuilder.AddForeignKey(
            name: "FK_WorkflowTransitions_WorkflowTasks_TaskId",
            table: "WorkflowTransitions",
            column: "TaskId",
            principalTable: "WorkflowTasks",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_WorkflowTransitions_WorkflowTasks_TaskId",
            table: "WorkflowTransitions");

        migrationBuilder.DropIndex(
            name: "IX_WorkflowTransitions_TaskId",
            table: "WorkflowTransitions");

        migrationBuilder.DropColumn(
            name: "TaskId",
            table: "WorkflowTransitions");
    }
} 