

namespace HotelManagement.TestingWorkspace.Utilities;

public class E2ETestLogger
{
    private readonly string _seedFile;
    private readonly string _experimentFile;


    public E2ETestLogger(string workspaceRoot)
    {
        _seedFile = Path.Combine(workspaceRoot, "seed_experiment.md");
        _experimentFile = Path.Combine(workspaceRoot, "experiment_testing.md");

        // Initialize files
        File.WriteAllText(_seedFile, "# Data Seeding Report\n\nThis file logs the automated seeding of edge-case test data.\n\n");
        File.WriteAllText(_experimentFile, "# Automated State Mutation Report\n\nThis file chronologically logs automated E2E testing scenarios.\n\n");
    }

    public void LogSeedData(string entity, string payload, string responseStatus, string resultDetails)
    {
        var log = $"### Seeded: {entity}\n- **Payload Sent:** `{payload}`\n- **HTTP Status:** `{responseStatus}`\n- **Result:** {resultDetails}\n\n";
        File.AppendAllText(_seedFile, log);
    }

    public void LogExperimentStep(string time, string actor, string action, string endpoint, string payload, string expected, string actual, string dataAffected)
    {
        var log = $"### [{time}] {actor} -> {action}\n" +
                  $"- **Endpoint Triggered:** `{endpoint}`\n" +
                  $"- **Payload:** `{payload}`\n" +
                  $"- **Expected Result:** `{expected}`\n" +
                  $"- **Actual Result:** `{actual}`\n" +
                  $"- **Data Affected:** {dataAffected}\n\n---\n\n";
        File.AppendAllText(_experimentFile, log);
    }
}
