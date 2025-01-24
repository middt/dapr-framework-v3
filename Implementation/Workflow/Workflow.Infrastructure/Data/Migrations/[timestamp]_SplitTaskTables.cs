using Microsoft.EntityFrameworkCore.Migrations;

public partial class SplitTaskTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create new tables for each task type
        migrationBuilder.CreateTable(
            name: "HumanTasks",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Title = table.Column<string>(maxLength: 200),
                Instructions = table.Column<string>(),
                AssignedTo = table.Column<string>(maxLength: 100),
                DueDate = table.Column<DateTime>(nullable: true),
                Form = table.Column<string>(type: "jsonb"),
                ReminderIntervalMinutes = table.Column<int>(),
                EscalationTimeoutMinutes = table.Column<int>(),
                EscalationAssignee = table.Column<string>(maxLength: 100)
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

        // Similar CreateTable calls for other task types...

        // Migrate existing data
        migrationBuilder.Sql(@"
            INSERT INTO ""HumanTasks"" (""Id"", ""Title"", ""Instructions"", ""AssignedTo"", ""DueDate"", 
                ""Form"", ""ReminderIntervalMinutes"", ""EscalationTimeoutMinutes"", ""EscalationAssignee"")
            SELECT ""Id"", ""Title"", ""Instructions"", ""AssignedTo"", ""DueDate"", 
                ""Form"", ""ReminderIntervalMinutes"", ""EscalationTimeoutMinutes"", ""EscalationAssignee""
            FROM ""WorkflowTasks""
            WHERE ""Type"" = 1;
            
            -- Similar INSERT statements for other task types...
        ");

        // Remove task-specific columns from WorkflowTasks
        migrationBuilder.DropColumn(name: "Title", table: "WorkflowTasks");
        migrationBuilder.DropColumn(name: "Instructions", table: "WorkflowTasks");
        // ... drop other task-specific columns
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Add back task-specific columns to WorkflowTasks
        migrationBuilder.AddColumn<string>(name: "Title", table: "WorkflowTasks");
        migrationBuilder.AddColumn<string>(name: "Instructions", table: "WorkflowTasks");
        // ... add other task-specific columns

        // Migrate data back to WorkflowTasks
        migrationBuilder.Sql(@"
            UPDATE ""WorkflowTasks"" wt
            SET ""Title"" = ht.""Title"",
                ""Instructions"" = ht.""Instructions"",
                -- ... other columns
            FROM ""HumanTasks"" ht
            WHERE wt.""Id"" = ht.""Id"";
            
            -- Similar UPDATE statements for other task types
        ");

        // Drop task-specific tables
        migrationBuilder.DropTable(name: "HumanTasks");
        migrationBuilder.DropTable(name: "DaprBindingTasks");
        migrationBuilder.DropTable(name: "DaprPubSubTasks");
        migrationBuilder.DropTable(name: "DaprServiceTasks");
        migrationBuilder.DropTable(name: "HttpTasks");
    }
} 