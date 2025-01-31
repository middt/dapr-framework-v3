using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InstanceAndConfig_Changed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstanceTasks_WorkflowInstances_WorkflowInstanceId",
                table: "WorkflowInstanceTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstanceTasks_WorkflowStates_StateId",
                table: "WorkflowInstanceTasks");

            migrationBuilder.DropColumn(
                name: "Config",
                table: "WorkflowTaskAssignment");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkflowInstanceId",
                table: "WorkflowInstanceTasks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "StateId",
                table: "WorkflowInstanceTasks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstanceTasks_WorkflowInstances_WorkflowInstanceId",
                table: "WorkflowInstanceTasks",
                column: "WorkflowInstanceId",
                principalTable: "WorkflowInstances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstanceTasks_WorkflowStates_StateId",
                table: "WorkflowInstanceTasks",
                column: "StateId",
                principalTable: "WorkflowStates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstanceTasks_WorkflowInstances_WorkflowInstanceId",
                table: "WorkflowInstanceTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstanceTasks_WorkflowStates_StateId",
                table: "WorkflowInstanceTasks");

            migrationBuilder.AddColumn<string>(
                name: "Config",
                table: "WorkflowTaskAssignment",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkflowInstanceId",
                table: "WorkflowInstanceTasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StateId",
                table: "WorkflowInstanceTasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstanceTasks_WorkflowInstances_WorkflowInstanceId",
                table: "WorkflowInstanceTasks",
                column: "WorkflowInstanceId",
                principalTable: "WorkflowInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstanceTasks_WorkflowStates_StateId",
                table: "WorkflowInstanceTasks",
                column: "StateId",
                principalTable: "WorkflowStates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
