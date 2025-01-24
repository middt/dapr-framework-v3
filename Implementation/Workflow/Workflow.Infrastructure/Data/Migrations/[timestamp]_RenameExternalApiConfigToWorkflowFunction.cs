using Microsoft.EntityFrameworkCore.Migrations;

namespace Workflow.Infrastructure.Data.Migrations
{
    public partial class RenameExternalApiConfigToWorkflowFunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "ExternalApiConfigs",
                newName: "WorkflowFunctions");

            // Update foreign key relationship name if it exists
            if (migrationBuilder.ActiveProvider == "Npgsql")
            {
                migrationBuilder.Sql(@"
                    ALTER TABLE ""WorkflowFunctions""
                    RENAME CONSTRAINT ""FK_ExternalApiConfigs_WorkflowDefinitions_WorkflowDefinitionId""
                    TO ""FK_WorkflowFunctions_WorkflowDefinitions_WorkflowDefinitionId"";
                ");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert foreign key relationship name if it exists
            if (migrationBuilder.ActiveProvider == "Npgsql")
            {
                migrationBuilder.Sql(@"
                    ALTER TABLE ""WorkflowFunctions""
                    RENAME CONSTRAINT ""FK_WorkflowFunctions_WorkflowDefinitions_WorkflowDefinitionId""
                    TO ""FK_ExternalApiConfigs_WorkflowDefinitions_WorkflowDefinitionId"";
                ");
            }

            migrationBuilder.RenameTable(
                name: "WorkflowFunctions",
                newName: "ExternalApiConfigs");
        }
    }
} 